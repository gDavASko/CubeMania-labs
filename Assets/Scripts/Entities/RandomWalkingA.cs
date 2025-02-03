using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class RandomWalkingA : MonoBehaviour
{
    public Vector3 targetPosition;
    public Vector3 originPosition;
    public float minDist = 1f;
    public float maxDist = 3f;
    public uint seed = 1;
    
    public class Baker : Baker<RandomWalkingA>
    {
        public override void Bake(RandomWalkingA authoring)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(e, new RandomWalking()
            {
                targetPosition = authoring.targetPosition,
                originPosition = authoring.originPosition,
                minDist = authoring.minDist,
                maxDist = authoring.maxDist,
                Random = new Unity.Mathematics.Random(authoring.seed),
            });
        }
    }
}

public struct RandomWalking: IComponentData
{
    public float3 targetPosition;
    public float3 originPosition;
    public float minDist;
    public float maxDist;
    public Unity.Mathematics.Random Random;
}