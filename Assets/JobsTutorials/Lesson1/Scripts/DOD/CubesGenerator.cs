using Jobs.Common; //这个路径中包括了AutoReturnToPool.cs的代码，检测目的地并释放回对象池
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace Jobs.DOD
{
    [RequireComponent(typeof(BoxCollider))]
    public class CubesGenerator : MonoBehaviour
    {
        public GameObject cubeArchetype = null;
        public GameObject targetArea = null;
        [Range(10, 10000)] public int generationTotalNum = 500;
        [Range(1, 60)] public int generationNumPerTicktime = 5;
        [Range(0.1f, 1.0f)] public float tickTime = 0.2f;
        [HideInInspector]
        public Vector3 generatorAreaSize;
        [HideInInspector]
        public Vector3 targetAreaSize;
        
        public float rotateSpeed = 180.0f;
        public float moveSpeed = 5.0f;
        private TransformAccessArray transformsAccessArray;
        //job
        //private NativeArray<Vector3> randTargetPosArray;
        
        //optimize2， float3是Unity.Mathematics的数据结构，支持在Burst编译时形成SIMD指令，提高运行效率
        private NativeArray<float3> randTargetPosArray;

        //开启collectionChecks时，当外部尝试销毁池内对象时，会触发异常报错
        public bool collectionChecks = true;
        // 对象池
        private ObjectPool<GameObject> pool = null;
        private Transform[] transforms;
        
        private float timer = 0.0f;

        static readonly ProfilerMarker profilerMarker = new ProfilerMarker("CubesMarchWithJob");
        void Start()
        {
            ///创建对象池
            if (pool == null)
                pool = new ObjectPool<GameObject>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool,
                    OnDestroyPoolObject, collectionChecks, 10, generationTotalNum);

            generatorAreaSize = GetComponent<BoxCollider>().size;
            targetAreaSize = targetArea.GetComponent<BoxCollider>().size;
            
            // job
            //randTargetPosArray = new NativeArray<Vector3>(generationTotalNum, Allocator.Persistent);
            //optimize2
            randTargetPosArray = new NativeArray<float3>(generationTotalNum, Allocator.Persistent);

            transforms = new Transform[generationTotalNum];//为啥必须把上限分配好并且初始化完所有cube？这比非jobs的内存占用大
            for (int i = 0; i < generationTotalNum; i++)
            {
                GameObject cube = pool.Get();
                var component = cube.AddComponent<AutoReturnToPool>();//给全部生成的cube挂载上这个脚本，为啥不在editor中给cube挂上？
                //好像editor中挂不挂一样，反正得运行时设置pool参数，也就是下面一句
                component.pool = pool;

                Vector3 randGenerationPos =  transform.position + new Vector3(Random.Range(-generatorAreaSize.x * 0.5f, generatorAreaSize.x * 0.5f),
                    0, Random.Range(-generatorAreaSize.z * 0.5f, generatorAreaSize.z * 0.5f));
                component.generationPos = randGenerationPos;//给AutoReturnToPool脚本设置初始参数，否则没有初始值
                cube.transform.position = randGenerationPos;//给cube设置位置
                
                Vector3 randTargetPos = targetArea.transform.position + new Vector3(Random.Range(-targetAreaSize.x * 0.5f, targetAreaSize.x * 0.5f),
                    0, Random.Range(-targetAreaSize.z * 0.5f, targetAreaSize.z * 0.5f));
                randTargetPosArray[i] =  randTargetPos;//放到nativeArray中
                component.targetPos = randTargetPos;//给AutoReturnToPool脚本设置目的地，用于release

                transforms[i] = cube.transform;
            }
            transformsAccessArray = new TransformAccessArray(transforms);//从transform数组转化成blittable的能用于job的TransformAccessArray
            for (int i = generationTotalNum-1; i >=0; i--)
            {
                pool.Release(transforms[i].gameObject);//为啥要Release？因为这只是初始化！
            }
            timer = 0.0f;
        }

        void Update() //在Update的时候批量 旋转+平移
        {
            using (profilerMarker.Auto())
            {
                //job
                //var autoRotateAndMoveJob = new AutoRotateAndMoveJob();
                //optimize2
                //下5行在实例化Job，并且给job设参数
                var autoRotateAndMoveJob = new AutoRotateAndMoveJobOptimize2();
                autoRotateAndMoveJob.randTargetPosArray = randTargetPosArray;
                autoRotateAndMoveJob.deltaTime = Time.deltaTime;
                autoRotateAndMoveJob.moveSpeed = moveSpeed;
                autoRotateAndMoveJob.rotateSpeed = rotateSpeed;
                JobHandle autoRotateAndMoveJobJobHandle =
                    autoRotateAndMoveJob.Schedule(transformsAccessArray);
                autoRotateAndMoveJobJobHandle.Complete();

                if (timer >= tickTime)
                {
                    GenerateCubes();
                    timer -= tickTime;
                }

                timer += Time.deltaTime;
            }
        }

        private void OnDestroy()
        {
            if(transformsAccessArray.isCreated)
                transformsAccessArray.Dispose();
            randTargetPosArray.Dispose();

            if (pool != null)
                pool.Dispose();
        }
        
        private void GenerateCubes()
        {
            if (!cubeArchetype  || pool == null)
                return;
            for (int i = 0; i < generationNumPerTicktime; ++i)
            {
                if (pool.CountActive < generationTotalNum) //这里是CountActive不是CountAll？和scene：Cubes里的写法不一样
                    //因此CountActive就会在消失后重新创建！
                {
                    /*这里和Scene：Cubes的区别在于，generate后没有重新设置其随机位置、随机目的地，这就导致上限10，单次10的时候
                    每一轮的物体都一样！*/
                    pool.Get();
                }
                else
                {
                    timer = 0;
                    return;
                }
            }
        }

        GameObject CreatePooledItem()
        {
            return Instantiate(cubeArchetype, transform);
        }

        void OnReturnedToPool(GameObject gameObject)
        {
            gameObject.SetActive(false);
        }

        void OnTakeFromPool(GameObject gameObject)
        {
            gameObject.SetActive(true);
        }

        void OnDestroyPoolObject(GameObject gameObject)
        {
            Destroy(gameObject);
        }
    }
}
