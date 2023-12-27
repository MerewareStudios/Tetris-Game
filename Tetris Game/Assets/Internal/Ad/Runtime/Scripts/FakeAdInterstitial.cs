using System;
using Internal.Core;
using UnityEngine;

public class FakeAdInterstitial : Lazyingleton<FakeAdInterstitial>
{
    
#region Mediation Variables

#if ADMOB_MEDIATION
            
    // TODO
#else
    private const string MaxAdUnitId = "c447e9a9232d8a7e";

#endif
    
    private void InitializeMediation()
    {
#if ADMOB_MEDIATION
        // TODO
#else
        MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
        MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialLoadFailedEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialDisplayedEvent;
        MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialClickedEvent;
        MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialHiddenEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialAdFailedToDisplayEvent;
#endif
    }
    
    private void LoadMediation()
    {
#if ADMOB_MEDIATION
        // TODO
#else
        MaxSdk.LoadInterstitial(MaxAdUnitId);
#endif
    }
    
    private bool IsMediationReady()
    {
#if ADMOB_MEDIATION
        // TODO
        return false;
#else
        return MaxSdk.IsInterstitialReady(MaxAdUnitId);
#endif
    }
    
    private void ShowMediation()
    {
#if ADMOB_MEDIATION
        // TODO
#else
        MaxSdk.ShowInterstitial(MaxAdUnitId);
#endif
    }
    
#if ADMOB_MEDIATION
    // TODO
#else
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
        InvokeForLoad();
    }

    private void OnInterstitialDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
#if UNITY_EDITOR
        Time.timeScale = 0.0f;
#endif
    }

    private void OnInterstitialAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    {
        this.OnFailedDisplay?.Invoke();
        LoadAd();
    }

    private void OnInterstitialClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
    }

    private void OnInterstitialHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        this.OnHidden?.Invoke();
        LoadAd();
    }
#endif
    
#endregion


    private int _retryAttempt;
    [System.NonSerialized] public System.Action OnHidden;
    [System.NonSerialized] public System.Action OnFailedDisplay;
    [System.NonSerialized] public System.Action<LoadState> OnLoadedStateChanged;
    [System.NonSerialized] private bool _invoking = false;
    [System.NonSerialized] public LoadState LoadState = LoadState.None;

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

    public void Show(System.Action onHidden = null, System.Action onFailedDisplay = null)
    {
        this.OnHidden = onHidden;
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

    public void DestroyInterstitial()
    {
        
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
