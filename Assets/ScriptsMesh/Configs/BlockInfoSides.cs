using UnityEngine;

namespace GDB.Meshes
{
    [CreateAssetMenu(fileName = "BlockInfoSides", menuName = "GDB/BlockInfoSides")]
    public class BlockInfoSides : BlockInfoSimple
    {
        [field:SerializeField] protected Vector2[] AtlasSpritePositionsUp { get; private set; } = null;
        [field:SerializeField] protected Vector2[] AtlasSpritePositionsDown { get; private set; } = null;

        public override Vector2 GetPixelOffset(Vector3 normal)
        {
            Vector2 res;

            if (normal == Vector3.up)
                res = AtlasSpritePositionsUp.Rnd();
            else if (normal == Vector3.down)
                res = AtlasSpritePositionsDown.Rnd();
            else
                res = base.GetPixelOffset(normal);
            
            return res;
        }
    }
}