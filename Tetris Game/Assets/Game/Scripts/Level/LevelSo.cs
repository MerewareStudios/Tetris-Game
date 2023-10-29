using System;
using System.Collections.Generic;
using System.Text;
using Game;
using Internal.Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game
{
    [CreateAssetMenu(fileName = "Level", menuName = "Game/Level Data", order = 0)]
    public class LevelSo : ScriptableObject
    {
        [SerializeField] public float deltaMult = 1.0f;
        [SerializeField] public Vector2Int boardSize = new Vector2Int(6, 7);
        [SerializeField] public Enemy.SpawnData EnemySpawnData;
        [SerializeField] public Const.Currency victoryReward;
        [SerializeField] public Const.Currency failReward;
        [SerializeField] public Board.SuggestedBlock[] suggestedBlocks;
        [SerializeField] public Board.PawnPlacement[] pawnPlacements;
        [SerializeField] public Pawn.Usage[] powerUps;


        public const int MinAutoWidth = 5;
        public const int MaxAutoWidthAdded = 4;
        
        public (string, int) ToString(int level)
        {
            int totalReward = 0;
            StringBuilder stringBuilder = new StringBuilder();
            
            stringBuilder.AppendLine("<color=black>" + "Level " + level + "</color>");
            stringBuilder.Append("<color=white>" + "Delta Mult : " + deltaMult + "</color>");
            stringBuilder.Append("<color=white>" + " | Board Size : " + boardSize.x + "x" + boardSize.y + "</color>");
            stringBuilder.AppendLine("<color=white>" + " | Spawn Interval : " + EnemySpawnData.spawnInterval + "</color>");

            int totalHealth = 0;
            
            foreach (var countData in EnemySpawnData.countDatas)
            {
                int enemyTotalReward = countData.enemyData.enemyRewards[0].amount * countData.count;
                stringBuilder.AppendLine("<color=red>" + countData.enemyData.name + " x" + countData.count + " (" + (countData.enemyData.maxHealth * countData.count) + ") : " + enemyTotalReward + "</color>");

                totalReward += enemyTotalReward;
                totalHealth += countData.enemyData.maxHealth * countData.count;
            }
            int nuggetCount = 0;
            foreach (var pawnPlacement in pawnPlacements)
            {
                if (pawnPlacement.usage.Equals(Pawn.Usage.Nugget))
                {
                    nuggetCount++;
                }
            }
            if (nuggetCount > 0)
            {
                totalReward += nuggetCount * 10;
                stringBuilder.AppendLine("<color=yellow>" + "Nugget x" + nuggetCount + " : " + (nuggetCount * 10) + "</color>");
            }
            stringBuilder.AppendLine("<color=magenta>" + "Victory Reward : " + victoryReward.amount + "</color>");
            
            totalReward += victoryReward.amount;
            
            stringBuilder.Append("<color=cyan>" + "Total Reward : " + totalReward + " | Total Health : " + totalHealth + "</color>");
            
            return (stringBuilder.ToString(), totalReward);
        }


        public static LevelSo AutoGenerate(int seed)
        {
            Random.InitState(seed);
            // Random.InitState((int)DateTime.Now.Ticks);

            LevelSo so = ScriptableObject.CreateInstance<LevelSo>();

            // Delta
            so.deltaMult = Random.Range(0.8f, 1.1f);
            // Size
            int addedSize = Random.Range(0, MaxAutoWidthAdded);
            so.boardSize = new Vector2Int(MinAutoWidth + addedSize, Random.Range(5, MinAutoWidth + 1 + addedSize + 1));
            // Spawn

            so.EnemySpawnData = new Enemy.SpawnData
            {
                spawnDelay = 3,
                spawnInterval = Random.Range(8.0f, 12.0f),
                countDatas = new List<Enemy.CountData>()
            };

            int maxHealth = 500 + (seed - 50) * 50;
            int currentHealth = 0;

            int spawnerCount = Random.Range(0, 2);
            
            void AddEnemy(EnemyData enemyData, int possibleHealth)
            {
                currentHealth += possibleHealth;
                so.EnemySpawnData.countDatas.Add(new Enemy.CountData(enemyData, 1));
            }
            
            for (int i = 0; i < spawnerCount; i++)
            {
                EnemyData enemyData = Const.THIS.GetRandomSpawnerEnemyData();
                AddEnemy(enemyData, 75);
            }

            while (currentHealth < maxHealth)
            {
                EnemyData enemyData = Const.THIS.GetRandomEnemyData();
                AddEnemy(enemyData, enemyData.maxHealth);
            }
            Debug.Log(spawnerCount + " " + so.EnemySpawnData.countDatas.Count + " " + currentHealth);
            // Reward
            so.victoryReward = new Const.Currency(Const.CurrencyType.Coin, 50);
            so.failReward = new Const.Currency(Const.CurrencyType.Coin, 5);
            // Suggested Block
            so.suggestedBlocks = null;
            // Pawn Placement
            so.pawnPlacements = Const.THIS.GetRandomPawnPlacement(so.boardSize);
            // Powerups
            so.powerUps = null;

            
            Random.InitState((int)DateTime.Now.Ticks);

            return so;
        }
    }
}