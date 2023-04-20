using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Jobs;

namespace Jobs.DOD
{
    [BurstCompile]
    struct WaveCubesJob : IJobParallelForTransform
    {
        [ReadOnly] public float elapsedTime;

        public void Execute(int index, TransformAccess transform)//这里不是TransformAccessArray而是单个的TransformAccess
        {
            var distance = Vector3.Distance(transform.position, Vector3.zero);
            transform.localPosition += Vector3.up * math.sin(elapsedTime * 3f + distance * 0.2f);
        }
    }
    

    public class WaveCubesWithJobs : MonoBehaviour
    {
        //ProfilerMarker<>
        static readonly ProfilerMarker<int> profilerMarker = new ProfilerMarker<int>("WaveCubesWithJobs UpdateTransform", "Objects Count");
        public GameObject cubeAchetype = null;
        [Range(10, 100)] public int xHalfCount = 40;
        [Range(10, 100)] public int zHalfCount = 40;

        private TransformAccessArray transformAccessArray; //will be initialized in Start()

        void Start()
        {
            transformAccessArray = new TransformAccessArray(4 * xHalfCount * zHalfCount);
            for (var z = -zHalfCount; z < zHalfCount; z++)
            {
                for (var x = -xHalfCount; x < xHalfCount; x++)
                {
                    var cube = Instantiate(cubeAchetype);//这里实例化后得到的是gameobject而不是entity，因此可能cache不友好
                    cube.transform.position = new Vector3(x * 1.1f, 0, z * 1.1f);
                    transformAccessArray.Add(cube.transform);
                }
            }
        }

        void Update()
        {
            using (profilerMarker.Auto(transformAccessArray.length))
            {
                var waveCubesJob = new WaveCubesJob
                {
                    elapsedTime = Time.time,
                };
                var waveCubesJobhandle = waveCubesJob.Schedule(transformAccessArray);
                waveCubesJobhandle.Complete();
            }
        }

        private void OnDestroy()
        {
            transformAccessArray.Dispose();//dots is unmanaged code, need to be disposed manually
        }
    }
}
