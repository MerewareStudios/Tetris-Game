using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Internal.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FakeAdBanner : Lazyingleton<FakeAdBanner>
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform offerFrame;
    [SerializeField] private Button enableButton;
    [SerializeField] private GameObject loadingBar;
    [SerializeField] private Color backgroundColor;
    [System.NonSerialized] private bool _loaded = false;
    [System.NonSerialized] public System.Action OnAdLoadedInternal;
    [System.NonSerialized] public System.Action OnOfferAccepted;
    [System.NonSerialized] public System.Action<bool> OnVisibilityChanged;
    [System.NonSerialized] private MaxSdkBase.BannerPosition _lastBannerPosition = MaxSdkBase.BannerPosition.BottomCenter;
    public Vector3 ButtonPosition => enableButton.transform.position;

    private const float OfferDistance = -220.0f;

    public const string BannerAdUnitId = "85fc6bf5a70ecf37";
    
    public void ShowOffer()
    {
        SetOfferState(true);

        // infoText.text = infoStr;

        enableButton.targetGraphic.raycastTarget = false;
        offerFrame.DOKill();

        offerFrame.anchoredPosition = new Vector2(0.0f, OfferDistance);
        offerFrame.DOAnchorPosY(0.0f, 0.5f).SetEase(Ease.OutQuad).SetDelay(0.5f).onComplete = () =>
        {
            enableButton.targetGraphic.raycastTarget = true;
        };

        Loading = !_loaded;
    }

    public bool Loading
    {
        set
        {
            loadingBar.SetActive(value);
            enableButton.gameObject.SetActive(!value);
        }
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
        if (!_loaded)
        {
            return;
        }
        MaxSdk.ShowBanner(BannerAdUnitId);
        OnVisibilityChanged?.Invoke(true);
    }
    
    public void HideAd()
    {
        MaxSdk.HideBanner(BannerAdUnitId);
        OnVisibilityChanged?.Invoke(false);
    }

    public void SetBannerPosition(MaxSdk.BannerPosition bannerPosition)
    {
        _lastBannerPosition = bannerPosition;
        MaxSdk.UpdateBannerPosition(BannerAdUnitId, bannerPosition);
    }
    
    public void Initialize()
    {
        MaxSdk.CreateBanner(BannerAdUnitId, _lastBannerPosition);
        MaxSdk.SetBannerExtraParameter(BannerAdUnitId, "adaptive_banner", "true");
        MaxSdk.SetBannerBackgroundColor(BannerAdUnitId, backgroundColor);

        
        MaxSdkCallbacks.Banner.OnAdLoadedEvent      += OnBannerAdLoadedEvent;
        MaxSdkCallbacks.Banner.OnAdLoadFailedEvent  += OnBannerAdLoadFailedEvent;
        MaxSdkCallbacks.Banner.OnAdClickedEvent     += OnBannerAdClickedEvent;
        MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent;
        MaxSdkCallbacks.Banner.OnAdExpandedEvent    += OnBannerAdExpandedEvent;
        MaxSdkCallbacks.Banner.OnAdCollapsedEvent   += OnBannerAdCollapsedEvent;
    }

    public void DestroyBanner()
    {
        MaxSdk.DestroyBanner(BannerAdUnitId);
    }

    private void OnBannerAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        _loaded = true;
        Loading = !_loaded;
        OnAdLoadedInternal?.Invoke();
    }

    private void OnBannerAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        Debug.Log("OnBannerAdLoadFailedEvent");
    }

    private void OnBannerAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("OnBannerAdClickedEvent");
    }

    private void OnBannerAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("OnBannerAdRevenuePaidEvent");
    }

    private void OnBannerAdExpandedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("OnBannerAdExpandedEvent");
    }

    private void OnBannerAdCollapsedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("OnBannerAdCollapsedEvent");
    }
}
