using System;
using DG.Tweening;
using Game;
using Internal.Core;
using IWI;
using UnityEngine;
using Visual.Effects;

public class GameManager : Singleton<GameManager>
{
    [System.NonSerialized] public static bool PLAYING = false;
    
    public static readonly int UnscaledTime = Shader.PropertyToID("_UnscaledTime");
    public static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    // public static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
    public static readonly int InsideColor = Shader.PropertyToID("_InsideColor");
    public static readonly int EmissionKey = Shader.PropertyToID("_EmissionColor");
    
    public static float TimeScale = 1.0f;
    
    public static void UpdateTimeScale()
    {
        Time.timeScale = GameManager.TimeScale * 
                         PowerSelectionScreen.THIS.Timescale * 
                         Consent.THIS.TimeScale *
                         MenuNavigator.THIS.TimeScale *
                         PiggyMenu.THIS.TimeScale;
    }

    public static void GameTimeScale(float value)
    {
        TimeScale = value;
        UpdateTimeScale();
    }

    void Awake()
    {
        AnalyticsManager.Init();
    }

    void Start()
    {
        Distortion.Complete = (go, state) =>
        {
            go.Despawn(Pool.Distortion);
            if (state)
            {
                ApplicationManager.THIS.GrabFeatureEnabled = false;
            }
        };

        if (ONBOARDING.UPGRADE_TAB.IsNotComplete())
        {
            Board.THIS.OnMerge += CheckMergeOnboarding;
        }

        LevelManager.THIS.LoadLevel();
        
    #if !UNITY_EDITOR
        if (!MaxSdk.IsUserConsentSet())
        {
            Consent.THIS.Open(() =>
            {
                Consent.THIS.Loading = true;
                AdManager.THIS.InitAdSDK(() =>
                {
                    Consent.THIS.Close();
                    LevelManager.THIS.BeginLevel();
                });
            });
            return;
        }
    #endif
        
        AdManager.THIS.InitAdSDK();
        
        // Const.THIS.PrintLevelData();
    }

    public void MarkTabStepsComplete()
    {
        ONBOARDING.BLOCK_TAB.SetComplete();
        Board.THIS.OnMerge -= CheckMergeOnboarding;
    }

    private void CheckMergeOnboarding()
    {
        if (ONBOARDING.SPEECH_CHEER.IsNotComplete())
        {
            Onboarding.CheerForMerge();
            ONBOARDING.SPEECH_CHEER.SetComplete();
            return;
        }

        if (ONBOARDING.BLOCK_TAB.IsNotComplete())
        {
            if (Wallet.COIN.Amount >= 15)
            {
                // ONBOARDING.BLOCK_TAB.SetComplete();
                UIManager.THIS.shop.AnimatedShow();
            }
            return;
        }
        if (ONBOARDING.WEAPON_TAB.IsNotComplete())
        {
            if (Wallet.COIN.Amount >= 25)
            {
                // ONBOARDING.WEAPON_TAB.SetComplete();
                UIManager.THIS.shop.AnimatedShow();
            }
            return;
        }
        if (ONBOARDING.UPGRADE_TAB.IsNotComplete())
        {
            if (Wallet.COIN.Amount >= 25 && LevelManager.CurrentLevel >= 4)
            {
                // ONBOARDING.UPGRADE_TAB.SetComplete();
                UIManager.THIS.shop.AnimatedShow();
            }
            return;
        }
    }

    
    
    public static void AddCoin(int value)
    {
       Wallet.COIN.Transaction(value);
    }
    public static void AddPiggyCoin(int value)
    {
        Wallet.PIGGY.Transaction(value);
    }
    public static void AddTicket(int value)
    {
        Wallet.TICKET.Transaction(value);
    }
    public static void AddHeart(int value)
    {
        Warzone.THIS.Player._CurrentHealth += value;
    }
    
    public void Deconstruct()
    {
        Spawner.THIS.Deconstruct();
        Map.THIS.Deconstruct();
        Board.THIS.Deconstruct();
        Warzone.THIS.Deconstruct();
    }

    public void OnVictory()
    {
        Warzone.THIS.OnVictory();
        Board.THIS.OnVictory();
        OnLevelEnd();
    }
    public void OnFail()
    {
        Warzone.THIS.OnFail();
        OnLevelEnd();
    }

    private void OnLevelEnd()
    {
        Onboarding.Deconstruct();
        Board.THIS.OnLevelEnd();
        Map.THIS.OnLevelEnd();
        Spawner.THIS.OnLevelEnd();
    }
}
