// #define FORCE_EDITOR_CONCENT

using System;
using System.Collections;
using DG.Tweening;
using Game;
using Google.Play.Review;
using Internal.Core;
using IWI;
using UnityEngine;
using Visual.Effects;


public class GameManager : Singleton<GameManager>
{
    [System.NonSerialized] public static bool PLAYING = false;

    [SerializeField] private Map map;
    [SerializeField] private Board board;
    [SerializeField] private Warzone warzone;
    [SerializeField] public Const Const;
    [SerializeField] public AnimConst AnimConst;
    [SerializeField] public Onboarding Onboarding;
    
    public static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    public static readonly int InsideColor = Shader.PropertyToID("_InsideColor");
    public static readonly int EmissionKey = Shader.PropertyToID("_EmissionColor");
    public static readonly int EmisKey = Shader.PropertyToID("_Emission");
    
    private int _timeScale = 1;

    private Coroutine _flowRoutine = null;
    
    public static void UpdateTimeScale()
    {
        Time.timeScale = GameManager.THIS._timeScale * 
                         AdBreakScreen.THIS.TimeScale * 
                         PowerSelectionScreen.THIS.TimeScale * 
                         Consent.THIS.TimeScale *
                         MenuNavigator.THIS.TimeScale *
                         PiggyMenu.THIS.TimeScale *
                         SettingsManager.THIS.TimeScale *
                         OfferScreen.THIS.TimeScale;
    }
    
    public static void GameTimeScale(int value)
    {
        GameManager.THIS._timeScale = value;
        UpdateTimeScale();
    }

    public void Init()
    {
        Map.THIS = map;
        Board.THIS = board;
        Warzone.THIS = warzone;
        Const.THIS = this.Const;
        AnimConst.THIS = this.AnimConst;
        Onboarding.THIS = this.Onboarding;
    }

    void Awake()
    {
        AnalyticsManager.FacebookInit();    
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

        DOTween.SetTweensCapacity(200, 50);
        
        AnalyticsManager.Init();
        LevelManager.THIS.LoadLevel();
        
        #if CREATIVE
        Const.THIS.creativeSettings.Speak();
        Const.THIS.creativeSettings.giftIndex = 0;
            return;
        #endif
        
        
        if (ONBOARDING.WEAPON_TAB.IsNotComplete())
        {
            Board.THIS.OnMerge += CheckMergeOnboarding;
        }
        
    #if !UNITY_EDITOR || FORCE_EDITOR_CONCENT
        if (!AdManager.HasTakenAnyConsent())
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
        OfferScreen.THIS.CheckForUnpack(2.5f);
        
        UIManagerExtensions.DistortWarmUp();
    }

    public void MarkTabStepsComplete()
    {
        ONBOARDING.BLOCK_TAB.SetComplete();
        Board.THIS.OnMerge -= CheckMergeOnboarding;
    }

    private bool CheckMergeOnboarding()
    {
        if (ONBOARDING.SPEECH_CHEER.IsNotComplete())
        {
            Onboarding.CheerForMerge();
            ONBOARDING.SPEECH_CHEER.SetComplete();
            return false;
        }

        if (ONBOARDING.BLOCK_TAB.IsNotComplete())
        {
            if (Wallet.COIN.Amount >= 10 && LevelManager.THIS.CurrentLevel() > 1)
            {
                Spawner.THIS.MountBack();
                UIManager.THIS.shop.AnimatedShow();
                return true;
            }
            return false;
        }
        if (ONBOARDING.WEAPON_TAB.IsNotComplete() && LevelManager.THIS.CurrentLevel() > 1)
        {
            if (Wallet.COIN.Amount >= 25)
            {
                Spawner.THIS.MountBack();
                UIManager.THIS.shop.AnimatedShow();
                return true;
            }
            return false;
        }
        return false;
    }
    
