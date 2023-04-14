using Jobs.Common;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Rendering.UI;
using Random = UnityEngine.Random;
namespace Jobs.OOD
{
    [RequireComponent(typeof(BoxCollider))]//保证该脚本绑定上的GenerateArea游戏对象有BoxCollider这一组件，因为下面会用到它。
    public class CubesGenerator : MonoBehaviour
    {
        public GameObject cubeArchetype = null;
        public GameObject targetArea = null;
        [Range(10, 10000)] public int generationTotalNum = 10;
        [Range(1, 60)] public int generationNumPerTicktime = 20;//这里的值优先级低于editor里设置的值，改成1也不变。
        [Range(0.1f, 1.0f)] public float tickTime = 1.0f;
        [HideInInspector]//不在Inspector中显示
        public Vector3 generatorAreaSize;
        [HideInInspector]
        public Vector3 targetAreaSize;
        
        //开启collectionChecks时，当外部尝试销毁池内对象时，会触发异常报错
        public bool collectionChecks = true;
        // 对象池
        private ObjectPool<GameObject> pool = null;
        private float timer = 0.0f;
        void Start()
        {
            ///创建对象池
            if (pool == null)
                pool = new ObjectPool<GameObject>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool,
                    OnDestroyPoolObject, collectionChecks, 10, generationTotalNum);

            generatorAreaSize = GetComponent<BoxCollider>().size;
            if (targetArea != null)
                targetAreaSize = targetArea.GetComponent<BoxCollider>().size;

            timer = 0.0f;
        }

        void Update()
        {
            if (timer >= tickTime)
            {
                GenerateCubes();
                timer -= tickTime;
            }

            timer += Time.deltaTime;
        }

        private void OnDestroy()
        {
            if (pool != null)
                pool.Dispose(); //对象池要手动清理！
        }
        
        private void GenerateCubes()
        {
            if (!cubeArchetype  || pool == null)
                return;
            for (int i = 0; i < generationNumPerTicktime; ++i)
            {
                if (pool.CountAll< generationTotalNum) //保证一批内也不会超过上限，
                {//奇怪的是设上限50，一次50后，就算物体消失也不会产生新的。主要原因是这里用的是pool.CountAll,计的是总数，被销毁的也还在
                    //因为对象池大小=上限，所以这段代码的意义最多总共生成那么多，生成完就不再产生新的了，但是如果在达到上限前有销毁的物体
                    //那就会复用老对象，减少老对象销毁和新对象创建的开销。
                    GameObject cube = pool.Get();
                    if (cube)
                    {
                        ReturnToPool component = cube.GetComponent<ReturnToPool>();
                        component.pool = pool;//保证cube在退回对象池的时候能找到这个对象池，因此给它的ReturnToPool脚本传输本对象池的引用
                        Vector3 randomPos = new Vector3(Random.Range(-generatorAreaSize.x * 0.5f, generatorAreaSize.x * 0.5f),
                            0,
                            Random.Range(-generatorAreaSize.z * 0.5f, generatorAreaSize.z * 0.5f));
                        cube.transform.position = transform.position + randomPos;
                        if (targetArea)//AutoRotateAndMove脚本挂在cube上，会方便找到它并设置它的目的地
                            cube.GetComponent<AutoRotateAndMove>().targetPos = GetRandomTargetPos();
                    }
                }
                else
                {//能到这里说明已经满了，因此可以把timer设为0，大多数情况下从for循环次数到了出去，而不是从else出去
                    timer = 0;
                    return;
                }
            }
        }
        private Vector3 GetRandomTargetPos()
        {
            return targetArea.transform.position + new Vector3(Random.Range(-targetAreaSize.x * 0.5f, targetAreaSize.x * 0.5f),
                0,
                Random.Range(-targetAreaSize.z * 0.5f, targetAreaSize.z * 0.5f));
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
