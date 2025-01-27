using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

partial struct ShootAttackSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
       foreach(var (shooter, target) in SystemAPI
            .Query< RefRW<ShootAttack>, 
                    RefRO<Target>>())
        {
            if (target.ValueRO.targetEntity == Entity.Null)
                continue;

            if(shooter.ValueRO.timer > 0f)
            {
                shooter.ValueRW.timer -= SystemAPI.Time.DeltaTime;
                continue;
            }

            shooter.ValueRW.timer = shooter.ValueRO.timerMax;

            int damage = 1;
            var targetHealth = SystemAPI.GetComponentRW<Health>(target.ValueRO.targetEntity);
            targetHealth.ValueRW.health = math.max(0, targetHealth.ValueRO.health - damage);
        }
    }
}
