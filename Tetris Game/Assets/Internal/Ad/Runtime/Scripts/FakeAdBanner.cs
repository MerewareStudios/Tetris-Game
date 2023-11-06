using DG.Tweening;
using Internal.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FakeAdBanner : Lazyingleton<FakeAdBanner>
{
    private const string BannerAdUnitId = "85fc6bf5a70ecf37";
    
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform offerFrame;
    [SerializeField] private Button enableButton;
    [SerializeField] private GameObject loadingBar;
    [SerializeField] private Color backgroundColor;
    
    [System.NonSerialized] public System.Action OnOfferAccepted;
    [System.NonSerialized] public System.Action<bool> VisibilityChanged;
    [System.NonSerialized] private MaxSdkBase.BannerPosition _lastBannerPosition = MaxSdkBase.BannerPosition.BottomCenter;
    [System.NonSerialized] private bool _visible = false;
    
    public Vector3 ButtonPosition => enableButton.transform.position;
    private const float OfferDistance = -220.0f;
    
    public bool Ready => _loadState.Equals(LoadState.Success);

    [System.NonSerialized] public LoadState _loadState = LoadState.None;

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
                    enableButton.gameObject.SetActive(false);
                    break;
                case LoadState.Success:
                    loadingBar.SetActive(false);
                    enableButton.gameObject.SetActive(true);
                    break;
                case LoadState.Fail:
                    break;
                case LoadState.Loading:
                    loadingBar.SetActive(true);
                    enableButton.gameObject.SetActive(false);
                    break;
                case LoadState.Destroyed:
                    SetOfferState(false);
                    break;
            }
        }
    }
    
    public void ShowOffer()
    {
        SetOfferState(true);

        enableButton.targetGraphic.raycastTarget = false;
        offerFrame.DOKill();

        offerFrame.anchoredPosition = new Vector2(0.0f, OfferDistance);
        offerFrame.DOAnchorPosY(0.0f, 0.5f).SetEase(Ease.OutQuad).SetDelay(0.5f).onComplete = () =>
        {
            enableButton.targetGraphic.raycastTarget = true;
        };
    }

    public void HideOffer()
    {
        enableButton.targetGraphic.raycastTarget = false;
        offerFrame.DOKill();

        offerFrame.DOAnchorPosY(OfferDistance, 0.25f).SetEase(Ease.InSine).onComplete = () =>
        {
            SetOfferState(false);
        };
    }

    public void SetOfferState(bool value)
    {
        canvas.enabled = value;
        this.gameObject.SetActive(value);
    }

    public void OnClick_AcceptOffer()
    {
        HideOffer();
        OnOfferAccepted?.Invoke();
    }

    
    public void ShowAd()
    {
        if (!Ready)
        {
            return;
        }
        _visible = true;
        VisibilityChanged?.Invoke(true);
        MaxSdk.ShowBanner(BannerAdUnitId);
    }
    
    public void HideAd()
    {
        _visible = false;
        VisibilityChanged?.Invoke(false);
        MaxSdk.HideBanner(BannerAdUnitId);
    }

    public void SetBannerPosition(MaxSdk.BannerPosition bannerPosition)
    {
        if (!_visible)
        {
            return;
        }
        _lastBannerPosition = bannerPosition;
        MaxSdk.UpdateBannerPosition(BannerAdUnitId, bannerPosition);
    }
    
    public void Initialize()
    {
        MaxSdk.SetBannerExtraParameter(BannerAdUnitId, "adaptive_banner", "true");
        MaxSdk.SetBannerBackgroundColor(BannerAdUnitId, backgroundColor);
        
        MaxSdkCallbacks.Banner.OnAdLoadedEvent      += OnBannerAdLoadedEvent;
        MaxSdkCallbacks.Banner.OnAdLoadFailedEvent  += OnBannerAdLoadFailedEvent;
        MaxSdkCallbacks.Banner.OnAdClickedEvent     += OnBannerAdClickedEvent;
        MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent;
        MaxSdkCallbacks.Banner.OnAdExpandedEvent    += OnBannerAdExpandedEvent;
        MaxSdkCallbacks.Banner.OnAdCollapsedEvent   += OnBannerAdCollapsedEvent;
        
        CurrentLoadState = LoadState.None;
    }
    
    public void LoadAd()
    {
        CurrentLoadState = LoadState.Loading;
        MaxSdk.CreateBanner(BannerAdUnitId, _lastBannerPosition);
    }

    public void DestroyBanner()
    {
        _visible = false;
        MaxSdk.DestroyBanner(BannerAdUnitId);
        CurrentLoadState = LoadState.Destroyed;
    }

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
}
