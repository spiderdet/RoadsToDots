using Unity.Entities;
using UnityEngine;

namespace DOTS.DOD.LESSON4
{
    public class WaveCubeGeneratorAuthoring : MonoBehaviour
    {
        public GameObject cubePrefab = null;
        [Range(10, 100)] public int xHalfCount = 40;
        [Range(10, 100)] public int zHalfCount = 40;
        
        class WaveCubeGeneratorBaker : Baker<WaveCubeGeneratorAuthoring>
        {
            public override void Bake(WaveCubeGeneratorAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new WaveCubeGenerator
                {
                    cubeProtoType = GetEntity(authoring.cubePrefab, TransformUsageFlags.Dynamic),
                    halfCountX = authoring.xHalfCount,
                    halfCountZ = authoring.zHalfCount
                });
            }
        }
    }
}
