

using Unity.Entities;
using UnityEngine;

namespace DOTS.DOD.LESSON1
{
    struct RedCubeTag : IComponentData
    {
    }
    public class RedCubeTagAuthoring : MonoBehaviour
    {
        public class Baker : Baker<RedCubeTagAuthoring>
        {
            public override void Bake(RedCubeTagAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var redCube = new RedCubeTag();
                AddComponent(entity, redCube);
            }
        }
    }
}
