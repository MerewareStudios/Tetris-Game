using Game;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(fileName = "Level", menuName = "Game/Level Data", order = 0)]
    public class LevelSo : ScriptableObject
    {
        [SerializeField] public Enemy.SpawnData EnemySpawnData;
        [SerializeField] public bool canGiveBonus = true;
        [SerializeField] public Const.Currency victoryReward;
        [SerializeField] public Const.Currency failReward;
        [SerializeField] public Board.SuggestedBlock[] suggestedBlocks;
    }
}



public static class LevelSoExtension
{
    public static bool CanSpawnBonus(this int level)
    {
        return Const.THIS.Levels[level - 1].canGiveBonus;
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
}