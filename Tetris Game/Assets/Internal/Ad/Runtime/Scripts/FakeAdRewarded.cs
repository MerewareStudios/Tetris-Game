using System;
using Internal.Core;
using UnityEngine;

public class FakeAdRewarded : Lazyingleton<FakeAdRewarded>
{
    
#region Mediation Variables

#if ADMOB_MEDIATION
            
    // TODO
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
#else
        MaxSdk.LoadRewardedAd(MaxAdUnitId);
#endif
    }
    
    private bool IsMediationReady()
    {
#if ADMOB_MEDIATION
        // TODO
        return false;
#else
        return MaxSdk.IsRewardedAdReady(MaxAdUnitId);
#endif
    }
    
    private void ShowMediation()
    {
#if ADMOB_MEDIATION
        // TODO
#else
        MaxSdk.ShowRewardedAd(MaxAdUnitId);
#endif
    }
    
#if ADMOB_MEDIATION
    // TODO
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
        
        _retryAttempt++;
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
    
    
    
    private int _retryAttempt;
    [System.NonSerialized] public System.Action OnHidden;
    [System.NonSerialized] public System.Action OnReward;
    [System.NonSerialized] public System.Action OnFailedDisplay;
    [System.NonSerialized] public System.Action<LoadState> OnLoadedStateChanged;
    [System.NonSerialized] public LoadState LoadState = LoadState.None;
    [System.NonSerialized] private bool _invoking = false;

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
    
    public void ForwardInvoke()
    {
        if (_invoking)
        {
            CancelInvoke(nameof(LoadAd));
            LoadAd();
        }
    }
    
    public void Initialize()
    {
        InitializeMediation();
        
        LoadState = LoadState.None;
    }

    public void LoadAd()
    {
        _invoking = false;
        LoadState = LoadState.Loading;
        OnLoadedStateChanged?.Invoke(LoadState);
        LoadMediation();
    }

    private void InvokeForLoad()
    {
        Invoke(nameof(LoadAd), Mathf.Pow(2, Math.Min(6, _retryAttempt)));
        _invoking = true;
    }
}
