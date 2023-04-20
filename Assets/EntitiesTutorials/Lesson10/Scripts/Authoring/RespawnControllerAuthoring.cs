using DOTS.DOD.LESSON9;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Serialization;
using UnityEngine;

namespace DOTS.DOD.LESSON10
{
    struct RespawnController : IComponentData
    {
        public float timer;
    }

    struct PrefabBufferElement : IBufferElementData
    {
        public EntityPrefabReference prefab; //EntityPrefabReference是内置类型，是buffer内元数据的类型，buffer是可变大小的
    }

    public class RespawnControllerAuthoring : MonoBehaviour
    {
        public GameObject[] spawners = null;
        [Range(1, 5)]public float timer = 3.0f;
        public class Baker : Baker<RespawnControllerAuthoring>
        {
            public override void Bake(RespawnControllerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);//不需要加载任何组件，创建最简单的实体
                var data = new RespawnController
                {
                    timer = authoring.timer
                };
                AddComponent(entity, data);
                var buffer = AddBuffer<PrefabBufferElement>(entity);//给实体添加buffer并且指定buffer内元数据的类型：PrefabBufferElement
                for (int i = 0; i < authoring.spawners.Length; i++)
                {
                    var elem = new PrefabBufferElement
                    {
                        prefab = new EntityPrefabReference(authoring.spawners[i])
                    };
                    buffer.Add(elem);
                }
            }
        }
    }
}
