using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HomaGames.HomaBelly.Internal;
using JetBrains.Annotations;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public abstract class MediatorBase
    {
        // Helpers
        private readonly NetworkHelper _networkHelper = new NetworkHelper();
        private readonly Events _events = new Events();
        private readonly InternalAdEvents _internalAdEvents = new InternalAdEvents();
        private AttributionAdViewEventManager _attributionAdViewEventManager;
        
        private const int MAX_RELOAD_RETRY_DELAY = 5;
        
        private static readonly BannerStyle DefaultBannerStyle =  
            new BannerStyle(BannerSize.BANNER, BannerPosition.BOTTOM, Color.white);

        public class AdState
        {
            public readonly AdInfo AdInfo;
            public BannerStyle BannerStyle;
            public bool IsBannerVisible;
            public bool IsAdLoaded;
            public AdState(AdPlacementType placementType, AdType type, string adPlacement)
            {
                AdInfo = new AdInfo(adPlacement,type,placementType);
            }

            private bool _loading = false;

            public bool Loading
            {
                get => _loading;
                set
                {
                    if (!value)
                        _retryCount = 0;
                    _loading = value;
                }
            }

            private int _retryCount = 0;
            public int LoadingRetryCount => _retryCount;
            /// <summary>
            /// Property used to obtain delay seconds between each retry attempt: 2, 4, 8, 16, 32, 32, 32...
            /// </summary>
            public int LoadingRetryDelayBase => Math.Min(_retryCount, MAX_RELOAD_RETRY_DELAY);
            public void Retry() => _retryCount++;
            public AdPlacementType PlacementType => AdInfo.AdPlacementType;
            public AdType Type => AdInfo.AdType;
            public string AdPlacement => AdInfo.PlacementId;
        }

        // Ad placement to ad state
        private readonly Dictionary<string, AdState> _adStates = new Dictionary<string, AdState>();

        internal AdState GetOrCreateAdState([NotNull] string placementId, AdType adType)
        {
            if (!_adStates.ContainsKey(placementId))
            {
                _adStates.Add(placementId, new AdState(GetAdPlacementTypeForPlacementId(placementId), adType, placementId));
            }

            return _adStates[placementId];
        }

        private bool _reportAdRevenue = false;


        private static readonly Dictionary<AdType, string> AdTypesToString = new Dictionary<AdType, string>
        {
            [AdType.Banner] = "banner",
            [AdType.Interstitial] = "interstitial",
            [AdType.RewardedVideo] = "rewarded_video",
        };

        private AdPlacementType GetAdPlacementTypeForPlacementId(string adPlacementId)
        {
            foreach (var adPlacementType in new[] {AdPlacementType.Default, AdPlacementType.HighValue})
            {
                foreach (var adType in new[] {AdType.Interstitial, AdType.RewardedVideo, AdType.Banner})
                {
                    if (TryGetDefaultAdId(adType, adPlacementType, out var currentPlacementId)
                        && currentPlacementId == adPlacementId)
                        return adPlacementType;
                }
            }

            return AdPlacementType.User;
        }

        internal bool TryGetDefaultAdId(AdType adType, AdPlacementType adPlacementType, out string adId)
        {
            adId = string.Empty;
            
            if (!AdTypesToString.ContainsKey(adType) || !_adPlacementConfigName.ContainsKey(adPlacementType))
                return false;
            
            string platform = GetPlatformString();
            
            string configPath = $"s_{platform}_{_adPlacementConfigName[adPlacementType]}_{AdTypesToString[adType]}_ad_unit_id";

            return HomaBellyManifestConfiguration.TryGetString(out adId, 
                       MediatorPackageName, configPath) 
                   && !string.IsNullOrEmpty(adId);
        }

        private string GetPlatformString()
        {            
#if UNITY_ANDROID
            return "android";
#elif UNITY_IOS
            return "ios";
#else
            return string.Empty;
#endif
        }

        // Base methods
        public void Initialize(Action onInitialized = null)
        {
            if (HomaBellyManifestConfiguration.TryGetBool(out var shouldReport, MediatorPackageName,
                    "b_report_ad_revenue"))
            {
                _reportAdRevenue = shouldReport;
            }
            
            _networkHelper.OnNetworkReachabilityChange += OnNetworkReachabilityChange;
            _networkHelper.StartListening();
            
            OnInitialised += () =>
            {
                // We do that *before* loading default ads, to not load them twice,
                // and only load the other ads the devs requested loading, like HVA 
                ReloadAllAdStatesInternally();
                
                // Preload interstitial and rewarded video ads to be cached
                LoadInterstitial(GetAdIdOrDefault(AdType.Interstitial));
                LoadRewardedVideoAd(GetAdIdOrDefault(AdType.RewardedVideo));
                LoadBanner(GetAdIdOrDefault(AdType.Banner), DefaultBannerStyle);
                
                onInitialized?.Invoke();
                HomaGamesLog.Debug($"[{MediatorPackageName} Mediator]  Initialized successfully");
            };
            OnAdRevenuePaidEvent += OnAdRevenuePaid;
            OnBannerLoadedEvent += s =>
            {
                var adState = GetOrCreateAdState(s, AdType.Banner);
                adState.Loading = false;
                adState.IsAdLoaded = true;
                
                HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] OnBannerAdLoadedEvent {adState.AdInfo}");
                _events.OnBannerAdLoadedEvent(adState.AdInfo);
            };
            BannerAdClickedEvent += s =>
            {
                var adState = GetOrCreateAdState(s, AdType.Banner);
                HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] BannerAdClickedEvent {adState.AdInfo}");
                _events.OnBannerAdClickedEvent(adState.AdInfo);
            };
            BannerAdLoadFailedEvent += (s, errorCode, errorMessage) =>
            {
                var adState = GetOrCreateAdState(s, AdType.Banner);
                HomaGamesLog.Debug(
                    $"[{MediatorPackageName} Mediator] BannerAdLoadFailedEvent with error code {errorCode}: {errorMessage}.\n {adState.AdInfo}");
                _events.OnBannerAdLoadFailedEvent(adState.AdInfo);
                RetryLoadAd(adState).ListenForErrors();
            };
            OnRewardedAdReceivedRewardEvent += (s, reward) =>
            {
                var adState = GetOrCreateAdState(s, AdType.RewardedVideo);
                HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] OnRewardedAdReceivedRewardEvent {adState.AdInfo}");
                _events.OnRewardedVideoAdRewardedEvent(reward,adState.AdInfo);
            };
            OnRewardedAdDismissedEvent += s =>
            {
                LoadRewardedVideoAd(s);
                var adState = GetOrCreateAdState(s, AdType.RewardedVideo);
                adState.IsAdLoaded = false;
                HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] OnRewardedAdDismissedEvent {adState.AdInfo}");
                _events.OnRewardedVideoAdClosedEvent(adState.AdInfo);
            };
            OnRewardedAdClickedEvent += s =>
            {
                var adState = GetOrCreateAdState(s, AdType.RewardedVideo);
                HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] OnRewardedAdClickedEvent {adState.AdInfo}");
                _events.OnRewardedVideoAdClickedEvent(adState.AdInfo);
            };
            OnRewardedAdDisplayedEvent += s =>
            {
                var adState = GetOrCreateAdState(s, AdType.RewardedVideo);
                adState.Loading = false;
                HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] OnRewardedAdDisplayedEvent {adState.AdInfo}");
                _events.OnRewardedVideoAdStartedEvent(adState.AdInfo);
            };
            OnRewardedAdFailedToDisplayEvent += (s, errorCode, error) =>
            {
                var adState = GetOrCreateAdState(s, AdType.RewardedVideo);
                adState.Loading = false;

                HomaGamesLog.Debug(
                    $"[{MediatorPackageName} Mediator] OnRewardedAdFailedToDisplayEvent with error code {errorCode}: {error}.\n {adState.AdInfo}");
                RetryLoadAd(GetOrCreateAdState(s, AdType.RewardedVideo)).ListenForErrors();
                _events.OnRewardedVideoAdShowFailedEvent(adState.AdInfo);
            };
            OnRewardedAdFailedEvent += (s, errorCode, error) =>
            {
                var adState = GetOrCreateAdState(s, AdType.RewardedVideo);
                HomaGamesLog.Debug(
                    $"[{MediatorPackageName} Mediator] OnRewardedAdFailedEvent with error code {errorCode}: {error}.\n {adState.AdInfo}");
                RetryLoadAd(adState).ListenForErrors();
                _events.OnRewardedVideoAvailabilityChangedEvent(false, adState.AdInfo);
            };
            OnRewardedAdLoadedEvent += s =>
            {
                var adState = GetOrCreateAdState(s, AdType.RewardedVideo);
                adState.Loading = false;
                adState.IsAdLoaded = true;

                HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] OnRewardedAdLoadedEvent {adState.AdInfo}");
                _events.OnRewardedVideoAvailabilityChangedEvent(true, adState.AdInfo);
            };
            OnInterstitialDismissedEvent += s =>
            {
                var adState = GetOrCreateAdState(s, AdType.Interstitial);
                adState.Loading = false;
                adState.IsAdLoaded = false;

                HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] OnInterstitialDismissedEvent {adState.AdInfo}");
                LoadInterstitial(s);

                _events.OnInterstitialAdClosedEvent(adState.AdInfo);
            };
            InterstitialFailedToDisplayEvent += (s, errorCode, error) =>
            {
                var adState = GetOrCreateAdState(s, AdType.Interstitial);
                HomaGamesLog.Debug(
                    $"[{MediatorPackageName} Mediator] InterstitialAdShowFailedEvent with error code {errorCode}: {error}.\n {adState.AdInfo}");
                _events.OnInterstitialAdShowFailedEvent(adState.AdInfo);
                RetryLoadAd(adState).ListenForErrors();
            };
            OnInterstitialFailedEvent += (s, errorCode, error) =>
            {
                var adState = GetOrCreateAdState(s, AdType.Interstitial);
                HomaGamesLog.Debug(
                    $"[{MediatorPackageName} Mediator] OnInterstitialFailedEvent with error code {errorCode}: {error}.\n {adState.AdInfo}");
                _events.OnInterstitialAdLoadFailedEvent(adState.AdInfo);
                RetryLoadAd(adState).ListenForErrors();
            };
            OnInterstitialLoadedEvent += s =>
            {
                var adState = GetOrCreateAdState(s, AdType.Interstitial);
                adState.Loading = false;
                adState.IsAdLoaded = true;

                HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] OnInterstitialLoadedEvent {adState.AdInfo}");
                _events.OnInterstitialAdReadyEvent(adState.AdInfo);
            };
            OnInterstitialClickedEvent += s =>
            {
                var adState = GetOrCreateAdState(s, AdType.Interstitial);
                HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] InterstitialAdClickedEvent {adState.AdInfo}");
                _events.OnInterstitialAdClickedEvent(adState.AdInfo);
            };
            OnInterstitialShownEvent += s =>
            {
                var adState = GetOrCreateAdState(s, AdType.Interstitial);
                adState.Loading = false;
                HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] InterstitialAdShowSucceededEvent {adState.AdInfo}");
                _events.OnInterstitialAdShowSucceededEvent(adState.AdInfo);
            };
            InternalInitialize();
        }

        public virtual void OnApplicationPause(bool pause)
        {
        }

        public virtual void ValidateIntegration()
        {
        }

        #region GDPR/CCPA

        /// <summary>
        /// Specifies if the user asserted being above the required age
        /// </summary>
        /// <param name="consent">true if user accepted, false otherwise</param>
        public virtual void SetUserIsAboveRequiredAge(bool consent)
        {
        }

        /// <summary>
        /// Specifies if the user accepted privacy policy and terms and conditions
        /// </summary>
        /// <param name="consent">true if user accepted, false otherwise</param>
        public virtual void SetTermsAndConditionsAcceptance(bool consent)
        {
        }

        /// <summary>
        /// Specifies if the user granted consent for analytics tracking
        /// </summary>
        /// <param name="consent">true if user accepted, false otherwise</param>
        public virtual void SetAnalyticsTrackingConsentGranted(bool consent)
        {
        }

        /// <summary>
        /// Specifies if the user granted consent for showing tailored ads
        /// </summary>
        /// <param name="consent">true if user accepted, false otherwise</param>
        public virtual void SetTailoredAdsConsentGranted(bool consent)
        {
        }

        #endregion

        /// <summary>
        /// Try reloading the ad after one or multiple failed attempts
        /// </summary>
        /// <param name="state">The ad to reload</param>
        private async Task RetryLoadAd(AdState state)
        {
            if (!state.Loading)
                return;

            state.Retry();
            int retryDelayInMs = (int) Math.Pow(2, state.LoadingRetryDelayBase) * 1000;
            HomaGamesLog.Debug(
                $"[{MediatorPackageName} Mediator] Trying to reload ad {state.AdPlacement} in {retryDelayInMs}. Retry count : {state.LoadingRetryCount}");
            await Task.Delay(retryDelayInMs);
            switch (state.Type)
            {
                case AdType.Banner:
                    LoadBanner(state.AdPlacement, state.BannerStyle);
                    break;
                case AdType.Interstitial:
                    LoadInterstitial(state.AdPlacement);
                    break;
                case AdType.RewardedVideo:
                    LoadRewardedVideoAd(state.AdPlacement);
                    break;
            }
        }

        // Rewarded Video Ads
        public void LoadRewardedVideoAd([NotNull] string placement)
        {
            GetOrCreateAdState(placement, AdType.RewardedVideo).Loading = true;
            InternalLoadRewardedVideoAd(placement);
        }
        
        public void LoadHighValueRewardedVideoAd()
        {
            if (TryGetDefaultAdId(AdType.RewardedVideo, AdPlacementType.HighValue ,out var adPlacement))
            {
                LoadRewardedVideoAd(adPlacement);
            }
            else
            {
                HomaGamesLog.Error($"[{MediatorPackageName} Mediator] No High Value rewarded video ad in configuration.");
            }
        }

        public void ShowRewardedVideoAd(string placement = null)
        {
            string finalPlacement = GetAdIdOrDefault(AdType.RewardedVideo, placement);
            if (!InternalIsInitialized)
            {
                HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] Not initialised");
                return;
            }

            // If rewarded video ad is ready, show it
            if (InternalIsRewardedVideoAdAvailable(finalPlacement))
            {
                HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] Video Ad available. Showing...");
                InternalShowRewardedVideoAd(finalPlacement);
            }
            else
            {
                LoadRewardedVideoAd(finalPlacement);
                HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] Rewarded video not available");
                _events.OnRewardedVideoAdShowFailedEvent(GetOrCreateAdState(finalPlacement, AdType.RewardedVideo)
                    .AdInfo);
            }
        }

        public void ShowHighValueRewardedVideoAd()
        {
            if(TryGetDefaultAdId(AdType.RewardedVideo, AdPlacementType.HighValue, out var adPlacement))
            {
                ShowRewardedVideoAd(adPlacement);
            }
            else
            {
                HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] No High Value ad configured for rewarded video ads.");
            }
        }

        public bool IsRewardedVideoAdAvailable(string placement = null)
        {
            if (!InternalIsInitialized) 
                return false;
            
            return InternalIsRewardedVideoAdAvailable(GetAdIdOrDefault(AdType.RewardedVideo, placement));
        }
        
        public bool IsHighValueRewardedVideoAdAvailable()
        {
            if(TryGetDefaultAdId(AdType.RewardedVideo, AdPlacementType.HighValue, out string highValuePlacement))
                return IsRewardedVideoAdAvailable(highValuePlacement);
            
            return false;
        }

        // Banners
        private void LoadBanner(string placementId, BannerStyle style) =>
            LoadBanner(style.Size, style.Position, placementId, style.BackgroundColor);
        public void LoadBanner(BannerSize size, BannerPosition position, string placementId = null,
            Color bannerBackgroundColor = default)
        {
            if (bannerBackgroundColor == default)
                bannerBackgroundColor = DefaultBannerStyle.BackgroundColor;

            placementId = GetAdIdOrDefault(AdType.Banner, placementId);
            
            var adState = GetOrCreateAdState(placementId, AdType.Banner);
            adState.BannerStyle = new BannerStyle(size, position, bannerBackgroundColor);

            // We will update the banner after initialisation
            if (!InternalIsInitialized) 
                return;
            
            // Destroy this banner in case it's already displayed
            DestroyBanner(placementId);

            GetOrCreateAdState(placementId, AdType.Banner).Loading = true;
            HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] Loading banner");
            InternalLoadBanner(size, position, placementId, bannerBackgroundColor);
        }

        public void ShowBanner(string nullablePlacement = null)
        {
            var placementId = GetAdIdOrDefault(AdType.Banner, nullablePlacement);
            var adState = GetOrCreateAdState(placementId, AdType.Banner);
            if (!adState.IsAdLoaded) return;
            InternalShowBanner(placementId);
            adState.IsBannerVisible = true;
            InvokeBannerShownEvent(placementId);
        }

        public void HideBanner(string nullablePlacement = null)
        {
            var placementId = GetAdIdOrDefault(AdType.Banner, nullablePlacement);
            var adState = GetOrCreateAdState(placementId, AdType.Banner);
            if (!adState.IsBannerVisible) return;
            InternalHideBanner(placementId);
            adState.IsBannerVisible = false;
            InvokeBannerHiddenEvent(placementId);
        }

        public void DestroyBanner(string nullablePlacement = null)
        {
            var placementId = GetAdIdOrDefault(AdType.Banner, nullablePlacement);
            var adState = GetOrCreateAdState(placementId, AdType.Banner);
            if (!adState.IsBannerVisible) return;
            InternalDestroyBanner(placementId);
            adState.IsBannerVisible = false;
            InvokeBannerDestroyedEvent(placementId);
        }

        private void InvokeBannerShownEvent([NotNull] string placementId)
        {
            var adState = GetOrCreateAdState(placementId, AdType.Banner);
            HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] OnBannerShownEvent {adState.AdInfo}");
            _internalAdEvents.OnBannerAdShownEvent(adState.AdInfo);
        }

        private void InvokeBannerHiddenEvent([NotNull] string placementId)
        {
            var adState = GetOrCreateAdState(placementId, AdType.Banner);
            HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] OnBannerHiddenEvent {adState.AdInfo}");
            _internalAdEvents.OnBannerAdHiddenEvent(adState.AdInfo);
        }

        private void InvokeBannerDestroyedEvent([NotNull] string placementId)
        {
            var adState = GetOrCreateAdState(placementId, AdType.Banner);
            HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] OnBannerDestroyedEvent {adState.AdInfo}");
            _internalAdEvents.OnBannerAdDestroyedEvent(adState.AdInfo);
            adState.IsAdLoaded = false;
        }

        public int GetBannerHeight(string placement = null)
        {
            return InternalGetBannerHeight(GetAdIdOrDefault(AdType.Banner, placement));
        }

        public void SetBannerPosition(BannerPosition position,string nullablePlacementId = null)
        {
            var placementId = GetAdIdOrDefault(AdType.Banner, nullablePlacementId);
            InternalSetBannerPosition(placementId, position);
            GetOrCreateAdState(placementId, AdType.Banner).BannerStyle.Position = position;
        }
        
        public void SetBannerBackgroundColor(Color color,string nullablePlacementId = null)
        {
            var placementId = GetAdIdOrDefault(AdType.Banner, nullablePlacementId);
            InternalSetBannerBackgroundColor(placementId, color);
            GetOrCreateAdState(placementId, AdType.Banner).BannerStyle.BackgroundColor = color;
        }
        
        public BannerData[] GetVisibleBanners()
        {
            return _adStates.Values.Where(ad => ad.Type == AdType.Banner && ad.IsBannerVisible)
                .Select(state => new BannerData
                {
                    AdInfo = state.AdInfo,
                    Style = state.BannerStyle
                }).ToArray();
        }

        // Interstitial
        public void LoadInterstitial([NotNull] string placement)
        {
            GetOrCreateAdState(placement, AdType.Interstitial).Loading = true;
    
            if (InternalIsInitialized)
                InternalLoadInterstitial(placement);
        }
        
        public void LoadHighValueInterstitial()
        {
            if (TryGetDefaultAdId(AdType.Interstitial, AdPlacementType.HighValue, out var adPlacement))
            {
                LoadInterstitial(adPlacement);
            }
            else
            {
                HomaGamesLog.Error($"[{MediatorPackageName} Mediator] No High Value interstitial ad in configuration.");
            }
        }

        public void ShowInterstitial(string placement = null)
        {
            var finalPlacement = GetAdIdOrDefault(AdType.Interstitial, placement);
            if (!InternalIsInitialized)
            {
                HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] Not initialised");
                return;
            }

            HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] Request show interstitial");
            if (InternalIsInterstitialAvailable(finalPlacement))
            {
                HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] Interstitial available");
                InternalShowInterstitial(finalPlacement);
            }
            else
            {
                LoadInterstitial(finalPlacement);
                HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] Interstitial not available");
                _events.OnInterstitialAdShowFailedEvent(GetOrCreateAdState(finalPlacement, AdType.Interstitial)
                    .AdInfo);
            }
        }
        
        public void ShowHighValueInterstitial()
        {
            if(TryGetDefaultAdId(AdType.Interstitial, AdPlacementType.HighValue, out var adPlacement))
            {
                ShowInterstitial(adPlacement);
            }
            else
            {
                HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] No High Value ad configured for interstitial ads.");
            }
        }

        public bool IsInterstitialAvailable(string placement = null)
        {
            if (!InternalIsInitialized) 
                return false;
            
            return InternalIsInterstitialAvailable(GetAdIdOrDefault(AdType.Interstitial, placement));
        }
        
        public bool IsHighValueInterstitialAvailable()
        {
            if(TryGetDefaultAdId(AdType.Interstitial, AdPlacementType.HighValue, out string highValuePlacement))
                return IsInterstitialAvailable(highValuePlacement);
            
            return false;
        }

        private string GetAdIdOrDefault(AdType adType, string placement = "")
        {
            if (string.IsNullOrEmpty(placement) && TryGetDefaultAdId(adType, AdPlacementType.Default, out string defaultPlacement))
                return defaultPlacement;
            return placement;
        }

        private void OnNetworkReachabilityChange(NetworkReachability reachability)
        {
            NetworkReachabilityChanged(reachability);
        }

        private void NetworkReachabilityChanged(NetworkReachability reachability)
        {
            if (reachability != NetworkReachability.NotReachable)
            {
                HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] Internet reachable. Reloading ads if necessary");
                ReloadAllAdStatesInternally();
            }
        }

        private void ReloadAllAdStatesInternally()
        {
            foreach (var adState in _adStates.Values)
            {
                if (adState.Loading)
                {
                    switch (adState.Type)
                    {
                        case AdType.Interstitial:
                            InternalLoadInterstitial(adState.AdPlacement);
                            break;
                        case AdType.RewardedVideo:
                            InternalLoadRewardedVideoAd(adState.AdPlacement);
                            break;
                        case AdType.Banner:
                            var style = adState.BannerStyle;
                            InternalLoadBanner(style.Size, style.Position, adState.AdPlacement, style.BackgroundColor);
                            break;
                        case AdType.Undefined:
                        case AdType.Video:
                        case AdType.Playable:
                        case AdType.OfferWall:
                            break;
                    }
                }
            }
        }

        private static readonly Dictionary<AdType, string> _adTypeConfigName = new Dictionary<AdType, string>()
        {
            {AdType.Banner,"banner_ad_unit_id"},
            {AdType.Interstitial,"interstitial_ad_unit_id"},
            {AdType.RewardedVideo,"rewarded_video_ad_unit_id"}
        };
        
        private static readonly Dictionary<AdPlacementType, string> _adPlacementConfigName = new Dictionary<AdPlacementType, string>()
        {
            {AdPlacementType.Default,"default"},
            {AdPlacementType.HighValue,"high_value"}
        };

        /// <summary>
        /// Callback invoked for ULRD
        /// </summary>
        /// <param name="adPlacement"></param>
        /// <param name="adType"></param>
        /// <param name="data"></param>
        private void OnAdRevenuePaid(string adPlacement, AdType adType, AdRevenueData data)
        {
            if (!_reportAdRevenue)
            {
                return;
            }

            AdTypesToString.TryGetValue(adType, out data.AdType);
            AnalyticsEventTracker.TrackAdRevenue(data);
            // Track AdView event for attribution platforms. We run it on Main Thread as there is Player Prefs
            // access to persist cadence and revenue if needed. Player Prefs can only be called in main thread
            _attributionAdViewEventManager ??= new AttributionAdViewEventManager();
            ThreadUtils.RunOnMainThreadCurrentOrForget(() => _attributionAdViewEventManager.OnAdRevenue(data));
        }

        #region Protected Interface

        protected abstract string MediatorPackageName { get; }

        // Ads
        protected abstract void InternalInitialize();
        protected abstract bool InternalIsInitialized { get; }

        protected abstract void InternalLoadBanner(BannerSize size, BannerPosition position,
            [NotNull] string placementId,
            Color backgroundColor);

        protected abstract void InternalShowBanner([NotNull] string placement);
        protected abstract void InternalHideBanner([NotNull] string placement);
        protected abstract void InternalDestroyBanner([NotNull] string placement);
        protected abstract int InternalGetBannerHeight([NotNull] string placement);
        protected abstract void InternalSetBannerPosition([NotNull] string placement,BannerPosition bannerPosition);
        protected abstract void InternalSetBannerBackgroundColor([NotNull] string placement,Color color);
        protected abstract void InternalLoadInterstitial([NotNull] string placement);
        protected abstract void InternalShowInterstitial([NotNull] string placement);
        protected abstract bool InternalIsInterstitialAvailable([NotNull] string placement);
        protected abstract void InternalLoadRewardedVideoAd([NotNull] string placement);
        protected abstract void InternalShowRewardedVideoAd([NotNull] string placement);

        protected abstract bool InternalIsRewardedVideoAdAvailable([NotNull] string placement);

        // Events
        public event Action OnInitialised;
        protected void InvokeOnInitialised() => OnInitialised?.Invoke();
        public event Action<string, AdType, AdRevenueData> OnAdRevenuePaidEvent;

        protected void InvokeOnAdRevenuePaidEvent(string placementId, AdType adType, AdRevenueData data) =>
            OnAdRevenuePaidEvent?.Invoke(placementId, adType, data);
        public event Action<string> OnBannerLoadedEvent;
        protected void InvokeOnBannerLoadedEvent(string placementId) => OnBannerLoadedEvent?.Invoke(placementId);
        public event Action<string> BannerAdClickedEvent;

        protected void InvokeBannerAdClickedEvent(string placementId) => BannerAdClickedEvent?.Invoke(placementId);

        // Ad placement, error code, error message
        public event Action<string, int, string> BannerAdLoadFailedEvent;

        protected void InvokeBannerAdLoadFailedEvent(string placementId, int errorCode, string error) =>
            BannerAdLoadFailedEvent?.Invoke(placementId, errorCode, error);

        public event Action<string> OnRewardedAdLoadedEvent;

        protected void InvokeOnRewardedAdLoadedEvent(string placementId) =>
            OnRewardedAdLoadedEvent?.Invoke(placementId);

        // Ad placement, error code, error message
        public event Action<string, int, string> OnRewardedAdFailedEvent;

        protected void InvokeOnRewardedAdFailedEvent(string placementId, int errorCode, string error) =>
            OnRewardedAdFailedEvent?.Invoke(placementId, errorCode, error);

        // Ad placement, error code, error message
        public event Action<string, int, string> OnRewardedAdFailedToDisplayEvent;

        protected void InvokeOnRewardedAdFailedToDisplayEvent(string placementId, int errorCode, string error) =>
            OnRewardedAdFailedToDisplayEvent?.Invoke(placementId, errorCode, error);

        public event Action<string> OnRewardedAdDisplayedEvent;

        protected void InvokeOnRewardedAdDisplayedEvent(string placementId) =>
            OnRewardedAdDisplayedEvent?.Invoke(placementId);

        public event Action<string> OnRewardedAdClickedEvent;

        protected void InvokeOnRewardedAdClickedEvent(string placementId) =>
            OnRewardedAdClickedEvent?.Invoke(placementId);

        public event Action<string> OnRewardedAdDismissedEvent;

        protected void InvokeOnRewardedAdDismissedEvent(string placementId) =>
            OnRewardedAdDismissedEvent?.Invoke(placementId);

        public event Action<string, VideoAdReward> OnRewardedAdReceivedRewardEvent;

        protected void InvokeOnRewardedAdReceivedRewardEvent(string placementId, VideoAdReward reward) =>
            OnRewardedAdReceivedRewardEvent?.Invoke(placementId, reward);

        public event Action<string> OnInterstitialClickedEvent;

        protected void InvokeOnInterstitialClickedEvent(string placementId) =>
            OnInterstitialClickedEvent?.Invoke(placementId);

        public event Action<string> OnInterstitialShownEvent;

        protected void InvokeOnInterstitialShownEvent(string placementId) =>
            OnInterstitialShownEvent?.Invoke(placementId);

        public event Action<string> OnInterstitialLoadedEvent;

        protected void InvokeOnInterstitialLoadedEvent(string placementId) =>
            OnInterstitialLoadedEvent?.Invoke(placementId);

        // Ad placement, error code, error message
        public event Action<string, int, string> OnInterstitialFailedEvent;

        protected void InvokeOnInterstitialFailedEvent(string placementId, int errorCode, string error) =>
            OnInterstitialFailedEvent?.Invoke(placementId, errorCode, error);

        // Ad placement, error code, error message
        public event Action<string, int, string> InterstitialFailedToDisplayEvent;

        protected void InvokeInterstitialFailedToDisplayEvent(string placementId, int errorCode, string error) =>
            InterstitialFailedToDisplayEvent?.Invoke(placementId, errorCode, error);

        public event Action<string> OnInterstitialDismissedEvent;

        protected void InvokeOnInterstitialDismissedEvent(string placementId) =>
            OnInterstitialDismissedEvent?.Invoke(placementId);

        #endregion

        public string DumpAdStates()
        {
            var s = $"AdStates for {MediatorPackageName} Mediator\n\n";
            foreach (var state in _adStates.Values)
            {
                s +=
                    $"Ad placement:{state.AdPlacement} Type:{state.Type} Loading:{state.Loading} Load retries {state.LoadingRetryCount} Placement Type {state.PlacementType}\n\n";
            }

            return s;
        }
    }
}
