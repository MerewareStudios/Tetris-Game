using System;
using Game;
using Game.UI;
using IWI;
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
        #if UNITY_EDITOR
        [Header("Data")]
        [ReadOnly] [SerializeField] private float minTime;
        [ReadOnly] [SerializeField] private int totalCoinRevenue;
        [ReadOnly] [SerializeField] private int totalPiggyRevenue;
        [ReadOnly] [SerializeField] private int totalTicketRevenue;
        [ReadOnly] [SerializeField] private int totalHeartRevenue;
        private void OnEnable()
        {
            UpdateData();
        }
        void OnValidate()
        {
            UpdateData();
        }
        private void UpdateData()
        {
            minTime = EnemySpawnData.spawnDelay;
            totalCoinRevenue = 0;
            totalPiggyRevenue = 0;
            totalTicketRevenue = 0;
            totalHeartRevenue = 0;
            foreach (var enemyData in EnemySpawnData.countDatas)
            {
                minTime += enemyData.count * (EnemySpawnData.spawnTimeOffset);
                foreach (var reward in enemyData.enemyType.Prefab<Enemy>().so.enemyRewards)
                {
                    switch (reward.type)
                    {
                        case UpgradeMenu.PurchaseType.Coin:
                            totalCoinRevenue += reward.amount * enemyData.count;
                            break;
                        case UpgradeMenu.PurchaseType.PiggyCoin:
                            totalPiggyRevenue += reward.amount * enemyData.count;
                            break;
                        case UpgradeMenu.PurchaseType.SkipTicket:
                            totalTicketRevenue += reward.amount * enemyData.count;
                            break;
                        case UpgradeMenu.PurchaseType.Heart:
                            totalHeartRevenue += reward.amount * enemyData.count;
                            break;
                    }
                }
            }
            switch (victoryReward.type)
            {
                case Const.CurrencyType.Coin:
                    totalCoinRevenue += victoryReward.amount;
                    break;
                case Const.CurrencyType.PiggyCoin:
                    totalPiggyRevenue += victoryReward.amount;
                    break;
                case Const.CurrencyType.Ticket:
                    totalTicketRevenue += victoryReward.amount;
                    break;
            }
        }
#endif
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