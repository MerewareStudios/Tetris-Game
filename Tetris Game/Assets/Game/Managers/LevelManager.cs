// #define FORCE_EDITOR_CONCENT

using DG.Tweening;
using Game;
using GameAnalyticsSDK;
using Internal.Core;
using IWI;
using TMPro;
using UnityEngine;

public class LevelManager : Singleton<LevelManager>
{
    [SerializeField] public TextMeshProUGUI levelText;
    [SerializeField] public RectTransform levelTextRect;
    [SerializeField] public Vector3 gameScale;
    [SerializeField] public Vector3 menuScale;
    [SerializeField] public Vector3 gameAnchor;
    [SerializeField] public Vector3 menuAnchor;
    [System.NonSerialized] public int Concede = 0;
    
    public static int CurrentLevel => THIS.CurrentLevel();
    public static LevelSo LevelSo { get; set; }
    public static float DeltaMult = 1.0f;
    public static float HealthMult = 1.0f;
    
    public void LoadLevel()
    {
        Concede = 0;
        HealthMult = 1.0f;
        LevelSo = Const.THIS.GetLevelSo(CurrentLevel);
        #if UNITY_EDITOR
            // SaveManager.CreateSavePoint("Level " + CurrentLevel + " Save Data");
        #endif

#if CREATIVE
        if (Const.THIS.creativeSettings.customSize)
        {
            Board.THIS.Construct(Const.THIS.creativeSettings.boardSize);
        }
        else
        {
            Board.THIS.Construct(BoardSize());
        }
#else
        Board.THIS.Construct(BoardSize());
#endif
    }

    public void OnLateLoad()
    {
        GameManager.PLAYING = true;
        Map.THIS.StartMainLoop();
        Spawner.THIS.OnLevelLoad();

        levelText.text = "Level " + CurrentLevel;
#if CREATIVE
        if (!Const.THIS.creativeSettings.levelTextEnabled)
        {
            levelText.enabled = false;
        }
#endif
        
        DeltaMult = GetDeltaMult();

        Warzone.THIS.OnLateLoad(LevelSo.sortInterval);

        UIManager.UpdateNotifications();

        BeginLevel();

        if (CurrentLevel >= 5)
        {
            Spawner.THIS.nextBlockDisplay.Visible = true;
        }
        
        AdManager.THIS.ShowBannerFrame();
    }
    
    public void ScaleLevelText(bool menuMode)
    {
        levelTextRect.DOKill();
        levelTextRect.DOScale(menuMode ? menuScale : gameScale, 0.35f).SetUpdate(true).SetEase(Ease.OutSine);
        levelTextRect.DOAnchorPos(menuMode ? menuAnchor : gameAnchor, 0.35f).SetUpdate(true).SetEase(Ease.OutSine);
    }

    public void BeginLevel()
    {
        if (ONBOARDING.ALL_BLOCK_STEPS.IsComplete())
        {
            Warzone.THIS.Begin();
            Spawner.THIS.DelayedSpawn(0.45f);
        }
        else if (ONBOARDING.DRAG_AND_DROP.IsNotComplete())
        {
            Onboarding.SpawnFirstBlockAndTeachPlacement();
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
        SlashScreen.THIS.Show(SlashScreen.State.Victory, 0.25f, GetVictoryReward(), CurrentLevel);
        this.NextLevel();

        Concede = 0;
        AnalyticsManager.LevelEnd(GAProgressionStatus.Complete);
    }
    
    public void OnFail()
    {
        GameManager.PLAYING = false;
        
        GameManager.THIS.OnFail();
        SlashScreen.THIS.Show(SlashScreen.State.Fail, 0.1f, GetFailReward(), CurrentLevel);
        
        AnalyticsManager.LevelEnd(GAProgressionStatus.Fail);
    }
    
    public void OnConcede()
    {
        GameManager.PLAYING = false;
        
        GameManager.THIS.OnFail();
        SlashScreen.THIS.Show(SlashScreen.State.Concede, 0.1f, GetFailReward(), CurrentLevel);
        
        AnalyticsManager.LevelEnd(GAProgressionStatus.Fail);
    }

    public void OnClick_Restart()
    {
        if (!GameManager.PLAYING)
        {
            return;
        }
        AnalyticsManager.Concede(LevelManager.CurrentLevel, ++LevelManager.THIS.Concede);
        
        OnConcede();
    }
    public static LevelSo.EnemySpawnDatum[] GetEnemySpawnData()
    {
        return LevelSo.enemySpawnData;
    }
    public static Const.Currency GetVictoryReward()
    {
        return LevelSo.victoryReward;
    }
    public static Const.Currency GetFailReward()
    {
        return LevelSo.failReward;
    }
    public static Board.SuggestedBlock[] GetSuggestedBlocks()
    {
        return LevelSo.suggestedBlocks;
    }
    public static float GetDeltaMult()
    {
        return LevelSo.deltaMult;
    }
    public static Vector2Int BoardSize()
    {
        return LevelSo.boardSize;
    }
    public static Board.PawnPlacement[] PawnPlacements()
    {
        return LevelSo.pawnPlacements;
    }
}
