using System;
using Internal.Core;
using UnityEngine;

public class FakeAdInterstitial : Lazyingleton<FakeAdInterstitial>
{
    private const string AdUnitId = "c447e9a9232d8a7e";
    private int _retryAttempt;
    [System.NonSerialized] public System.Action OnFinish;
    [System.NonSerialized] public System.Action OnFailedDisplay;
    [System.NonSerialized] public System.Action<LoadState> OnLoadedStateChanged;

    [System.NonSerialized] public LoadState LoadState = LoadState.Loading;

    public bool Ready => MaxSdk.IsInterstitialReady(AdUnitId);

    public void Show(System.Action onFinish = null, System.Action onFailedDisplay = null)
    {
        this.OnFinish = onFinish;
        this.OnFailedDisplay = onFailedDisplay;
        if (MaxSdk.IsInterstitialReady(AdUnitId))
        {
            MaxSdk.ShowInterstitial(AdUnitId);
        }
    }
    
    public void Initialize()
    {
        MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
        MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialLoadFailedEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialDisplayedEvent;
        MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialClickedEvent;
        MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialHiddenEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialAdFailedToDisplayEvent;
        
        LoadInterstitial();
    }

    private void LoadInterstitial()
    {
        LoadState = LoadState.Loading;
        OnLoadedStateChanged?.Invoke(LoadState);
        MaxSdk.LoadInterstitial(AdUnitId);
    }

    private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        LoadState = LoadState.Success;
        OnLoadedStateChanged?.Invoke(LoadState);
        
        _retryAttempt = 0;
    }

    private void OnInterstitialLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        LoadState = LoadState.Fail;
        OnLoadedStateChanged?.Invoke(LoadState);
        
        _retryAttempt++;
        Invoke("LoadInterstitial", Mathf.Pow(2, Math.Min(6, _retryAttempt)));
    }

    private void OnInterstitialDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        
    }

    private void OnInterstitialAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    {
        this.OnFailedDisplay?.Invoke();
        LoadInterstitial();
    }

    private void OnInterstitialClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        
        
    }

    private void OnInterstitialHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        this.OnFinish?.Invoke();

        LoadInterstitial();
    }
    
}
