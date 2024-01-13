// #define LOG

#if ADMOB_MEDIATION
    using GoogleMobileAds.Api;
#endif
    using AdCore;
    using DG.Tweening;
using Internal.Core;
using UnityEngine;
using UnityEngine.UI;

public class FakeAdBanner : AdCore.AdBase<FakeAdBanner>
{
#region Mediation Variables

#if ADMOB_MEDIATION
    BannerView _bannerView;

#if UNITY_ANDROID
    private string _adUnitId = "ca-app-pub-9794688140048159/4924832074";
#elif UNITY_IPHONE
    private string _adUnitId = "ca-app-pub-3940256099942544/2934735716";
#else
    private string _adUnitId = "unused";
    
#endif
    // TODO
#else
    private const string MaxAdUnitId = "85fc6bf5a70ecf37";

#endif
    
    private void InitializeMediation()
    {
#if ADMOB_MEDIATION
        // TODO

        base.ADType = AdType.BANNER;
        DestroyMediation();
        
        _bannerView = new BannerView(_adUnitId, AdSize.Banner, ToMediationBannerPosition(_currentPosition));
        CallAnalytics(_adUnitId, _bannerView);
        
        _bannerView.OnBannerAdLoaded += OnBannerAdLoaded;
        _bannerView.OnBannerAdLoadFailed += OnBannerAdLoadFailed;
        _bannerView.OnAdPaid += OnAdPaid;
        _bannerView.OnAdImpressionRecorded += OnAdImpressionRecorded;
        _bannerView.OnAdClicked += OnAdClicked;
        _bannerView.OnAdFullScreenContentOpened += OnAdFullScreenContentOpened;
        _bannerView.OnAdFullScreenContentClosed += OnAdFullScreenContentClosed;
#else
        MaxSdk.SetBannerExtraParameter(MaxAdUnitId, "adaptive_banner", "true");
        MaxSdk.SetBannerBackgroundColor(MaxAdUnitId, backgroundColor);
        
        MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoadedEvent;
        MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerAdLoadFailedEvent;
        MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerAdClickedEvent;
        MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent;
        MaxSdkCallbacks.Banner.OnAdExpandedEvent += OnBannerAdExpandedEvent;
        MaxSdkCallbacks.Banner.OnAdCollapsedEvent += OnBannerAdCollapsedEvent;
#endif
    }
    
    private void LoadMediation()
    {
#if ADMOB_MEDIATION
        // TODO
        _bannerView.LoadAd(new AdRequest());
#else
        MaxSdk.CreateBanner(MaxAdUnitId, ToMediationBannerPosition(_currentPosition));
#endif
    }
   
    private void DestroyMediation()
    {
#if ADMOB_MEDIATION
        // TODO
        if (_bannerView == null)
        {
            return;
        }
        _bannerView.Destroy();
        _bannerView = null;
#else
        MaxSdk.DestroyBanner(MaxAdUnitId);
#endif
    }
    
    private void SetMediationPosition(BannerPosition bannerPosition)
    {
#if ADMOB_MEDIATION
        // TODO
        _bannerView?.SetPosition(ToMediationBannerPosition(bannerPosition));
#else
        MaxSdk.UpdateBannerPosition(MaxAdUnitId, ToMediationBannerPosition(bannerPosition));
#endif
    }
    
    private void ShowMediation()
    {
#if ADMOB_MEDIATION
        // TODO
        _bannerView.Show();
#else
        MaxSdk.ShowBanner(MaxAdUnitId);
#endif
    }
    
