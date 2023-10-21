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