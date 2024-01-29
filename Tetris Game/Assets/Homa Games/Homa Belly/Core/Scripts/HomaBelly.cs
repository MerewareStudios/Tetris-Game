 using System;
using System.Collections.Generic;
 using System.Dynamic;
 using System.Threading.Tasks;
 using JetBrains.Annotations;
 using UnityEngine;
using UnityEngine.Scripting;

namespace HomaGames.HomaBelly
{
    public class HomaBelly : IHomaBelly
    {
        #region Constants
        /// <summary>
        /// If this define symbol is set in the build settings, logs will be enabled.
        /// </summary>
        public const string LOGS_ENABLED_DEFINE_SYMBOL = "HOMA_DEVELOPMENT";
        #endregion

        #region Public properties
        public bool IsInitialized
        {
            get
            {
                return homaBridge.IsInitialized;
            }
        }
        #endregion

        #region Singleton pattern

        private static HomaBelly _instance;
        public static HomaBelly Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new HomaBelly();
                }

                return _instance;
            }
        }

#endregion

#region Private properties
#if UNITY_EDITOR && UNITY_2019_1_OR_NEWER
        private static IHomaBellyBridge homaBridge = new HomaDummyBridge();
#else
        private static IHomaBellyBridge homaBridge = new HomaBridge();
#endif
        private Events events = new Events();

        /// <summary>
        /// If network is not reachable, API calls will be stored in
        /// this queue. All Actions will be triggered when network is
        /// reachable again
        /// </summary>
        private static Queue<Action> unreachableNetworkActionQueue = new Queue<Action>();
        private const int REACHABILITY_WAIT_MS = 3000;
        private static Task unreachableNetworkTaskDelay;
        private static bool IsNetworkReachable
        {
            get
            {
                return Application.internetReachability != NetworkReachability.NotReachable;
            }
        }
