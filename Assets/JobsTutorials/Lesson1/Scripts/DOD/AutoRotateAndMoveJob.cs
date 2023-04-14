using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

//本代码文件仅定义了一个自动旋转和移动但不检测目的地的job（多种实现方式），并且不需要像Scene：Cubes一样绑定到Cube上
//检测目的地的代码在Common-AutoReturnToPool中，于CubesGenerator中加上。
namespace Jobs.DOD
{
    public struct AutoRotateAndMoveJob : IJobParallelForTransform //没有用Burst编译，下个Optimzie0用了，其他一个字没变
    {
        public float deltaTime;
        public float moveSpeed;
        public float rotateSpeed;
        public NativeArray<Vector3> randTargetPosArray;

        public void Execute(int index, TransformAccess transform)//实现移动和旋转，并未实现目的地到达检测
        {
            Vector3 moveDir = (randTargetPosArray[index] - transform.position).normalized;
            transform.position += moveDir * moveSpeed * deltaTime;
            //为啥下面用四行来完成旋转？因为不能用transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime)，
            //这里的transform已经不是类了！没有现成的函数可以用
            Vector3 localEulerAngles = transform.localRotation.eulerAngles; 
            localEulerAngles.y += rotateSpeed * deltaTime;
            Quaternion localRotation = Quaternion.Euler(localEulerAngles);
            transform.localRotation = localRotation;
        }
    }

    [BurstCompile]//与上面一个字没变，就是加了BurstCompile
    public struct AutoRotateAndMoveJobOptimize0 : IJobParallelForTransform
    {
        public float deltaTime;
        public float moveSpeed;
        public float rotateSpeed;
        public NativeArray<Vector3> randTargetPosArray;

        public void Execute(int index, TransformAccess transform)
        {
            Vector3 moveDir = (randTargetPosArray[index] - transform.position).normalized;
            transform.position += moveDir * moveSpeed * deltaTime;
            Vector3 localEulerAngles = transform.localRotation.eulerAngles;
            localEulerAngles.y += rotateSpeed * deltaTime;
            Quaternion localRotation = Quaternion.Euler(localEulerAngles);
            transform.localRotation = localRotation;
        }
    }

    [BurstCompile]
    public struct AutoRotateAndMoveJobOptimize1 : IJobParallelForTransform
    {
        public float deltaTime;
        public float moveSpeed;
        public float rotateSpeed;
        [ReadOnly] public NativeArray<Vector3> randTargetPosArray;//仅额外加了ReadOnly提高并行读取此数组的能力

        public void Execute(int index, TransformAccess transform)
        {
            Vector3 moveDir = (randTargetPosArray[index] - transform.position).normalized;
            transform.position += moveDir * moveSpeed * deltaTime;
            Vector3 localEulerAngles = transform.localRotation.eulerAngles;
            localEulerAngles.y += rotateSpeed * deltaTime;
            Quaternion localRotation = Quaternion.Euler(localEulerAngles);
            transform.localRotation = localRotation;
        }
    }

    [BurstCompile]
    public struct AutoRotateAndMoveJobOptimize2 : IJobParallelForTransform
    {
        public float deltaTime;
        public float moveSpeed;
        public float rotateSpeed;
        [ReadOnly] public NativeArray<float3> randTargetPosArray;

        public void Execute(int index, TransformAccess transform)
        {//使用float3替换Vector3、用math.normalize()而不是xx.normalized，方便Burst编译器编译成SIMD指令，进而提高效率
            float3 moveDir = math.normalize(randTargetPosArray[index] - (float3)transform.position);
            float3 delta = moveDir * moveSpeed * deltaTime;
            transform.position += new Vector3(delta.x, delta.y, delta.z); //position只能用Vector3不能再用float了
            Vector3 localEulerAngles = transform.localRotation.eulerAngles;
            localEulerAngles.y += rotateSpeed * deltaTime;
            Quaternion localRotation = Quaternion.Euler(localEulerAngles);
            transform.localRotation = localRotation;
        }
    }
}