    public void ShareTheGame(System.Action onStart, System.Action<bool> onFinish)
    {
        if (_flowRoutine != null)
        {
            return;
        }
        onStart?.Invoke();
        _flowRoutine = StartCoroutine(Flow());
        return;

        IEnumerator Flow()
        {
            const string shareSubject = "Play Combatris";
            const string shareSocialMessageGooglePlay = "\ud83d\udd25 Combatris \ud83d\udd25\n\nGoogle Play\n"
                                        + "https://play.google.com/store/apps/details?id=com.iwi.combatris";

            yield return new WaitForSecondsRealtime(0.25f);
            
            if (!Application.isEditor)
            {
                //Create intent for action send
                AndroidJavaClass intentClass = new AndroidJavaClass ("android.content.Intent");
                AndroidJavaObject intentObject = new AndroidJavaObject ("android.content.Intent");
                intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string> ("ACTION_SEND"));

                //put text and subject extra
                intentObject.Call<AndroidJavaObject> ("setType", "text/plain");
                intentObject.Call<AndroidJavaObject> ("putExtra", intentClass.GetStatic<string> ("EXTRA_SUBJECT"), shareSubject);
                intentObject.Call<AndroidJavaObject>  ("putExtra", intentClass.GetStatic<string> ("EXTRA_TEXT"), shareSocialMessageGooglePlay);

                //call createChooser method of activity class
                AndroidJavaClass unity = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
                AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject> ("currentActivity");
                AndroidJavaObject chooser = intentClass.CallStatic<AndroidJavaObject> ("createChooser", intentObject, "Share your high score");
                currentActivity.Call ("startActivity", chooser);
            }

            yield return new WaitForSecondsRealtime(0.25f);
            _flowRoutine = null;
            onFinish?.Invoke(true);
        }
    }

    public void LeaveComment(System.Action onStart, System.Action<bool> onFinish)
    {
        if (_flowRoutine != null)
        {
            return;
        }
        onStart?.Invoke();
        _flowRoutine = StartCoroutine(Flow());
        return;

        IEnumerator Flow()
        {
            ReviewManager reviewManager = new ReviewManager();
            
            var requestFlowOperation = reviewManager.RequestReviewFlow();
            
            yield return requestFlowOperation;
            
            if (requestFlowOperation.Error != ReviewErrorCode.NoError)
            {
                Debug.LogError(requestFlowOperation.Error);
                onFinish?.Invoke(false);
                yield return new WaitForSeconds(0.25f);
                yield break;
            }
            var playReviewInfo = requestFlowOperation.GetResult();
            
            var launchFlowOperation = reviewManager.LaunchReviewFlow(playReviewInfo);
            
            yield return launchFlowOperation;
            
            if (launchFlowOperation.Error != ReviewErrorCode.NoError)
            {
                Debug.LogError(launchFlowOperation.Error);
                onFinish?.Invoke(false);
                yield return new WaitForSeconds(0.25f);
                yield break;
            }

            yield return new WaitForSecondsRealtime(0.25f);
            _flowRoutine = null;
            onFinish?.Invoke(true);
        }
    }

    public void PlayCoinAudio()
    {
        // Audio.Coin.PlayOneShot();
    }
    
    public static void AddCoin(int value)
    {
        Audio.Meta_Coin.PlayOneShot();
        Wallet.COIN.Add(value);
    }
    public static void AddPiggyCoin(int value)
    {
        Audio.Meta_Gem.PlayOneShot();
        Wallet.PIGGY.Add(value);
    }
    public static void AddTicket(int value)
    {
        Audio.Meta_Ticket.PlayOneShot();
        Wallet.TICKET.Add(value);
    }
    // public static void AddTicketEnd()
    // {
    //     Audio.Meta_Ticket_End.PlayOneShot();
    // }
    // public static void AddGemEnd()
    // {
    //     Audio.Meta_Gem_End.PlayOneShot();
    // }
    public static void AddHeart(int value)
    {
        Audio.Heart.PlayOneShot();
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
        PowerSelectionScreen.THIS.Close();
    }
}