#endregion

        internal static async Task Initialize()
        {            
#if !HOMA_DEVELOPMENT && !UNITY_EDITOR
            UnityEngine.Debug.Log("<color=#41CBA5>[Homa Belly] All logs have been disabled in this build to improve the performance. If you want to have logs, you have to toggle ON 'Development Build' in Homa Belly window.</color>");
#endif
            await ThreadUtils.RunOnMainThreadAsync(() =>
            {
                homaBridge.Initialize();
                
                CheckNetworkReachabilityBeforeAction(() => homaBridge.InitializeInternetConnectivityDependantComponents());
            });
            SetupComponentProxy();
        }

        private static void SetupComponentProxy()
        {
            var proxyGameObject = new GameObject("HomaBellyProxy");
            HomaBellyComponentProxy componentProxy = proxyGameObject.AddComponent<HomaBellyComponentProxy>();
            componentProxy.SetOnApplicationPause(Instance.OnApplicationPause);
        }

        internal static Task InitializeRemoteConfigurationDependantComponents(RemoteConfiguration.RemoteConfigurationModelEveryTime remoteConfigurationModel)
        {
            CheckNetworkReachabilityBeforeAction(() => homaBridge.InitializeRemoteConfigurationDependantComponents(remoteConfigurationModel));
            return Task.CompletedTask;
        }

        private void OnApplicationPause(bool pause)
        {
            homaBridge.OnApplicationPause(pause);
        }

        /// <summary>
        /// Method filtering API calls on network reachabiliy.
        /// If reachable, executes it. If not, stores in in the
        /// #unreachableNetworkActionQueue to be executed when
        /// network is reachable again
        /// </summary>
        /// <param name="action">The action to be executed</param>
        private static void CheckNetworkReachabilityBeforeAction(Action action)
        {
            if (IsNetworkReachable)
            {
                action.Invoke();
            }
            else
            {
                HomaGamesLog.Warning($"[Homa Belly] Network not reachable. Deferring call to {action} until network available again.");
                unreachableNetworkActionQueue.Enqueue(action);
                WaitForReachability();
            }
        }

        /// <summary>
        /// Method triggered when an API call is done and network is not reachable.
        /// This method will loop until network is reachable and execute
        /// all the deferred actions
        /// </summary>
        private static void WaitForReachability()
        {
            // If network recovery task is already running
            if (unreachableNetworkTaskDelay == null
                || unreachableNetworkTaskDelay.IsCanceled
                || unreachableNetworkTaskDelay.IsCompleted
                || unreachableNetworkTaskDelay.IsFaulted)
            {
                // Task waiting REACHABILITY_WAIT_MS to retry any API call
                // If network is not reachable after that time, a new task
                // will be scheduled to try again, until network is reachable
                unreachableNetworkTaskDelay = Task.Delay(REACHABILITY_WAIT_MS)
                    .ContinueWithOnMainThread(result =>
                    {
                        // If network is reachable and actions need to be
                        // triggered, proceed
                        if (IsNetworkReachable
                            && unreachableNetworkActionQueue != null
                            && unreachableNetworkActionQueue.Count > 0)
                        {
                            HomaGamesLog.Debug($"[Homa Belly] Network recovered. Executing deferred actions");
                            while (unreachableNetworkActionQueue.Count > 0)
                            {
                                Action action = unreachableNetworkActionQueue.Dequeue();
                                if (action != null)
                                {
                                    action.Invoke();
                                }
                            }

                            HomaGamesLog.Debug($"[Homa Belly] Deferred actions successfully invoked");
                        }
                    })
                    .ContinueWithOnMainThread(result =>
                    {
                        // Network recovery task is done
                        unreachableNetworkTaskDelay = null;

                        // If network is still not reachable, reset task
                        if (!IsNetworkReachable)
                        { 
                            WaitForReachability();
                        }
                    });
            }
        }
        
                
        [NotNull, ItemNotNull, Pure]
        private static List<IAttribution> GetAllAttributionDependencies()
        {
#if UNITY_EDITOR
            return new List<IAttribution>();
#else
            return HomaBridgeServices.GetAttributions(out var attributions) ? attributions : new List<IAttribution>();
#endif
        }

        #region IHomaBellyBridge

        public void ValidateIntegration()
        {
            homaBridge.ValidateIntegration();
        }

        // Rewarded Video Ads
        public void LoadExtraRewardedVideoAd(string placementId)
        {
            AfterInitialization(() => homaBridge.LoadExtraRewardedVideoAd(placementId));
        }

        public void LoadHighValueRewardedVideoAd()
        {
            AfterInitialization(homaBridge.LoadHighValueRewardedVideoAd);
        }

        public void ShowRewardedVideoAd(string placementName, string placementId = null)
        {
            homaBridge.ShowRewardedVideoAd(placementName, placementId);
        }

        public void ShowHighValueRewardedVideoAd(string placementName)
        {
            homaBridge.ShowHighValueRewardedVideoAd(placementName);
        }

        public bool IsRewardedVideoAdAvailable(string placementId = null)
        {
            return homaBridge.IsRewardedVideoAdAvailable(placementId);
        }

        public bool IsHighValueRewardedVideoAdAvailable()
        {
            return homaBridge.IsHighValueRewardedVideoAdAvailable();
        }

        // Banners
        public void LoadBanner(string placementId)
        {
            LoadBanner(BannerSize.BANNER, BannerPosition.BOTTOM, placementId);
        }

        public void LoadBanner(UnityEngine.Color bannerBackgroundColor, string placementId = null)
        {
            LoadBanner(BannerSize.BANNER, BannerPosition.BOTTOM, placementId, bannerBackgroundColor);
        }

        public void LoadBanner(BannerPosition position, string placementId = null)
        {
            LoadBanner(BannerSize.BANNER, position, placementId);
        }

        public void LoadBanner(BannerSize size, string placementId = null)
        {
            LoadBanner(size, BannerPosition.BOTTOM, placementId);
        }

        public void LoadBanner(BannerSize size = BannerSize.BANNER, BannerPosition position = BannerPosition.BOTTOM, string placementId = null, Color bannerBackgroundColor = default)
        {
            homaBridge.LoadBanner(size, position, placementId, bannerBackgroundColor);
        }

        public void ShowBanner(string placementId = null)
        {
            homaBridge.ShowBanner(placementId);
        }

        public void HideBanner(string placementId = null)
        {
            homaBridge.HideBanner(placementId);
        }

        public void DestroyBanner(string placementId = null)
        {
            homaBridge.DestroyBanner(placementId);
        }

        public int GetBannerHeight(string placementId = null)
        {
            return homaBridge.GetBannerHeight(placementId);
        }

        public void SetBannerPosition(BannerPosition position, string placementId = null)
        {
            homaBridge.SetBannerPosition(position,placementId);
        }

        public void SetBannerBackgroundColor(Color color, string placementId = null)
        {
            homaBridge.SetBannerBackgroundColor(color,placementId);
        }

        public List<BannerData> GetAllDisplayedBannerData()
        {
            return homaBridge.GetAllDisplayedBannerData();
        }

        public void LoadExtraInterstitial(string placementId)
        {
            AfterInitialization(() => homaBridge.LoadExtraInterstitial(placementId));
        }

        public void LoadHighValueInterstitial()
        {
            AfterInitialization(homaBridge.LoadHighValueInterstitial);
        }

        public void ShowInterstitial(string placementName, string placementId = null)
        {
            homaBridge.ShowInterstitial(placementName, placementId);
        }

        public void ShowHighValueInterstitial(string placementName)
        {
            homaBridge.ShowHighValueInterstitial(placementName);
        }

        public bool IsInterstitialAvailable(string placementId = null)
        {
            return homaBridge.IsInterstitialAvailable(placementId);
        }

        public bool IsHighValueInterstitialAvailable()
        {
            return homaBridge.IsHighValueInterstitialAvailable();
        }

        [Preserve]
        public void SetUserIsAboveRequiredAge(bool consent)
        {
            homaBridge.SetUserIsAboveRequiredAge(consent);
        }

        [Preserve]
        public void SetTermsAndConditionsAcceptance(bool consent)
        {
            homaBridge.SetTermsAndConditionsAcceptance(consent);
        }

        [Preserve]
        public void SetAnalyticsTrackingConsentGranted(bool consent)
        {
            homaBridge.SetAnalyticsTrackingConsentGranted(consent);
        }

        [Preserve]
        public void SetTailoredAdsConsentGranted(bool consent)
        {
            homaBridge.SetTailoredAdsConsentGranted(consent);
        }

