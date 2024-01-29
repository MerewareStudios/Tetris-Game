using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomaGames.HomaBelly.Internal.Analytics;
using HomaGames.HomaBelly.Utilities;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Homa Bridge is the main connector between the public facade (HomaBelly)
    /// and all the inner behaviour of the Homa Belly library. All features
    /// and callbacks will be centralized within this class.
    /// </summary>
    public class HomaBridge : IHomaBellyBridge
    {
        #region Private properties

        private InitializationStatus initializationStatus = new InitializationStatus();
        private AnalyticsHelper analyticsHelper = new AnalyticsHelper();
        #endregion

        #region Public properties

        public bool IsInitialized
        {
            get
            {
                return initializationStatus.IsInitialized;
            }
        }

        #endregion

        public void Initialize()
        {
            // Instantiate
            int servicesCount = HomaBridgeServices.InstantiateServices();
            initializationStatus.SetComponentsToInitialize(servicesCount);
            
            // Try to auto configure analytics custom dimensions from NTesting
            // This is done before initializing to ensure all analytic events
            // properly gather the custom dimension
            AutoConfigureAnalyticsCustomDimensionsForNTesting();
            
            // Auto-track AdEvents
            RegisterAdEventsForAnalytics();
            InitializeAnalytics();
            
            analyticsHelper.Start();
        }

        public void InitializeInternetConnectivityDependantComponents()
        {
            // Initialize
            InitializeMediators();
            InitializeCustomerSupport();

            // Start initialization grace period timer
            initializationStatus.StartInitializationGracePeriod();
        }

        /// <summary>
        /// Initializes all those components that require from Remote Configuration
        /// data in order to initialize
        /// </summary>
        public void InitializeRemoteConfigurationDependantComponents(RemoteConfiguration.RemoteConfigurationModelEveryTime remoteConfigurationModel)
        {
            HomaGamesLog.Debug("[Homa Belly] Initializing Homa Belly after Remote Configuration fetch");
            CrossPromotionManager.Initialize(remoteConfigurationModel);
            InitializeAttributions(remoteConfigurationModel);
        }

        public void SetDebug(bool enabled)
        {

        }

        public void ValidateIntegration()
        {
            // Mediators
            if (HomaBridgeServices.GetMediators(out var mediators))
            {
                foreach (MediatorBase mediator in mediators)
                {
                    mediator.ValidateIntegration();
                }
            }
            
            // Old Mediators
            if (HomaBridgeServices.GetOldMediators(out var oldMediators))
            {
                foreach (IMediator mediator in oldMediators)
                {
                    mediator.ValidateIntegration();
                }
            }

            // Attributions
            if (HomaBridgeServices.GetAttributions(out var attributions))
            {
                foreach (IAttribution attribution in attributions)
                {
                    attribution.ValidateIntegration();
                }
            }

            // Analytics
            if (HomaBridgeServices.GetAnalytics(out var analytics))
            {
                foreach (AnalyticsBase analytic in analytics)
                {
                    analytic.ValidateIntegration();
                }
            }
        }

        public void OnApplicationPause(bool pause)
        {
            // Analytics Helper
            analyticsHelper.OnApplicationPause(pause);

            // Mediators
            if (HomaBridgeServices.GetMediators(out var mediators))
            {
                foreach (MediatorBase mediator in mediators)
                {
                    mediator.OnApplicationPause(pause);
                }
            }
            
            // Old Mediators
            if (HomaBridgeServices.GetOldMediators(out var oldMediators))
            {
                foreach (IMediator mediator in oldMediators)
                {
                    mediator.OnApplicationPause(pause);
                }
            }

            // Attributions
            if (HomaBridgeServices.GetAttributions(out var attributions))
            {
                foreach (IAttribution attribution in attributions)
                {
                    attribution.OnApplicationPause(pause);
                }
            }
        }

        #region IHomaBellyBridge

        public void LoadExtraRewardedVideoAd(string placementId)
        {
            if (HomaBridgeServices.GetMediators(out var mediators))
            {
                foreach (MediatorBase mediator in mediators)
                {
                    mediator.LoadRewardedVideoAd(placementId);
                }
            }
        }

        public void LoadHighValueRewardedVideoAd()
        {
            if (HomaBridgeServices.GetMediators(out var mediators))
            {
                foreach (MediatorBase mediator in mediators)
                {
                    mediator.LoadHighValueRewardedVideoAd();
                }
            }
        }

        public void ShowRewardedVideoAd(string placementName, string placementId = null)
        {
            Analytics.RewardedAdTriggered(placementName,placementId==null ? AdPlacementType.Default : AdPlacementType.User);

            if (HomaBridgeServices.GetMediators(out var mediators))
            {
                foreach (MediatorBase mediator in mediators)
                {
                    mediator.ShowRewardedVideoAd(placementId);
                }
            }

            if (HomaBridgeServices.GetOldMediators(out var oldMediators))
            {
                foreach (IMediator mediator in oldMediators)
                {
                    mediator.ShowRewardedVideoAd(placementId);
                }
            }
        }

        public void ShowHighValueRewardedVideoAd(string placementName)
        {
            Analytics.RewardedAdTriggered(placementName,AdPlacementType.HighValue);

            if (HomaBridgeServices.GetMediators(out var mediators))
            {
                foreach (MediatorBase mediator in mediators)
                {
                    mediator.ShowHighValueRewardedVideoAd();
                }
            }
        }

        public bool IsRewardedVideoAdAvailable(string placementId = null)
        {
            bool available = false;
            if (HomaBridgeServices.GetMediators(out var mediators) )
            {
                foreach (MediatorBase mediator in mediators)
                {
                    available |= mediator.IsRewardedVideoAdAvailable(placementId);
                }
            }

            if (HomaBridgeServices.GetOldMediators(out var oldMediators))
            {
                foreach (IMediator mediator in oldMediators)
                {
                    mediator.IsRewardedVideoAdAvailable(placementId);
                }
            }

            return available;
        }

        public bool IsHighValueRewardedVideoAdAvailable()
        {
            bool available = false;
            if (HomaBridgeServices.GetMediators(out var mediators) )
            {
                foreach (MediatorBase mediator in mediators)
                {
                    available |= mediator.IsHighValueRewardedVideoAdAvailable();
                }
            }

            return available;
        }

        // Banners
        public void LoadBanner(BannerSize size, BannerPosition position, string placementId = null, UnityEngine.Color bannerBackgroundColor = default)
        {
            if (HomaBridgeServices.GetMediators(out var mediators ))
            {
                foreach (MediatorBase mediator in mediators)
                {
                    mediator.LoadBanner(size, position, placementId, bannerBackgroundColor);
                }
            }

            if (HomaBridgeServices.GetOldMediators(out var oldMediators))
            {
                foreach (IMediator mediator in oldMediators)
                {
                    mediator.LoadBanner(size, position, placementId, bannerBackgroundColor);
                }
            }
        }

        public void ShowBanner(string placementId = null)
        {
            if (HomaBridgeServices.GetMediators(out var mediators))
            {
                foreach (MediatorBase mediator in mediators)
                {
                    mediator.ShowBanner(placementId);
                }
            }

            if (HomaBridgeServices.GetOldMediators(out var oldMediators))
            {
                foreach (IMediator mediator in oldMediators)
                {
                    mediator.ShowBanner(placementId);
                }
            }
        }

        public void HideBanner(string placementId = null)
        {
            if (HomaBridgeServices.GetMediators(out var mediators))
            {
                foreach (MediatorBase mediator in mediators)
                {
                    mediator.HideBanner(placementId);
                }
            }

            if (HomaBridgeServices.GetOldMediators(out var oldMediators))
            {
                foreach (IMediator mediator in oldMediators)
                {
                    mediator.HideBanner(placementId);
                }
            }
        }

        public void DestroyBanner(string placementId = null)
        {
            if (HomaBridgeServices.GetMediators(out var mediators))
            {
                foreach (MediatorBase mediator in mediators)
                {
                    mediator.DestroyBanner(placementId);
                }
            }

            if (HomaBridgeServices.GetOldMediators(out var oldMediators))
            {
                foreach (IMediator mediator in oldMediators)
                {
                    mediator.DestroyBanner(placementId);
                }
            }
        }

        public int GetBannerHeight(string placementId = null)
        {
            int size = 0;
            
            if (HomaBridgeServices.GetMediators(out var mediators))
            {
                foreach (MediatorBase mediator in mediators)
                {
                    size = Mathf.Max(size, mediator.GetBannerHeight(placementId));
                }
            }

            return size;
        }

        public void SetBannerPosition(BannerPosition position, string placementId = null)
        {
            if (HomaBridgeServices.GetMediators(out var mediators))
            {
                foreach (MediatorBase mediator in mediators)
                {
                    mediator.SetBannerPosition(position,placementId);
                }
            }
        }

        public void SetBannerBackgroundColor(Color color, string placementId = null)
        {
            if (HomaBridgeServices.GetMediators(out var mediators))
            {
                foreach (MediatorBase mediator in mediators)
                {
                    mediator.SetBannerBackgroundColor(color,placementId);
                }
            }
        }

        public List<BannerData> GetAllDisplayedBannerData()
        {
            var output = new List<BannerData>();
            if (HomaBridgeServices.GetMediators(out var mediators))
            {
                foreach (MediatorBase mediator in mediators)
                {
                    output.AddRange(mediator.GetVisibleBanners());
                }
            }

            return output;
        }

        public void LoadExtraInterstitial(string placementId)
        {
            if (HomaBridgeServices.GetMediators(out var mediators))
            {
                foreach (MediatorBase mediator in mediators)
                {
                    mediator.LoadInterstitial(placementId);
                }
            }
        }

        public void LoadHighValueInterstitial()
        {
            if (HomaBridgeServices.GetMediators(out var mediators))
            {
                foreach (MediatorBase mediator in mediators)
                {
                    mediator.LoadHighValueInterstitial();
                }
            }
        }

        public void ShowInterstitial(string placementName, string placementId = null)
        {
            Analytics.InterstitialAdTriggered(placementName,placementId==null? AdPlacementType.Default : AdPlacementType.User);
            if (HomaBridgeServices.GetMediators(out var mediators))
            {
                foreach (MediatorBase mediator in mediators)
                {
                    mediator.ShowInterstitial(placementId);
                }
            }

            if (HomaBridgeServices.GetOldMediators(out var oldMediators))
            {
                foreach (IMediator mediator in oldMediators)
                {
                    mediator.ShowInterstitial(placementId);
                }
            }
        }

        public void ShowHighValueInterstitial(string placementName)
        {
            Analytics.InterstitialAdTriggered(placementName,AdPlacementType.HighValue);
            if (HomaBridgeServices.GetMediators(out var mediators))
            {
                foreach (MediatorBase mediator in mediators)
                {
                    mediator.ShowHighValueInterstitial();
                }
            }
        }

        public bool IsInterstitialAvailable(string placementId = null)
        {
            bool available = false;
            if (HomaBridgeServices.GetMediators(out var mediators))
            {
                foreach (MediatorBase mediator in mediators)
                {
                    available |= mediator.IsInterstitialAvailable(placementId);
                }
            }

            if (HomaBridgeServices.GetOldMediators(out var oldMediators))
            {
                foreach (IMediator mediator in oldMediators)
                {
                    available |= mediator.IsInterstitialAvailable(placementId);
                }
            }

            return available;
        }

        public bool IsHighValueInterstitialAvailable()
        {
            bool available = false;
            if (HomaBridgeServices.GetMediators(out var mediators) )
            {
                foreach (MediatorBase mediator in mediators)
                {
                    available |= mediator.IsHighValueInterstitialAvailable();
                }
            }

            return available;
        }

        public void SetUserIsAboveRequiredAge(bool consent)
        {
            // For mediators
            if (HomaBridgeServices.GetMediators(out var mediators))
            {
                foreach (MediatorBase mediator in mediators)
                {
                    mediator.SetUserIsAboveRequiredAge(consent);
                }
            }

            // For old Mediators
            if (HomaBridgeServices.GetOldMediators(out var oldMediators))
            {
                foreach (IMediator mediator in oldMediators)
                {
                    mediator.SetUserIsAboveRequiredAge(consent);
                }
            }

            // For attributions
            if (HomaBridgeServices.GetAttributions(out var attributions))
            {
                foreach (IAttribution attribution in attributions)
                {
                    attribution.SetUserIsAboveRequiredAge(consent);
                }
            }

            // For analytics
            if (HomaBridgeServices.GetAnalytics(out var analytics))
            {
                foreach (AnalyticsBase analytic in analytics)
                {
                    analytic.SetUserIsAboveRequiredAge(consent);
                }
            }
        }

        public void SetTermsAndConditionsAcceptance(bool consent)
        {
            // For mediators
            if (HomaBridgeServices.GetMediators(out var mediators))
            {
                foreach (MediatorBase mediator in mediators)
                {
                    mediator.SetTermsAndConditionsAcceptance(consent);
                }
            }

            // For old Mediators
            if (HomaBridgeServices.GetOldMediators(out var oldMediators))
            {
                foreach (IMediator mediator in oldMediators)
                {
                    mediator.SetTermsAndConditionsAcceptance(consent);
                }
            }

            // For attributions
            if (HomaBridgeServices.GetAttributions(out var attributions))
            {
                foreach (IAttribution attribution in attributions)
                {
                    attribution.SetTermsAndConditionsAcceptance(consent);
                }
            }

            // For analytics
            if (HomaBridgeServices.GetAnalytics(out var analytics))
            {
                foreach (AnalyticsBase analytic in analytics)
                {
                    analytic.SetTermsAndConditionsAcceptance(consent);
                }
            }
        }

        public void SetAnalyticsTrackingConsentGranted(bool consent)
        {
            // For mediators
            if (HomaBridgeServices.GetMediators(out var mediators))
            {
                foreach (MediatorBase mediator in mediators)
                {
                    mediator.SetAnalyticsTrackingConsentGranted(consent);
                }
            }

            // For old Mediators
            if (HomaBridgeServices.GetOldMediators(out var oldMediators))
            {
                foreach (IMediator mediator in oldMediators)
                {
                    mediator.SetAnalyticsTrackingConsentGranted(consent);
                }
            }

            // For attributions
            if (HomaBridgeServices.GetAttributions(out var attributions))
            {
                foreach (IAttribution attribution in attributions)
                {
                    attribution.SetAnalyticsTrackingConsentGranted(consent);
                }
            }

            // For analytics
            if (HomaBridgeServices.GetAnalytics(out var analytics))
            {
                foreach (AnalyticsBase analytic in analytics)
                {
                    analytic.SetAnalyticsTrackingConsentGranted(consent);
                }
            }
        }

        public void SetTailoredAdsConsentGranted(bool consent)
        {
            // For mediators
            if (HomaBridgeServices.GetMediators(out var mediators))
            {
                foreach (MediatorBase mediator in mediators)
                {
                    mediator.SetTailoredAdsConsentGranted(consent);
                }
            }

            // For old Mediators
            if (HomaBridgeServices.GetOldMediators(out var oldMediators))
            {
                foreach (IMediator mediator in oldMediators)
                {
                    mediator.SetTailoredAdsConsentGranted(consent);
                }
            }

            // For attributions
            if (HomaBridgeServices.GetAttributions(out var attributions))
            {
                foreach (IAttribution attribution in attributions)
                {
                    attribution.SetTailoredAdsConsentGranted(consent);
                }
            }

            // For analytics
            if (HomaBridgeServices.GetAnalytics(out var analytics))
            {
                foreach (AnalyticsBase analytic in analytics)
                {
                    analytic.SetTailoredAdsConsentGranted(consent);
                }
            }
        }

#if UNITY_PURCHASING
        public void TrackInAppPurchaseEvent(UnityEngine.Purchasing.Product product, bool isRestored = false)
        {
        }
#endif

        public void TrackInAppPurchaseEvent(string productId, string currencyCode, double unitPrice, string transactionId = null, string payload = null, bool isRestored = false)
        {
        }
        
        public void TrackResourceEvent(ResourceFlowType flowType, string currency, float amount, string itemType, string itemId)
        {
        }

        public void TrackProgressionEvent(ProgressionStatus progressionStatus, string progression01, int score = 0)
        {
        }

        public void TrackProgressionEvent(ProgressionStatus progressionStatus, string progression01, string progression02, int score = 0)
        {
        }

        public void TrackProgressionEvent(ProgressionStatus progressionStatus, string progression01, string progression02, string progression03, int score = 0)
        {
        }

        public void TrackErrorEvent(ErrorSeverity severity, string message)
        {
            if (HomaBridgeServices.GetAnalytics(out var analytics))
            {
                foreach (AnalyticsBase analytic in analytics)
                {
                    analytic.TrackErrorEvent(severity, message);
                }
            }
        }

        public void TrackDesignEvent(string eventName, float eventValue = 0f)
        {
        }

        public void TrackAdEvent(AdAction adAction, AdType adType, string adNetwork, string adPlacementId)
        {
        }

        public void TrackAdRevenue(AdRevenueData adRevenueData)
        {
            if (HomaBridgeServices.GetAttributions(out var attributions))
            {
                foreach (IAttribution attribution in attributions)
                {
                    attribution.TrackAdRevenue(adRevenueData);
                }
            }

            if (HomaBridgeServices.GetAnalytics(out var analytics))
            {
                foreach (AnalyticsBase analytic in analytics)
                {
                    analytic.TrackAdRevenue(adRevenueData);
                }
            }
        }

        public void TrackAttributionEvent(string eventName, Dictionary<string, object> arguments = null)
        {
            if (HomaBridgeServices.GetAttributions(out var attributions))
            {
                foreach (IAttribution attribution in attributions)
                {
                    attribution.TrackEvent(eventName, arguments);
                }
            }
        }

        public void TrackAttributionEventWithPartnerParameters(string eventName,
            Dictionary<string, object> partnerParameters, Dictionary<string, object> arguments = null)
        {
            if (HomaBridgeServices.GetAttributions(out var attributions))
            {
                foreach (IAttribution attribution in attributions)
                {
                    attribution.TrackEventWithPartnerParameters(eventName, partnerParameters, arguments);
                }
            }
        }

        public void SetCustomDimension01(string customDimension)
        {
            if (HomaBridgeServices.GetAnalytics(out var analytics))
            {
                foreach (AnalyticsBase analytic in analytics)
                {
                    analytic.SetCustomDimension01(customDimension);
                }
            }
        }

        public void SetCustomDimension02(string customDimension)
        {
            if (HomaBridgeServices.GetAnalytics(out var analytics))
            {
                foreach (AnalyticsBase analytic in analytics)
                {
                    analytic.SetCustomDimension02(customDimension);
                }
            }
        }

        public void SetCustomDimension03(string customDimension)
        {
            if (HomaBridgeServices.GetAnalytics(out var analytics))
            {
                foreach (AnalyticsBase analytic in analytics)
                {
                    analytic.SetCustomDimension03(customDimension);
                }
            }
        }

        #endregion

        #region Private helpers

        private void RegisterAdEventsForAnalytics()
        {
            // Interstitial
            Events.onInterstitialAdShowSucceededEvent += (id) =>
            {
                analyticsHelper.OnInterstitialAdWatched(id.PlacementId);
            };

            // Rewarded Video
            Events.onRewardedVideoAdRewardedEvent += (reward,adInfo) =>
            {
                analyticsHelper.OnRewardedVideoAdWatched(reward.getPlacementName());
            };
        }

        private void AutoConfigureAnalyticsCustomDimensionsForNTesting()
        {
            // This is required after implementing Geryon <> Analytics automatic integration
            // and assign ExternalTokens to Custom Dimensions
            string customDimension01 = Geryon.Config.ExternalToken0; 
            string customDimension02 = Geryon.Config.ExternalToken1;
            string customDimension03 = Geryon.Config.ExternalToken2;
            
            if (!string.IsNullOrEmpty(customDimension01))
            {
                HomaGamesLog.Debug($"Setting Game Analytics custom dimension 01 to: {customDimension01}");
                SetCustomDimension01(customDimension01);
            }

            if (!string.IsNullOrEmpty(customDimension02))
            {
                HomaGamesLog.Debug($"Setting Game Analytics custom dimension 02 to: {customDimension02}");
                SetCustomDimension02(customDimension02);
            }

            if (!string.IsNullOrEmpty(customDimension03))
            {
                HomaGamesLog.Debug($"Setting Game Analytics custom dimension 03 to: {customDimension03}");
                SetCustomDimension03(customDimension03);
            }
        }

        #endregion

        #region Mediators

        private void InitializeMediators()
        {
            if (!HomaBridgeServices.GetMediators(out var mediators))
            {
                HomaGamesLog.Warning($"[Homa Belly] No mediators found in this project.");
                return;
            }
            
            foreach (MediatorBase mediator in mediators)
            {
                try
                {
                    mediator.Initialize(initializationStatus.OnInnerComponentInitialized);
                }
                catch (Exception e)
                {
                    HomaGamesLog.Warning($"[Homa Belly] Exception initializing {mediator}: {e.Message}");
                }
            }
            
            // For old Mediators
            HomaBridgeServices.GetOldMediators(out var oldMediators);
            
            foreach (IMediator mediator in oldMediators)
            {
                try
                {
#pragma warning disable CS0618
                    if (mediator is IMediatorWithInitializationCallback mediatorWithInitializationCallback)
#pragma warning restore CS0618
                        mediatorWithInitializationCallback.Initialize(initializationStatus.OnInnerComponentInitialized);
                    else
                        mediator.Initialize();
                    
                    mediator.RegisterEvents();
                }
                catch (Exception e)
                {
                    HomaGamesLog.Warning($"[Homa Belly] Exception initializing {mediator}: {e.Message}");
                }
            }
        }

#endregion

#region Attributions

        private void InitializeAttributions(RemoteConfiguration.RemoteConfigurationModelEveryTime remoteConfigurationEveryTime)
        {
            if (!HomaBridgeServices.GetAttributions(out var attributions))
            {
                HomaGamesLog.Warning($"[Homa Belly] No attribution services found in this project.");
                return;
            }

            if (remoteConfigurationEveryTime?.AttributionConfigurationModel?.DisableSingular == true)
            {
                attributions.RemoveAll(attribution => attribution.GetType().FullName?.Contains("Singular") == true);
            }
            
            string nTestingId = Geryon.Config.NTESTING_ID;

            foreach (IAttribution attribution in attributions)
            {
                try
                {
                    // Homa Belly v1.2.0+
                    (attribution as IAttributionWithInitializationCallback).Initialize(nTestingId, initializationStatus.OnInnerComponentInitialized);
                }
                catch (Exception e)
                {
                    HomaGamesLog.Warning($"[Homa Belly] Exception initializing {attribution}: {e.Message}");
                }
            }
        }

#endregion

#region Analytics

        private void InitializeAnalytics()
        {
            if (!HomaBridgeServices.GetAnalytics(out var analytics))
            {
                HomaGamesLog.Warning($"[Homa Belly] No analytics services found in this project.");
                return;
            }
            
            foreach (AnalyticsBase analytic in analytics)
            {
                try
                {
                    analytic.Initialize(initializationStatus.OnInnerComponentInitialized);
                }
                catch (Exception e)
                {
                    HomaGamesLog.Warning($"[Homa Belly] Exception initializing {analytic}: {e.Message}");
                }
            }
        }

#endregion

#region Customer Support

    private void InitializeCustomerSupport()
    {
        if (!HomaBridgeServices.GetCustomerSupport(out var customerSupport))
            return;
        try
        {
            customerSupport.Initialize();
        }
        catch (Exception e)
        {
            HomaGamesLog.Warning($"[Homa Belly] Exception initializing {customerSupport}: {e.Message}");
        }
    }

#endregion
    }
}
