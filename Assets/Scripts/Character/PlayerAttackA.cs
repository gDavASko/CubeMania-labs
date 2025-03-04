
using Unity.Entities;
using UnityEngine;

namespace GDB.Character
{
    public class PlayerAttackA : MonoBehaviour
    {
        public int Damage = 25;
        public float CoolDown = 0.5f;
        
        public class Baker : Baker<PlayerAttackA>
        {
            public override void Bake(PlayerAttackA authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new PlayerAttack
                {
                    Damage = authoring.Damage,
                    CoolDown = authoring.CoolDown
                });
            }
        }
    }

    public struct PlayerAttack : IComponentData
    {
        public int Damage;
        public float CoolDown;
        public float CDTimer;
    }
}