#if UNITY_PURCHASING
        [Obsolete("Use HomaGames.HomaBelly.Analytics.TrackInAppPurchaseEvent instead")]
        public void TrackInAppPurchaseEvent(UnityEngine.Purchasing.Product product, bool isRestored = false)
        {
            AnalyticsEventTracker.TrackInAppPurchaseEvent(product, isRestored);
        }
#endif

        [Obsolete("InAppPurchase events are automated in HomaBelly Purchases package. You don't need to call this method anymore.")]
        public void TrackInAppPurchaseEvent(string productId, string currencyCode, double unitPrice, string transactionId = null, string payload = null, bool isRestored = false)
        {
        }

        [Obsolete("Use HomaGames.HomaBelly.Analytics.ResourceFlowEvent or HomaGames.HomaBelly.Analytics.ItemObtained instead")]
        public void TrackResourceEvent(ResourceFlowType flowType, string currency, float amount, string itemType, string itemId)
        {
            HomaGamesLog.Warning($"[Analytics] Obsolete TrackResourceEvent method. Please use HomaGames.HomaBelly.Analytics.ResourceFlowEvent or HomaGames.HomaBelly.Analytics.ItemObtained instead");
            Analytics.ResourceFlowEvent(flowType, currency, amount, 0, itemType, itemId, ResourceFlowReason.Obsolete);

            // In order to still support legacy invocations to this method, if there is an informed `itemId`
            // we send an `ItemObtained` or `ItemConsumed` event depending on the `ResourceFlowType`
            if (!string.IsNullOrEmpty(itemId))
            {
                switch (flowType)
                {
                    case ResourceFlowType.Undefined:
                        break;
                    case ResourceFlowType.Source:
                        Analytics.ItemObtained(itemId, 0, ItemFlowReason.Obsolete);
                        break;
                    case ResourceFlowType.Sink:
                        Analytics.ItemConsumed(itemId, 0, ItemFlowReason.Obsolete);
                        break;
                    default:
                        // NO-OP
                        break;
                }   
            }
        }

        [Obsolete("Use the new HomaGames.HomaBelly.Analytics.Level* or HomaGames.HomaBelly.Analytics.Checkpoint methods instead")]
        public void TrackProgressionEvent(ProgressionStatus progressionStatus, string progression01, int score = 0)
        {
            HomaGamesLog.Warning($"[Analytics] Obsolete TrackProgressionEvent method. Please use the new HomaGames.HomaBelly.Analytics.Level* or HomaGames.HomaBelly.Analytics.Checkpoint methods instead");

            if (int.TryParse(progression01, out int level))
            {
                switch (progressionStatus)
                {
                    case ProgressionStatus.Undefined:
                        Analytics.DesignEvent("progression_event", new DesignDimensions(key1:progressionStatus.ToString(), key2:progression01, score:score));
                        break;
                    case ProgressionStatus.Start:
                        Analytics.LevelStarted(level);
                        break;
                    case ProgressionStatus.Complete:
                        Analytics.LevelCompleted();
                        break;
                    case ProgressionStatus.Fail:
                        Analytics.LevelFailed("progression_status_fail");
                        break;
                }
            }
            else
            {
                Analytics.DesignEvent("progression_event", new DesignDimensions(key1:progressionStatus.ToString(), key2:progression01, score:score));
            }
        }

        [Obsolete("Use the new HomaGames.HomaBelly.Analytics.Level* or HomaGames.HomaBelly.Analytics.Checkpoint methods instead")]
        public void TrackProgressionEvent(ProgressionStatus progressionStatus, string progression01, string progression02, int score = 0)
        {
            HomaGamesLog.Warning($"[Analytics] Obsolete TrackProgressionEvent method. Please use the new HomaGames.HomaBelly.Analytics.Level* or HomaGames.HomaBelly.Analytics.Checkpoint methods instead");
            Analytics.DesignEvent("progression_event", new DesignDimensions(key1:progressionStatus.ToString(), key2:progression01, key3:progression02, score:score));
        }

        [Obsolete("Use the new HomaGames.HomaBelly.Analytics.Level* or HomaGames.HomaBelly.Analytics.Checkpoint methods instead")]
        public void TrackProgressionEvent(ProgressionStatus progressionStatus, string progression01, string progression02, string progression03, int score = 0)
        {
            HomaGamesLog.Warning($"[Analytics] Obsolete TrackProgressionEvent method. Please use the new HomaGames.HomaBelly.Analytics.Level* or HomaGames.HomaBelly.Analytics.Checkpoint methods instead");
            Analytics.DesignEvent("progression_event", new DesignDimensions(key1:progressionStatus.ToString(), key2:progression01, key3:progression02, key4:progression03, score:score));
        }

        public void TrackErrorEvent(ErrorSeverity severity, string message)
        {
            homaBridge.TrackErrorEvent(severity, message);
        }

        [Obsolete("Use HomaGames.HomaBelly.Analytics.DesignEvent instead")]
        public void TrackDesignEvent(string eventName, float eventValue = 0)
        {
            HomaGamesLog.Warning($"[Analytics] Obsolete TrackDesignEvent method. Please use the new HomaGames.HomaBelly.Analytics.DesignEvent instead");
            Analytics.DesignEvent(eventName, eventValue != 0 ? new DesignDimensions(score:eventValue) : null);
        }

        [Obsolete("If this is a suggested ad, use HomaGames.HomaBelly.Analytics.SuggestedRewardedAd instead. Other ad events are automated.")]
        public void TrackAdEvent(AdAction adAction, AdType adType, string adNetwork, string adPlacementId)
        {
            HomaGamesLog.Warning($"[Analytics] Obsolete TrackAdEvent method. Please use the new HomaGames.HomaBelly.Analytics.SuggestedRewardedAd or remove this call.");
            Analytics.DesignEvent("TrackAdEvent", new DesignDimensions(adAction.ToString(), adType.ToString(), adNetwork, adPlacementId));
        }

        public void TrackAdRevenue(AdRevenueData adRevenueData)
        {
            AnalyticsEventTracker.TrackAdRevenue(adRevenueData);
        }

        /// <summary>
        /// Tracks an event on the attribution platform
        /// </summary>
        /// <param name="eventName">The event name</param>
        /// <param name="arguments">(Optional) Additional arguments. Dictionary values must have one of these types: string, int, long, float, double, null, List, Dictionary&lt;String,object&gt;</param>
        public void TrackAttributionEvent(string eventName, Dictionary<string, object> arguments = null)
        {
            homaBridge.TrackAttributionEvent(eventName, arguments);
        }

        /// <summary>
        /// Tracks an event on the attribution platform with partner parameters
        /// </summary>
        /// <param name="eventName">The event name</param>
        /// <param name="partnerParameters">Partner parameters. Dictionary values must have one of these types: string, int, long, float, double, null, ArrayList, Dictionary<String,object></param>
        /// <param name="arguments">(Optional) Additional arguments. Dictionary values must have one of these types: string, int, long, float, double, null, ArrayList, Dictionary<String,object></param>
        public void TrackAttributionEventWithPartnerParameters(string eventName,
            Dictionary<string, object> partnerParameters,
            Dictionary<string, object> arguments = null)
        {
            homaBridge.TrackAttributionEventWithPartnerParameters(eventName, partnerParameters, arguments);
        }

        public void SetCustomDimension01(string customDimension)
        {
            homaBridge.SetCustomDimension01(customDimension);
        }

        public void SetCustomDimension02(string customDimension)
        {
            homaBridge.SetCustomDimension02(customDimension);
        }

        public void SetCustomDimension03(string customDimension)
        {
            homaBridge.SetCustomDimension03(customDimension);
        }

        #endregion

        private void AfterInitialization(Action action)
        {
            
            if (IsInitialized)
            {
                action();
            }
            else
            {
                Events.onInitialized += action;
            }
        }
    }
}
