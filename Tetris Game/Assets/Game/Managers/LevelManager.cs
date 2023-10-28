using Game;
using GameAnalyticsSDK;
using Internal.Core;
using IWI;
using UnityEngine;

public class LevelManager : Singleton<LevelManager>
{
    public static int CurrentLevel => THIS.CurrentLevel();
    public static LevelSo CurrentLevelSo;
    public static float DeltaMult = 1.0f;

    public void LoadLevel()
    {
        CurrentLevelSo = Const.THIS.GetLevelSo(CurrentLevel);
        AnalyticsManager.LevelStart(CurrentLevel);
        
        Board.THIS.Construct(BoardSize());

        GameManager.PLAYING = true;
        Map.THIS.StartMainLoop();
        Spawner.THIS.OnLevelLoad();

        UIManager.THIS.levelText.text = "LEVEL " + CurrentLevel;
        UIManager.THIS.levelTextMenu.text = UIManager.THIS.levelText.text;
        DeltaMult = GetDeltaMult();

        Warzone.THIS.EnemySpawnData = GetEnemySpawnData();
        Warzone.THIS.OnLevelLoad();
        
        if (ONBOARDING.ALL_BLOCK_STEPS.IsComplete())
        {
            Warzone.THIS.Begin();
            Spawner.THIS.DelayedSpawn(0.45f);
        }
        else if (ONBOARDING.DRAG_AND_DROP.IsNotComplete())
        {
            Onboarding.SpawnFirstBlockAndTeachPlacement();
        }

        if (ONBOARDING.UPGRADE_TAB.IsComplete())
        {
            AdManager.THIS.ShowBannerOrOffer();
        }
    }

    public void CheckEndLevel()
    {
        if (!GameManager.PLAYING)
        {
            return;
        }
        if (Warzone.THIS.Player._CurrentHealth <= 0)
        {
            OnFail();
            return;
        }
        if (Warzone.THIS.IsCleared)
        {
            OnVictory();
        }
    }

    public void OnVictory()
    {
        GameManager.PLAYING = false;
        
        GameManager.THIS.OnVictory();
        SlashScreen.THIS.Show(SlashScreen.State.Victory, 0.25f, GetVictoryReward());
        this.NextLevel();
        
        AnalyticsManager.LevelEnd(GAProgressionStatus.Complete);
    }
    
    public void OnFail()
    {
        GameManager.PLAYING = false;
        
        GameManager.THIS.OnFail();
        SlashScreen.THIS.Show(SlashScreen.State.Fail, 0.25f, GetFailReward());
        
        AnalyticsManager.LevelEnd(GAProgressionStatus.Fail);
    }
    
    
    public static Pawn.Usage GetRandomPowerUp()
    {
        if (CurrentLevelSo.powerUps == null || CurrentLevelSo.powerUps.Length == 0)
        {
            return Const.THIS.powerUps.Random();
        }
        return CurrentLevelSo.powerUps.Random();
    }
    public static Enemy.SpawnData GetEnemySpawnData()
    {
        return CurrentLevelSo.EnemySpawnData;
    }
    public static Const.Currency GetVictoryReward()
    {
        return CurrentLevelSo.victoryReward;
    }
    public static Const.Currency GetFailReward()
    {
        return CurrentLevelSo.failReward;
    }
    public static Board.SuggestedBlock[] GetSuggestedBlocks()
    {
        return CurrentLevelSo.suggestedBlocks;
    }
    public static float GetDeltaMult()
    {
        return CurrentLevelSo.deltaMult;
    }
    public static Vector2Int BoardSize()
    {
        return CurrentLevelSo.boardSize;
    }
    public static Board.PawnPlacement[] PawnPlacements()
    {
        return CurrentLevelSo.pawnPlacements;
    }
}
