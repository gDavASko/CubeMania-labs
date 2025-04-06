using UnityEngine;

namespace GDB.Meshes
{
    [CreateAssetMenu(fileName = "BlockInfoSimple", menuName = "GDB/BlockInfoSimple")]
    public class BlockInfoSimple : ScriptableObject
    {
        [field:SerializeField] public BlockType BlockType { get; private set; } =  BlockType.Air;
        [field:SerializeField] protected Vector2 AtlasSpritePositions { get; private set; }
        [field:SerializeField] public Vector2[] AtlasBreakSpritePositions { get; private set; } = null;

        [field:SerializeField] public AudioClip BreakSound { get; private set; } = null;
        [field:SerializeField] public AudioClip StepsSound { get; private set; } = null;
        [field:SerializeField] public int Health { get; private set; } = 10;

        public virtual Vector2 GetPixelOffset(Vector3 normal)
        {
            return AtlasSpritePositions;
        }
    }
}