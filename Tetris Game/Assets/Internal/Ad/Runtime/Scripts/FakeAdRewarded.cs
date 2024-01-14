#if ADMOB_MEDIATION
    using GoogleMobileAds.Api;
#endif
using System;
using Internal.Core;
using UnityEngine;

public class FakeAdRewarded : AdCore.AdBase<FakeAdRewarded>
{
    
#region Mediation Variables

#if ADMOB_MEDIATION
    // TODO
#if UNITY_ANDROID
    private string _adUnitId = "ca-app-pub-9794688140048159/7403655756";
#elif UNITY_IPHONE
    private string _adUnitId = "ca-app-pub-3940256099942544/1712485313";
#else
    private string _adUnitId = "unused";
#endif
    
    private RewardedAd _rewardedAd;
    
#else
    private const string MaxAdUnitId = "3a4b26d73c5511e1";

#endif
    
    private void InitializeMediation()
    {
#if ADMOB_MEDIATION
        // TODO
#else
        MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
        MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdLoadFailedEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
        MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
        MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardedAdRevenuePaidEvent;
        MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdHiddenEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
        MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;
#endif
    }
    
    private void LoadMediation()
    {
#if ADMOB_MEDIATION
        // TODO
        DestroyMediation();
        RewardedAd.Load(_adUnitId, new AdRequest(), (ad, error) =>
        {
            WorkerThread.Current.AddJob(() =>
            {
                OnRewardedAdLoadCallback(ad, error);
            });
        });
#else
        MaxSdk.LoadRewardedAd(MaxAdUnitId);
#endif
    }
    
    private bool IsMediationReady()
    {
#if ADMOB_MEDIATION
        // TODO
        return _rewardedAd != null && _rewardedAd.CanShowAd();
#else
        return MaxSdk.IsRewardedAdReady(MaxAdUnitId);
#endif
    }
    
    private void ShowMediation()
    {
#if ADMOB_MEDIATION
        // TODO
        _rewardedAd.Show(OnRewardedAdReceivedRewardEvent);
#else
        MaxSdk.ShowRewardedAd(MaxAdUnitId);
#endif
    }
    
    private void DestroyMediation()
    {
#if ADMOB_MEDIATION
        // TODO
        if (_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }
#else

#endif
    }
    
#if ADMOB_MEDIATION
    // TODO

    private void OnRewardedAdLoadCallback(RewardedAd ad, LoadAdError error)
    {
        if (error != null || ad == null)
        {
            OnRewardedAdLoadFailedEvent(error);
            return;
        }

        _rewardedAd = ad;
        
        JoinAnalytics(_adUnitId, _rewardedAd);

        
        // ad.OnAdPaid += OnRewardedAdRevenuePaidEvent;
        // ad.OnAdImpressionRecorded += OnAdImpressionRecorded;
        // // ad.OnAdClicked += OnRewardedAdClickedEvent;
        // ad.OnAdFullScreenContentOpened += OnRewardedAdFullScreenContentOpened;
        ad.OnAdFullScreenContentClosed += OnRewardedAdFullScreenContentClosed;
        ad.OnAdFullScreenContentFailed += OnRewardedAdFullScreenContentFailed;
        
        
        LoadState = LoadState.Success;
        OnLoadedStateChanged?.Invoke(LoadState);
        
        ResetAttempts();
    }

    private void OnRewardedAdLoadFailedEvent(LoadAdError error)
    {
        LoadState = LoadState.Fail;
        OnLoadedStateChanged?.Invoke(LoadState);

        InvokeForLoad();
    }

//     private void OnRewardedAdFullScreenContentOpened()
//     {
// // #if UNITY_EDITOR
// //         Time.timeScale = 0.0f;
// // #endif
//     }

    private void OnRewardedAdFullScreenContentFailed(AdError error)
    {
        WorkerThread.Current.AddJob(() =>
        {
            this.OnFailedDisplay?.Invoke();
            LoadAd();
        });
    }

    // private void OnRewardedAdClickedEvent()
    // {
    // }

    private void OnRewardedAdFullScreenContentClosed()
    {
        WorkerThread.Current.AddJob(() =>
        {
            this.OnHidden?.Invoke();
            LoadAd();
        });
    }

    private void OnRewardedAdReceivedRewardEvent(Reward reward)
    {
        WorkerThread.Current.AddJob(() =>
        {
            OnReward?.Invoke();
        });
    }

    // private void OnRewardedAdRevenuePaidEvent(AdValue adValue)
    // {
    // }
    //
    // private void OnAdImpressionRecorded()
    // {
    // }
    
#else
    private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        LoadState = LoadState.Success;
        OnLoadedStateChanged?.Invoke(LoadState);
        
        _retryAttempt = 0;
    }

    private void OnRewardedAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        LoadState = LoadState.Fail;
        OnLoadedStateChanged?.Invoke(LoadState);
        
        InvokeForLoad();
    }

    private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
#if UNITY_EDITOR
        Time.timeScale = 0.0f;
#endif
    }

    private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    {
        this.OnFailedDisplay?.Invoke();
        LoadAd();
    }

    private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        
    }

    private void OnRewardedAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        this.OnHidden?.Invoke();
        LoadAd();
    }

    private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
    {
        OnReward?.Invoke();
    }

    private void OnRewardedAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        
    }
#endif
    
#endregion
    
    
    
    [System.NonSerialized] public System.Action OnHidden;
    [System.NonSerialized] public System.Action OnReward;
    [System.NonSerialized] public System.Action OnFailedDisplay;
    [System.NonSerialized] public System.Action<LoadState> OnLoadedStateChanged;

    public bool Ready
    {
        get
        {
            if (IsMediationReady())
            {
                return true;
            }
            ForwardInvoke();
            return false;
        }
    }

    public void Show(System.Action onHidden = null, System.Action onReward = null, System.Action onFailedDisplay = null)
    {
        this.OnHidden = onHidden;
        this.OnReward = onReward;
        this.OnFailedDisplay = onFailedDisplay;
        
        if (Ready)
        {
            ShowMediation();
        }
    }
    
    public override void Initialize(AnalyticsSubscription subscribeToAnalytics)
    {
        base.Initialize(subscribeToAnalytics);
        InitializeMediation();
        LoadState = LoadState.None;
    }

    public override bool LoadAd()
    {
        if (base.LoadAd())
        {
            return true;
        }
        LoadState = LoadState.Loading;
        OnLoadedStateChanged?.Invoke(LoadState);
        LoadMediation();
        return true;
    }


}
