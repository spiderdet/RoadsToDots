﻿using DOTS.DOD.LESSON0;
using DOTS.DOD.LESSON2;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace DOTS.DOD.LESSON2
{
    [BurstCompile]
    partial struct RotateCubeWithJobEntity : IJobEntity
    {
        public float deltaTime;
        void Execute(ref LocalTransform transform, in RotateSpeed speed)//会再查询拥有这两个组件的entity，查到后对其处理
        {
            transform = transform.RotateY(speed.rotateSpeed * deltaTime);
        }
    }
    
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(CubeRotateWithIJobEntitySystemGroup))]
    public partial struct CubeRotateWithIJobEntitySystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var job = new RotateCubeWithJobEntity { deltaTime = SystemAPI.Time.DeltaTime };
            job.ScheduleParallel();
            //job.Schedule();
            //job.Run();
        }
    }
}
