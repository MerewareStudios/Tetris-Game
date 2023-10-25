using System.Text;
using Game;
using Internal.Core;
using UnityEngine;

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
    }
}



public static class LevelSoExtension
{
    public static Pawn.Usage GetRandomPowerUp(this int level)
    {
        if (Const.THIS.Levels[level - 1].powerUps.Length == 0)
        {
            return Const.THIS.powerUps.Random();
        }
        return Const.THIS.Levels[level - 1].powerUps.Random();
    }
    public static Enemy.SpawnData GetEnemySpawnData(this int level)
    {
        return Const.THIS.Levels[level - 1].EnemySpawnData;
    }
    public static Const.Currency GetVictoryReward(this int level)
    {
        return Const.THIS.Levels[level - 1].victoryReward;
    }
    public static Const.Currency GetFailReward(this int level)
    {
        return Const.THIS.Levels[level - 1].failReward;
    }
    public static Board.SuggestedBlock[] GetSuggestedBlocks(this int level)
    {
        return Const.THIS.Levels[level - 1].suggestedBlocks;
    }
    public static float DeltaMult(this int level)
    {
        return Const.THIS.Levels[level - 1].deltaMult;
    }
    public static Vector2Int BoardSize(this int level)
    {
        return Const.THIS.Levels[level - 1].boardSize;
    }
    public static Board.PawnPlacement[] PawnPlacements(this int level)
    {
        return Const.THIS.Levels[level - 1].pawnPlacements;
    }
}