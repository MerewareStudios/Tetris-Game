using Game;
using Internal.Core;
using UnityEngine;
using Visual.Effects;

public class GameManager : Singleton<GameManager>
{
    [System.NonSerialized] public static bool PLAYING = false;
    
    private static readonly int UnscaledTime = Shader.PropertyToID("_UnscaledTime");
    public static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    // public static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
    public static readonly int InsideColor = Shader.PropertyToID("_InsideColor");
    public static readonly int EmissionKey = Shader.PropertyToID("_EmissionColor");

    void Start()
    {
        Distortion.Complete = (go, state) =>
        {
            go.Despawn();
            if (state)
            {
                ApplicationManager.THIS.GrabFeatureEnabled = false;
            }
        };
        
        Board.THIS.OnMerge += (amount) =>
        {
            if (ONBOARDING.HAVE_MERGED.IsNotComplete())
            {
                Onboarding.CheerForMerge();
                ONBOARDING.HAVE_MERGED.SetComplete();
            }

            if (!ONBOARDING.LEARNED_ALL_TABS.IsNotComplete())
            {
                return;
            }
            
            if (ONBOARDING.ABLE_TO_USE_BLOCK_TAB.IsNotComplete())
            {
                if (Wallet.COIN.Amount >= 15)
                {
                    ONBOARDING.ABLE_TO_USE_BLOCK_TAB.SetComplete();
                    UIManager.THIS.shop.AnimatedShow();
                }
                return;
            }
            if (ONBOARDING.ABLE_TO_USE_WEAPON_TAB.IsNotComplete())
            {
                if (Wallet.COIN.Amount >= 25)
                {
                    ONBOARDING.ABLE_TO_USE_WEAPON_TAB.SetComplete();
                    UIManager.THIS.shop.AnimatedShow();
                }
                return;
            }
            if (ONBOARDING.ABLE_TO_USE_UPGRADE_TAB.IsNotComplete())
            {
                if (Wallet.PIGGY.Amount >= 1 || LevelManager.CurrentLevel >= 4)
                {
                    ONBOARDING.ABLE_TO_USE_UPGRADE_TAB.SetComplete();
                    UIManager.THIS.shop.AnimatedShow();
                }
                return;
            }
        };

        LevelManager.THIS.LoadLevel();
    }

    void Update()
    {
        SaveManager.THIS.saveData.playTime += Time.deltaTime;

        if (UIManager.MenuVisible)
        {
            Shader.SetGlobalFloat(UnscaledTime, Time.unscaledTime);
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
        Powerup.THIS.Deconstruct();
    }

    public void OnVictory()
    {
        Warzone.THIS.OnVictory();
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
