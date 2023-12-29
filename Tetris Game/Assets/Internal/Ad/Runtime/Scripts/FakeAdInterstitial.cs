#define LOG

#if ADMOB_MEDIATION
    using GoogleMobileAds.Api;
#endif
using System;
using Internal.Core;
using UnityEngine;

public class FakeAdInterstitial : Lazyingleton<FakeAdInterstitial>
{
    
#region Mediation Variables

#if ADMOB_MEDIATION
    // TODO
    
#if UNITY_ANDROID
    private string _adUnitId = "ca-app-pub-9794688140048159/9730436196";
#elif UNITY_IPHONE
    private string _adUnitId = "ca-app-pub-3940256099942544/4411468910";
#else
    private string _adUnitId = "unused";
#endif
    
    private InterstitialAd _interstitialAd;

    
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
        DestoryMediation();
        
        Log("OnInterstitialAd Load Request");
        
        InterstitialAd.Load(_adUnitId, new AdRequest(), (ad, error) =>
        {
            WorkerThread.Current.AddJob(() =>
            {
                OnInterstitialAdLoadCallback(ad, error);
            });
        });
#else
        MaxSdk.LoadInterstitial(MaxAdUnitId);
#endif
    }
    
    private bool IsMediationReady()
    {
#if ADMOB_MEDIATION
        // TODO
        return _interstitialAd != null && _interstitialAd.CanShowAd();
#else
        return MaxSdk.IsInterstitialReady(MaxAdUnitId);
#endif
    }
    
    private void ShowMediation()
    {
#if ADMOB_MEDIATION
        // TODO
        _interstitialAd.Show();
#else
        MaxSdk.ShowInterstitial(MaxAdUnitId);
#endif
    }
    
    private void DestoryMediation()
    {
#if ADMOB_MEDIATION
        // TODO
        if (_interstitialAd == null)
        {
            return;
        }
        _interstitialAd.Destroy();
        _interstitialAd = null;
#else

#endif
    }
    
#if ADMOB_MEDIATION
    // TODO
    
    private void OnInterstitialAdLoadCallback(InterstitialAd ad, LoadAdError error)
    {
        if (error != null || ad == null)
        {
            OnInterstitialAdLoadFailedEvent(error);
            return;
        }

        Log("OnInterstitialAdLoaded");
        
        _interstitialAd = ad;
        
        ad.OnAdPaid += OnAdPaid;
        ad.OnAdImpressionRecorded += OnAdImpressionRecorded;
        ad.OnAdClicked += OnAdClicked;
        ad.OnAdFullScreenContentOpened += OnInterstitialAdFullScreenContentOpened;
        ad.OnAdFullScreenContentClosed += OnInterstitialAdFullScreenContentClosed;
        ad.OnAdFullScreenContentFailed += OnInterstitialAdFullScreenContentFailed;
        
        
        LoadState = LoadState.Success;
        OnLoadedStateChanged?.Invoke(LoadState);
        
        _retryAttempt = 0;
    }
    
    private void OnInterstitialAdLoadFailedEvent(LoadAdError error)
    {
        LogError("Rewarded ad failed to load an ad with error : " + error);

        LoadState = LoadState.Fail;
        OnLoadedStateChanged?.Invoke(LoadState);
        
        _retryAttempt++;
        InvokeForLoad();
    }
    private void OnInterstitialAdFullScreenContentOpened()
    {
// #if UNITY_EDITOR
//         Time.timeScale = 0.0f;
// #endif
    }
    private void OnInterstitialAdFullScreenContentFailed(AdError error)
    {
        WorkerThread.Current.AddJob(() =>
        {
            LogError(error.ToString());
            this.OnFailedDisplay?.Invoke();
            LoadAd();
        });
    }
    private void OnInterstitialAdFullScreenContentClosed()
    {
        WorkerThread.Current.AddJob(() =>
        {
            Log("OnInterstitialAdFullScreenContentClosed");
            this.OnHidden?.Invoke();
            LoadAd();
        });
    }
    private void OnAdClicked()
    {
    }
    private void OnAdPaid(AdValue adValue)
    {
    }
    private void OnAdImpressionRecorded()
    {
    }
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
