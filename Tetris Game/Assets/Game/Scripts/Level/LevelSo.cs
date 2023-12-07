using System;
using Febucci.Attributes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game
{
    [CreateAssetMenu(fileName = "Level", menuName = "Game/Level Data", order = 0)]
    public class LevelSo : ScriptableObject
    {
        [SerializeField] public float sortInterval = 6.0f;
        [SerializeField] public float deltaMult = 1.0f;
        [SerializeField] public Vector2Int boardSize = new Vector2Int(6, 7);
        [SerializeField] public int countdown = 3;
        [SerializeField] public int totalCoin = 100;
        [SerializeField] public EnemySpawnDatum[] enemySpawnData;
        [SerializeField] public Const.Currency victoryReward;
        [SerializeField] public Const.Currency failReward;
        [SerializeField] public Board.SuggestedBlock[] suggestedBlocks;
        [SerializeField] public Board.PawnPlacement[] pawnPlacements;
        [SerializeField] public Airplane.CarryData carryData;

        [System.Serializable]
        public class EnemySpawnDatum
        {
            [SerializeField] public EnemyData enemyData;
            [MinValue(1)][SerializeField] public int count = 1;
            [SerializeField] public float distance = 0.0f;
            [SerializeField] public float offset = 0.0f;
            [SerializeField] public float delay = 0;
        } 
        
        public static LevelSo AutoGenerate(int seed)
        {
            Random.InitState(seed);

            LevelSo so = ScriptableObject.CreateInstance<LevelSo>();

            // // Delta
            // so.deltaMult = Random.Range(0.8f, 1.1f);
            // // Size
            // int addedSize = Random.Range(0, MaxAutoWidthAdded);
            // so.boardSize = new Vector2Int(MinAutoWidth + addedSize, Random.Range(5, MinAutoWidth + 1 + addedSize + 1));
            // // Spawn
            //
            // so.EnemySpawnData = new Enemy.SpawnData
            // {
            //     spawnDelay = 3,
            //     spawnInterval = Random.Range(8.0f, 12.0f),
            //     countDatas = new List<Enemy.CountData>()
            // };
            //
            // // int excess = Mathf.Min(seed, 60) - 50;
            // // int maxHealth = 250 + excess * 50;
            // int maxHealth = 250 + (seed - 50) * 25;
            // int currentHealth = 0;
            //
            // int spawnerCount = Random.Range(0, 2);
            //
            // void AddEnemy(EnemyData enemyData, int possibleHealth)
            // {
            //     currentHealth += possibleHealth;
            //     so.EnemySpawnData.countDatas.Add(new Enemy.CountData(enemyData, 1));
            // }
            //
            // for (int i = 0; i < spawnerCount; i++)
            // {
            //     EnemyData enemyData = Const.THIS.GetRandomSpawnerEnemyData();
            //     AddEnemy(enemyData, 75);
            // }
            //
            // while (currentHealth < maxHealth)
            // {
            //     EnemyData enemyData = Const.THIS.GetRandomEnemyData();
            //     AddEnemy(enemyData, enemyData.maxHealth);
            // }
            // // Reward
            // so.victoryReward = new Const.Currency(Const.CurrencyType.Coin, 50);
            // so.failReward = new Const.Currency(Const.CurrencyType.Coin, 5);
            // // Suggested Block
            // so.suggestedBlocks = null;
            // // Pawn Placement
            // so.pawnPlacements = Const.THIS.GetRandomPawnPlacement(so.boardSize);
            // // Powerups
            // // so.powerUps = null;
            
            Random.InitState((int)DateTime.Now.Ticks);

            return so;
        }
    }
}