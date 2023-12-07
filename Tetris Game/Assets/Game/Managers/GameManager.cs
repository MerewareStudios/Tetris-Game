// #define FORCE_EDITOR_CONCENT

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

    void Start()
    {
        AnalyticsManager.Init();

        Distortion.Complete = (go, state) =>
        {
            go.Despawn(Pool.Distortion);
            if (state)
            {
                ApplicationManager.THIS.GrabFeatureEnabled = false;
            }
        };

        if (ONBOARDING.WEAPON_TAB.IsNotComplete())
        {
            Board.THIS.OnMerge += CheckMergeOnboarding;
        }

        LevelManager.THIS.LoadLevel();
        
        DOTween.SetTweensCapacity(200, 50);
        
        
    #if !UNITY_EDITOR || FORCE_EDITOR_CONCENT
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
        OfferScreen.THIS.CheckForUnpack(2.5f);
        
        UIManagerExtensions.DistortWarmUp();

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
            if (Wallet.COIN.Amount >= 10)
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
    
    
    public static void AddCoin(int value)
    {
       Wallet.COIN.Add(value);
    }
    public static void AddPiggyCoin(int value)
    {
        Wallet.PIGGY.Add(value);
    }
    public static void AddTicket(int value)
    {
        Wallet.TICKET.Add(value);
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
        PowerSelectionScreen.THIS.Close();
    }
}
