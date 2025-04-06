using UnityEngine;

namespace GDB.Meshes
{
    [CreateAssetMenu(fileName = "BlockInfoSides", menuName = "GDB/BlockInfoSides")]
    public class BlockInfoSides : BlockInfoSimple
    {
        [field:SerializeField] protected Vector2 AtlasSpritePositionsUp { get; private set; }
        [field:SerializeField] protected Vector2 AtlasSpritePositionsDown { get; private set; }

        public override Vector2 GetPixelOffset(Vector3 normal)
        {
            Vector2 res;

            if (normal == Vector3.up)
                res = AtlasSpritePositionsUp;
            else if (normal == Vector3.down)
                res = AtlasSpritePositionsDown;
            else
                res = base.GetPixelOffset(normal);
            
            return res;
        }
    }
}