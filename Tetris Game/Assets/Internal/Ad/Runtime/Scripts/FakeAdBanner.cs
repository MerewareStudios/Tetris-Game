using System.Collections;
using System.Collections.Generic;
using Internal.Core;
using UnityEngine;
using UnityEngine.UI;

public class FakeAdBanner : Lazyingleton<FakeAdBanner>
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasScaler canvasScaler;
    [SerializeField] private RectTransform fakeBanner;
    [SerializeField] private Color backgroundColor;
    [System.NonSerialized] private bool _loaded = false;
    [System.NonSerialized] public System.Action OnAdLoadedInternal;

    private const string BannerAdUnitId = "85fc6bf5a70ecf37";
    
    public void Show()
    {
        if (!_loaded)
        {
            return;
        }
        MaxSdk.ShowBanner(BannerAdUnitId);
        FakeAdBanner.THIS.OnBannerAdShownEvent();
    }
    
    public void Hide()
    {
        MaxSdk.HideBanner(BannerAdUnitId);
        FakeAdBanner.THIS.OnBannerAdHideEvent();
    } 
    
    public void Initialize()
    {
        MaxSdk.CreateBanner(BannerAdUnitId, MaxSdkBase.BannerPosition.BottomCenter);
        MaxSdk.SetBannerExtraParameter(BannerAdUnitId, "adaptive_banner", "true");
        MaxSdk.SetBannerBackgroundColor(BannerAdUnitId, backgroundColor);

        MaxSdkCallbacks.Banner.OnAdLoadedEvent      += OnBannerAdLoadedEvent;
        MaxSdkCallbacks.Banner.OnAdLoadFailedEvent  += OnBannerAdLoadFailedEvent;
        MaxSdkCallbacks.Banner.OnAdClickedEvent     += OnBannerAdClickedEvent;
        MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent;
        MaxSdkCallbacks.Banner.OnAdExpandedEvent    += OnBannerAdExpandedEvent;
        MaxSdkCallbacks.Banner.OnAdCollapsedEvent   += OnBannerAdCollapsedEvent;
    }

    private void OnBannerAdShownEvent()
    {
        Debug.Log("OnBannerAdShownEvent");
    }
    private void OnBannerAdHideEvent()
    {
        Debug.Log("OnBannerAdShownEvent");
    }
    
    private void OnBannerAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        _loaded = true;
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
