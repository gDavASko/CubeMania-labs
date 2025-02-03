using Unity.Burst;
using Unity.Entities;
using UnityEngine.UI;

[UpdateInGroup(typeof(LateSimulationSystemGroup), OrderLast = true)]
partial struct ResetEventsSystem : ISystem
{

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var select in SystemAPI.Query<RefRW<Selected>>().WithPresent<Selected>())
        {
            select.ValueRW.onSelected = false;
            select.ValueRW.onDeselected = false;
        };

        foreach (var hp in SystemAPI.Query<RefRW<Health>>())
        {
            hp.ValueRW.onHPChanged = false;
        };
    }
}
