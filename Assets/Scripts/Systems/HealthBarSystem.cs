using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
partial struct HealthBarSystem : ISystem
{
    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        Vector3 camForward = Vector3.zero;
     
        if(Camera.main != null)
            camForward = Camera.main.transform.forward;

        foreach (var (hpBar, barLT) in SystemAPI.Query<RefRO<HealthBar>, RefRW<LocalTransform>>())
        {
            var parentLt = SystemAPI.GetComponent<LocalTransform>(hpBar.ValueRO.healthEntity);
            
            if(parentLt.Scale >= 1f)
                barLT.ValueRW.Rotation = parentLt.InverseTransformRotation(Quaternion.LookRotation(camForward, Vector3.up));
            

            var hp = SystemAPI.GetComponentRW<Health>(hpBar.ValueRO.healthEntity);

            if (!hp.ValueRO.onHPChanged)
                continue;

            float curPercent = (float)hp.ValueRO.health / hp.ValueRO.maxHealth;

            if (curPercent >= 1f)
            {
                barLT.ValueRW.Scale = 0f;
            }
            else
            { 
                barLT.ValueRW.Scale = 1f;
            }
                var hpBarMat = SystemAPI.GetComponentRW<PostTransformMatrix>(hpBar.ValueRO.barVisualEntity);
                hpBarMat.ValueRW.Value = float4x4.Scale(curPercent, 1f, 1f);            
        }
    }
}
