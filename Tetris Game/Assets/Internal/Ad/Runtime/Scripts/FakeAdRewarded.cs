using System;
using Internal.Core;
using UnityEngine;

public class FakeAdRewarded : Lazyingleton<FakeAdRewarded>
{
    private const string AdUnitId = "3a4b26d73c5511e1";
    private int _retryAttempt;
    [System.NonSerialized] public System.Action OnFinish;
    [System.NonSerialized] public System.Action OnReward;
    [System.NonSerialized] public System.Action OnFailedDisplay;
    [System.NonSerialized] public System.Action<LoadState> OnLoadedStateChanged;
    
    [System.NonSerialized] public LoadState LoadState = LoadState.Loading;

    public bool Ready => MaxSdk.IsRewardedAdReady(AdUnitId);

    public void Show(System.Action onFinish = null, System.Action onReward = null, System.Action onFailedDisplay = null)
    {
        this.OnFinish = onFinish;
        this.OnReward = onReward;
        this.OnFailedDisplay = onFailedDisplay;
        if (MaxSdk.IsRewardedAdReady(AdUnitId))
        {
            MaxSdk.ShowRewardedAd(AdUnitId);
        }
    }
    
    public void Initialize()
    {
        MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
        MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdLoadFailedEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
        MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
        MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardedAdRevenuePaidEvent;
        MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdHiddenEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
        MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;
            
        LoadRewardedAd();
    }
    
    

    private void LoadRewardedAd()
    {
        LoadState = LoadState.Loading;
        OnLoadedStateChanged?.Invoke(LoadState);
        MaxSdk.LoadRewardedAd(AdUnitId);
    }

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
        Invoke("LoadRewardedAd", Mathf.Pow(2, Math.Min(6, _retryAttempt)));
    }

    private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        
    }

    private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    {
        this.OnFailedDisplay?.Invoke();
        LoadRewardedAd();
    }

    private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        
    }

    private void OnRewardedAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        this.OnFinish?.Invoke();

        LoadRewardedAd();
    }

    private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
    {
        OnReward?.Invoke();
    }

    private void OnRewardedAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        
    }
}
