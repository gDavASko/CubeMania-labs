using System.Collections.Generic;
using UnityEngine;

namespace GDB.Meshes
{
    [CreateAssetMenu(fileName = "BlocksCommon", menuName = "GDB/BlocksCommon")]
    public class BlocksCommon : ScriptableObject
    {
        [SerializeField] private BlockInfoSimple[] blocks;
        [SerializeField] private BlockInfoSimple _defaultblock;

        private Dictionary<BlockType, BlockInfoSimple> _blocksDict = null;

        private Dictionary<BlockType, BlockInfoSimple> BlocksDict
        {
            get
            {
                if (_blocksDict == null)
                {
                    _blocksDict = new Dictionary<BlockType, BlockInfoSimple>();
                    foreach (var block in blocks)
                    {
                        _blocksDict.Add(block.BlockType, block);
                    }
                }
                return _blocksDict;
            }
        }

        public BlockInfoSimple Get(BlockType type)
        {
            if (BlocksDict.TryGetValue(type, out var block))
            {
                return block;
            }
            
            return _defaultblock;
        }
    }
}