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
    
    public static int CurrentLevel => THIS.CurrentLevel();
    private static LevelSo _currentLevelSo;
    public static float DeltaMult = 1.0f;
    public void LoadLevel()
    {
        _currentLevelSo = Const.THIS.GetLevelSo(CurrentLevel);
        AnalyticsManager.LevelStart(CurrentLevel);
        
        Board.THIS.Construct(BoardSize());

        GameManager.PLAYING = true;
        Map.THIS.StartMainLoop();
        Spawner.THIS.OnLevelLoad();

        levelText.text = "LEVEL " + CurrentLevel;
        DeltaMult = GetDeltaMult();

        Warzone.THIS.EnemySpawnData = GetEnemySpawnData();
        Warzone.THIS.OnLevelLoad();

        UIManager.UpdateNotifications();
       
#if !UNITY_EDITOR || FORCE_EDITOR_CONCENT
        if (MaxSdk.IsUserConsentSet())
#endif
        {
            BeginLevel();
        }

        if (ONBOARDING.WEAPON_TAB.IsComplete())
        {
            AdManager.THIS.ShowBannerOffer();
        }
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
        
        AnalyticsManager.LevelEnd(GAProgressionStatus.Complete);
    }
    
    public void OnFail()
    {
        GameManager.PLAYING = false;
        
        GameManager.THIS.OnFail();
        SlashScreen.THIS.Show(SlashScreen.State.Fail, 0.25f, GetFailReward(), CurrentLevel);
        
        AnalyticsManager.LevelEnd(GAProgressionStatus.Fail);
    }

    public void OnClick_Restart()
    {
        Consent.THIS.Close();
        if (!GameManager.PLAYING)
        {
            return;
        }
        OnFail();
    }
    public static Enemy.SpawnData GetEnemySpawnData()
    {
        return _currentLevelSo.EnemySpawnData;
    }
    public static Const.Currency GetVictoryReward()
    {
        return _currentLevelSo.victoryReward;
    }
    public static Const.Currency GetFailReward()
    {
        return _currentLevelSo.failReward;
    }
    public static Board.SuggestedBlock[] GetSuggestedBlocks()
    {
        return _currentLevelSo.suggestedBlocks;
    }
    public static float GetDeltaMult()
    {
        return _currentLevelSo.deltaMult;
    }
    public static Vector2Int BoardSize()
    {
        return _currentLevelSo.boardSize;
    }
    public static Board.PawnPlacement[] PawnPlacements()
    {
        return _currentLevelSo.pawnPlacements;
    }
}
