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

#if UNITY_IOS
string bannerAdUnitId = "YOUR_IOS_BANNER_AD_UNIT_ID"; // Retrieve the ID from your account
#else // UNITY_ANDROID
    private const string BannerAdUnitId = "85fc6bf5a70ecf37"; // Retrieve the ID from your account
#endif
    
    public static void Show()
    {
        FakeAdBanner.THIS.canvas.enabled = true;
        
        MaxSdk.ShowBanner(BannerAdUnitId);
    }
    
    public static void Hide()
    {
        MaxSdk.HideBanner(BannerAdUnitId);
        FakeAdBanner.THIS.canvas.enabled = false;
    }
    
    public float GetBannerHeight()
    {
        // if (Screen.height <= 400*Mathf.RoundToInt(Screen.dpi/160)) 
        // {
        //     return 32*Mathf.RoundToInt(Screen.dpi/160);
        // } 
        // else if (Screen.height <= 720*Mathf.RoundToInt(Screen.dpi/160)) 
        // {
        //     return 50*Mathf.RoundToInt(Screen.dpi/160);
        // } 
        // else 
        // {
        //     return 90*Mathf.RoundToInt(Screen.dpi/160);
        // }
        
        // float f = Screen.dpi / 160f;
        // float dp = Screen.height / f;
        // Debug.Log(dp);
        // if (dp>720f) 
        // {
        //     return 90f * f;
        // } 
        // else if (dp>400f) 
        // {
        //     return 50f * f;
        // } 
        // else 
        // {
        //     return 32f * f;
        // }
        
            // if (Screen.height <= 400*Mathf.RoundToInt(Screen.dpi/160)) {
            //     return 32*Mathf.RoundToInt(Screen.dpi/160);
            // } else if (Screen.height <= 720*Mathf.RoundToInt(Screen.dpi/160)) {
            //     return 50*Mathf.RoundToInt(Screen.dpi/160);
            // } else {
            //     return 90*Mathf.RoundToInt(Screen.dpi/160);
            // }

        // Debug.Log(canvasScaler.scaleFactor);
        // float bannerSizePixels = Screen.height <= 400 ? 32 : Screen.height < 720 ? 50 : 90;
        // var percent = (100f / Screen.height) * bannerSizePixels;
        // var bannerSize = canvasScaler.referenceResolution.y * (percent / 100f);
        // return bannerSize;
        // return Mathf.RoundToInt(50 * (Screen.dpi / 160.0f));

        Debug.Log(Helper.Pixel2Dp(Screen.height));

        float heightDp = Helper.Pixel2Dp(Screen.height);
        return heightDp switch
        {
            > 720.0f => Helper.Dp2Pixel(90),
            > 400.0f => Helper.Dp2Pixel(50),
            _ => Helper.Dp2Pixel(32)
        };
    }

    
    public void Initialize()
    {
        // Banners are automatically sized to 320×50 on phones and 728×90 on tablets
        // You may call the utility method MaxSdkUtils.isTablet() to help with view sizing adjustments
        MaxSdk.CreateBanner(BannerAdUnitId, MaxSdkBase.BannerPosition.BottomCenter);
        MaxSdk.SetBannerExtraParameter(BannerAdUnitId, "adaptive_banner", "true");
        
        // Debug.Log(MaxSdkUtils.GetAdaptiveBannerHeight());

        // Set background or background color for banners to be fully functional
        MaxSdk.SetBannerBackgroundColor(BannerAdUnitId, backgroundColor);
        
        
        
        // There may be cases when you would like to stop auto-refresh, for instance, if you want to manually refresh banner ads. To stop auto-refresh for a banner ad, use the following code:
        // MaxSdk.StopBannerAutoRefresh(bannerAdUnitId);
        // Start auto-refresh for a banner ad with the following code:
        // MaxSdk.StartBannerAutoRefresh(bannerAdUnitId);
        // Manually refresh the contents with the following code. Note that you must stop auto-refresh before you call LoadBanner().
        // MaxSdk.LoadBanner(bannerAdUnitId);
        

        MaxSdkCallbacks.Banner.OnAdLoadedEvent      += OnBannerAdLoadedEvent;
        MaxSdkCallbacks.Banner.OnAdLoadFailedEvent  += OnBannerAdLoadFailedEvent;
        MaxSdkCallbacks.Banner.OnAdClickedEvent     += OnBannerAdClickedEvent;
        MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent;
        MaxSdkCallbacks.Banner.OnAdExpandedEvent    += OnBannerAdExpandedEvent;
        MaxSdkCallbacks.Banner.OnAdCollapsedEvent   += OnBannerAdCollapsedEvent;


        float bannerHeight = GetBannerHeight();
        Debug.Log(bannerHeight);
        fakeBanner.sizeDelta = new Vector2(fakeBanner.sizeDelta.x, bannerHeight);
    }

    private void OnBannerAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {}

    private void OnBannerAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo) {}

    private void OnBannerAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {}

    private void OnBannerAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {}

    private void OnBannerAdExpandedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)  {}

    private void OnBannerAdCollapsedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {}
}
