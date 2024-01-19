using System;
using Febucci.Attributes;
using Internal.Core;
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
        
        public static LevelSo AutoGenerate(int levelIndex)
        {
            Random.InitState(levelIndex);

            // LevelSo so = ScriptableObject.CreateInstance<LevelSo>();
            LevelSo so = Const.THIS.GetModLevel(levelIndex);

            // SetSpawnData(so, Const.THIS.GetRandomAutoLevel());
            // SetSizeAndPlacementData(so, Const.THIS.GetRandomAutoLevel());
            // SetTotalCoin(so, Const.THIS.GetRandomAutoLevel());
            // SetTotalCoin(so, Const.THIS.GetRandomAutoLevel());
            // SetRewards(so, Const.THIS.GetRandomAutoLevel());
            // SetCarryData(so, level);

            LevelManager.HealthMult = 1.0f + Mathf.FloorToInt(levelIndex / (float)Const.THIS.MaxLevel) * 0.25f;
            Random.InitState((int)DateTime.Now.Ticks);

            return so;
        }

        private static void SetSpawnData(LevelSo toData, LevelSo fromData)
        {
            toData.sortInterval = fromData.sortInterval;
            toData.deltaMult = fromData.deltaMult;
            toData.countdown = fromData.countdown;
            toData.enemySpawnData = fromData.enemySpawnData;
        }
        private static void SetSizeAndPlacementData(LevelSo toData, LevelSo fromData)
        {
            toData.boardSize = fromData.boardSize;
            toData.pawnPlacements = fromData.pawnPlacements;
        }
        private static void SetTotalCoin(LevelSo toData, LevelSo fromData)
        {
            toData.totalCoin = fromData.totalCoin;
        }
         
        private static void SetRewards(LevelSo toData, LevelSo fromData)
        {
            toData.victoryReward = fromData.victoryReward;
            toData.failReward = fromData.failReward;
        }
        private static void SetCarryData(LevelSo data, int level)
        {
            if (level % 5 == 0)
            {
                data.carryData = new Airplane.CarryData(Cargo.Type.MaxStack, Random.Range(0, 5));
                return;
            }

            if (Helper.IsPossible(0.1f))
            {
                data.carryData = new Airplane.CarryData(Cargo.Type.Health, Random.Range(0, 5));
                return;
            }
            if (Helper.IsPossible(0.1f))
            {
                data.carryData = new Airplane.CarryData(Cargo.Type.Chest, Random.Range(0, 5));
                return;
            }
            // if (Helper.IsPossible(0.1f))
            // {
            //     data.carryData = new Airplane.CarryData(Cargo.Type.Intel, Random.Range(0, 5));
            //     return;
            // }
            
            data.carryData = new Airplane.CarryData(Cargo.Type.MaxStack, -1);
        }
    }
}