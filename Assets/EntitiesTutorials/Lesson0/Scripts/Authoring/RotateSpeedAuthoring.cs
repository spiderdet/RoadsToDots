using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DOTS.DOD.LESSON0
{
    struct RotateSpeed : IComponentData
    {
        public float rotateSpeed;
    }
    public class RotateSpeedAuthoring : MonoBehaviour
    {
        [Range(0, 360)]public float rotateSpeed = 360.0f;
        public class Baker : Baker<RotateSpeedAuthoring>
        {
            public override void Bake(RotateSpeedAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                /*TransformUsageFlags可在conversion阶段用于区分不同entity的transform，减少不需要考虑的entity，比如.Dynamic
                 *代表在runtime时transform会动的那些entity */
                var data = new RotateSpeed
                {
                    rotateSpeed = math.radians(authoring.rotateSpeed)
                };
                AddComponent(entity,data);
            }
        }
    }
}
