using DG.Tweening;
using Internal.Core;
using UnityEngine;
using UnityEngine.UI;

public class FakeAdBanner : Lazyingleton<FakeAdBanner>
{
#region Mediation Variables

#if ADMOB_MEDIATION
            
    // TODO
#else
    private const string MaxAdUnitId = "85fc6bf5a70ecf37";

#endif
    
    private void InitializeMediation()
    {
#if ADMOB_MEDIATION
        // TODO
#else
        MaxSdk.SetBannerExtraParameter(MaxAdUnitId, "adaptive_banner", "true");
        MaxSdk.SetBannerBackgroundColor(MaxAdUnitId, backgroundColor);
        
        MaxSdkCallbacks.Banner.OnAdLoadedEvent      += OnBannerAdLoadedEvent;
        MaxSdkCallbacks.Banner.OnAdLoadFailedEvent  += OnBannerAdLoadFailedEvent;
        MaxSdkCallbacks.Banner.OnAdClickedEvent     += OnBannerAdClickedEvent;
        MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent;
        MaxSdkCallbacks.Banner.OnAdExpandedEvent    += OnBannerAdExpandedEvent;
        MaxSdkCallbacks.Banner.OnAdCollapsedEvent   += OnBannerAdCollapsedEvent;
#endif
    }
    
    private void LoadMediation()
    {
#if ADMOB_MEDIATION
        // TODO
#else
        MaxSdk.CreateBanner(MaxAdUnitId, ToMediationBannerPosition(_currentPosition));
#endif
    }
    
    private void DestroyMediation()
    {
#if ADMOB_MEDIATION
        // TODO
#else
        MaxSdk.DestroyBanner(MaxAdUnitId);
#endif
    }
    
    private void SetMediationPosition(BannerPosition bannerPosition)
    {
#if ADMOB_MEDIATION
        // TODO
#else
        MaxSdk.UpdateBannerPosition(MaxAdUnitId, ToMediationBannerPosition(bannerPosition));
#endif
    }
    
    private void ShowMediation()
    {
#if ADMOB_MEDIATION
        // TODO
#else
        MaxSdk.ShowBanner(MaxAdUnitId);
#endif
    }
    
    private void HideMediation()
    {
#if ADMOB_MEDIATION
        // TODO
#else
        MaxSdk.HideBanner(MaxAdUnitId);
#endif
    }
    
    
#if ADMOB_MEDIATION
    // TODO
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
    
    public bool Ready => _loadState.Equals(LoadState.Success);
    public bool Visible => canvas.enabled;

    [System.NonSerialized] private LoadState _loadState = LoadState.None;

    public LoadState CurrentLoadState
    {
        get => _loadState;
        private set
        {
            _loadState = value;

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
            return;
        }
        if (!_overlayVisible)
        {
            return;
        }
        ShowMediation();
    }
    
    public void HideAd(bool hide)
    {
        _overlayVisible = !hide;

        if (hide)
        {
            HideMediation();
        }
        else
        {
            ShowAd();
        }
    }
    
    public void Initialize()
    {
        InitializeMediation();
        CurrentLoadState = LoadState.None;
    }
    
    public void LoadAd()
    {
        CurrentLoadState = LoadState.Loading;
        LoadMediation();
    }

    public void DestroyBanner()
    {
        DestroyMediation();
        CurrentLoadState = LoadState.Destroyed;
    }

  
}
