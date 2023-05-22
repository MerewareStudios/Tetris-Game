using Internal.Core;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Map : Singleton<Map>
    {
        [Header("Grid Settings")]
        [SerializeField] public Grid grid;
        [SerializeField] private int width = 5;
        [SerializeField] private int height = 5;
        [SerializeField] private float space = 1.0f;
        [SerializeField] private Vector3 spawnPos = Vector3.zero;
        
        [Header("Block Settings")]
        [System.NonSerialized] public Block currentBlock;
        [SerializeField] public List<BlockPrefabData> blockPrefabData;

        [System.NonSerialized] public MaterialPropertyBlock MPB_PAWN;

        void Awake()
        {
            MPB_PAWN = new();    
        }
        void Start()
        {
            grid.Generate(width, height, space);

            SpawnRandomBlock();
        }

        public void BlockPlaced()
        {
            currentBlock.Mark();
            grid.Submit(currentBlock);
        }

        public void SpawnRandomBlock()
        {
            Spawn((Block.Type)Random.Range(0, System.Enum.GetValues(typeof(Block.Type)).Length));
            //Spawn(Block.Type.S);
        }

        public void Spawn(Block.Type blockType)
        {
            currentBlock = blockPrefabData[((int)blockType)].block.Spawn<Block>(this.transform);
            currentBlock.transform.position = spawnPos + currentBlock.offset;
            currentBlock.transform.localRotation = Quaternion.identity;
            currentBlock.transform.localScale = Vector3.one;
            currentBlock.OnSpawn();
        }

        [System.Serializable]
        public class BlockPrefabData
        {
            [SerializeField] public Block.Type Type;
            [SerializeField] public Pool block;
        }
    }
}
