using System;
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

            LevelSo so = ScriptableObject.CreateInstance<LevelSo>();

            // Delta
            so.deltaMult = Random.Range(0.8f, 1.1f);
            // Size
            int addedSize = Random.Range(0, 4);
            so.boardSize = new Vector2Int(5 + addedSize, 6 + addedSize);
            // Spawn
            so.EnemySpawnData.spawnDelay = 3;
            so.EnemySpawnData.spawnInterval = Random.Range(9.0f, 15.0f);
            so.EnemySpawnData.countDatas = null;
            // Reward
            so.victoryReward = new Const.Currency(Const.CurrencyType.Coin, 50);
            so.failReward = new Const.Currency(Const.CurrencyType.Coin, 5);
            // Suggested Block
            
            // Pawn Placement
            
            // Pawn Placement

            
            Random.InitState((int)DateTime.Now.Ticks);

            return so;
        }
    }
}