    private void HideMediation()
    {
#if ADMOB_MEDIATION
        // TODO
        _bannerView.Hide();
#else
        MaxSdk.HideBanner(MaxAdUnitId);
#endif
    }
    
    
#if ADMOB_MEDIATION
    // TODO
    private static AdPosition ToMediationBannerPosition(BannerPosition bannerPosition)
    {
        switch (bannerPosition)
        {
            case BannerPosition.TopCenter:
                return AdPosition.Top;
            case BannerPosition.BottomCenter:
                return AdPosition.Bottom;
        }
        return AdPosition.Top;
    }
#else
    private static MaxSdkBase.BannerPosition ToMediationBannerPosition(BannerPosition bannerPosition)
    {
        switch (bannerPosition)
        {
            case BannerPosition.TopCenter:
                return MaxSdkBase.BannerPosition.TopCenter;
            case BannerPosition.BottomCenter:
                return MaxSdkBase.BannerPosition.BottomCenter;
        }
        return MaxSdkBase.BannerPosition.TopCenter;
    }
#endif
    
    
    
#if ADMOB_MEDIATION
    // TODO
    private void OnBannerAdLoaded()
    {
        WorkerThread.Current.AddJob(() =>
        {
            Log("OnBannerAdLoaded");
            CurrentLoadState = LoadState.Success;
            ResetAttempts();
        });
    }
    private void OnBannerAdLoadFailed(LoadAdError error)
    {
        WorkerThread.Current.AddJob(() =>
        {
            LogError("OnBannerAdLoadFailed " + error.ToString());
            CurrentLoadState = LoadState.Fail;
            InvokeForLoad();
        });
    }
    private void OnAdPaid(AdValue adValue)
    {
    }
    private void OnAdImpressionRecorded()
    {
    }
    private void OnAdClicked()
    {
    }
    private void OnAdFullScreenContentOpened()
    {
    }
    private void OnAdFullScreenContentClosed()
    {
    }
#else
    private void OnBannerAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        CurrentLoadState = LoadState.Success;
    }

    private void OnBannerAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        CurrentLoadState = LoadState.Fail;
    }

    private void OnBannerAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
    }

    private void OnBannerAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
    }

    private void OnBannerAdExpandedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
    }

    private void OnBannerAdCollapsedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        
    }
#endif
    
#endregion

    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform offerFrame;
    [SerializeField] private RectTransform animPivot;
    [SerializeField] private GameObject loadingBar;
    [SerializeField] private Color backgroundColor;
    [SerializeField] private RectTransform topPivot;
    [SerializeField] private RectTransform bottomPivot;
    [SerializeField] private Button closeButton;

    [System.NonSerialized] private bool _overlayVisible = true;
    
    [System.NonSerialized] private BannerPosition _currentPosition = BannerPosition.BottomCenter;


    public enum BannerPosition
    {
        TopCenter,
        BottomCenter,
    }

    public BannerPosition Position
    {
        set
        {
            _currentPosition = value;

            SetMediationPosition(value);
            
            switch (_currentPosition)
            {
                case BannerPosition.TopCenter:
                    offerFrame.SetParent(topPivot);
                    offerFrame.rotation = topPivot.rotation;
                    closeButton.gameObject.SetActive(true);
                    break;
                case BannerPosition.BottomCenter:
                    offerFrame.SetParent(bottomPivot);
                    offerFrame.rotation = bottomPivot.rotation;
                    closeButton.gameObject.SetActive(false);
                    break;
                
            }
            offerFrame.localPosition = Vector3.zero;
            offerFrame.localScale = Vector3.one;
        }
    }
    
    private const float OfferDistance = -300.0f;
    
    public bool Ready => LoadState.Equals(LoadState.Success);
    public bool Visible => canvas.enabled;

    public LoadState CurrentLoadState
    {
        get => LoadState;
        private set
        {
            LoadState = value;

            switch (value)
            {
                case LoadState.None:
                    loadingBar.SetActive(true);
                    break;
                case LoadState.Success:
                    loadingBar.SetActive(false);
                    ShowAd();
                    break;
                case LoadState.Fail:
                    break;
                case LoadState.Loading:
                    loadingBar.SetActive(true);
                    break;
                case LoadState.Destroyed:
                    SetOfferState(false);
                    break;
            }
        }
    }
    
    public void ShowFrame()
    {
        SetOfferState(true);
        animPivot.DOKill();
        animPivot.anchoredPosition = new Vector2(0.0f, OfferDistance);
        animPivot.DOAnchorPosY(0.0f, 0.5f).SetEase(Ease.OutQuad).SetUpdate(true).SetDelay(0.15f);
    }

    public void SetOfferState(bool value)
    {
        canvas.enabled = value;
        this.gameObject.SetActive(value);
    }

    public void ShowAd()
    {
        if (!Ready)
        {
            ForwardInvoke();
            return;
        }
        if (!_overlayVisible)
        {
            return;
        }
        ShowMediation();
    }
    
    public void HideAdWithFrame()
    {
        _overlayVisible = false;
        HideMediation();
    }
    
    public void ShowAdWithFrame()
    {
        _overlayVisible = true;
        ShowAd();
    }
    
    public void Initialize()
    {
        InitializeMediation();
        CurrentLoadState = LoadState.None;
    }
    
    public override bool LoadAd()
    {
        if (base.LoadAd())
        {
            return true;
        }
        CurrentLoadState = LoadState.Loading;
        LoadMediation();
        return true;
    }

    public void DestroyBanner()
    {
        DestroyMediation();
        CurrentLoadState = LoadState.Destroyed;
    }
}
