using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.Usercentrics;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HomaGames.HomaBelly.DataPrivacy
{
    internal static class UserCentricsApiWrapper
    {
        public static event Action<AttAuthorizationStatus> AttAuthorizationStatusChanged;
        private const string AdsTrackingTransparencyConsentStatus = "homagames.idfa.ios_ads_tracking_consent_status";

        private const int FullInitializationTimeout = 10;
        private const int TcfDataFetchTimeout = 3;

        private static AttAuthorizationStatus? _authorizationStatus;
        public static AttAuthorizationStatus AttAuthorizationStatus
        {
            get
            {
                _authorizationStatus ??= GetAttStatusFromCache(AttAuthorizationStatus.NotDetermined);
                return _authorizationStatus.Value;
            }
            private set
            {
                _authorizationStatus = value;
                CacheAttStatus(value);
                AttAuthorizationStatusChanged?.Invoke(value);
            }
        }

        private static List<UsercentricsServiceConsent> _latestServiceList;

        private static bool _shouldOverrideConsent;
        public static bool Initialized { get; private set; }
        public static bool InitializationFailed { get; private set; }
        public static bool ShouldCollectConsent { get; private set; } = true;
        public static string TcString { get; private set; }
        internal static CmpData CmpData { get; private set; }

        public static bool IsApplicationBlacklisted
        {
            get
            {
                var platform = Application.platform == RuntimePlatform.IPhonePlayer
                    ? PublishedAppPlatform.Ios
                    : PublishedAppPlatform.Android;
                return CmpData != null &&
                       CmpData.publishedApps.Any(x => x.platform == platform && x.bundleId == Application.identifier);
            }
        }

        public static async Task InitializeAsync()
        {
            var initializationComplete = false;

            if (HomaBellyManifestConfiguration.TryGetString(out var geoRulesetId, "homabelly_core",
                    "s_geo_ruleset_id"))
                Usercentrics.Instance.RulesetID = geoRulesetId;

            Usercentrics.Instance.Initialize(status =>
            {
                ShouldCollectConsent = status.shouldCollectConsent;
                _latestServiceList = status.consents;

                InitializationFailed = false;
                Initialized = true;

                initializationComplete = true;
            }, errorString =>
            {
                Debug.LogError(errorString);

                InitializationFailed = true;
                Initialized = false;

                initializationComplete = true;
            });

            using (var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(FullInitializationTimeout)))
            {
                await TaskUtils.WaitUntil(() => initializationComplete, timeoutCts.Token)
                    // ReSharper disable once MethodSupportsCancellation
                    .ContinueWithOnMainThread(_ => Object.DontDestroyOnLoad(Usercentrics.Instance.gameObject));
            }

            FetchCmpData();
        }

        private static bool GetUserConsent()
        {
            if (_latestServiceList == null)
                return true;

            var nonEssentialServices = _latestServiceList
                .Where(service => !service.isEssential)
                .ToList();

            if (nonEssentialServices.Count == 0)
                return true;

            return nonEssentialServices.Any(service => service.status);
        }

        public static async Task<bool> ShowConsentFirstScreenAndAttAsync()
        {
            ThrowIfNotInitialized();

            var interactedWith = false;
            var firstLayerClosed = false;

            Usercentrics.Instance.ShowFirstLayer(UsercentricsLayout.PopupCenter, consentResponse =>
            {
                interactedWith = consentResponse.userInteraction != UsercentricsUserInteraction.NoInteraction;
                _latestServiceList = consentResponse.consents;

                firstLayerClosed = true;
            });

            await TaskUtils.WaitUntil(() => firstLayerClosed);

            if (interactedWith)
                await ShowAttIfNecessaryAsync();

            return interactedWith;
        }

        public static async Task ShowConsentMenuAndAttAsync()
        {
            ThrowIfNotInitialized();

            var consentMenuClosed = false;

            Usercentrics.Instance.ShowSecondLayer(true, consentResponse =>
            {
                _latestServiceList = consentResponse.consents;

                consentMenuClosed = true;
            });

            await TaskUtils.WaitUntil(() => consentMenuClosed);

            await ShowAttIfNecessaryAsync();
        }

        public static async Task ShowAttIfNecessaryAsync()
        {
            // Att popup should always be shown
            // Show Att if Usercentrics failed to initialize
            // or if the application is blacklisted
            // or if the user has already granted permission (went though the regular flow)
            if (!Initialized || IsApplicationBlacklisted || GetUserConsent())
                await ShowAttOnIosAsync();
        }

        private static async Task ShowAttOnIosAsync()
        {
            if (Application.platform != RuntimePlatform.IPhonePlayer)
                return;

            var attClosed = false;

            AppTrackingTransparency.Instance.PromptForAppTrackingTransparency(status =>
            {
                AttAuthorizationStatus = ToPublicApiEnum(status);
                attClosed = true;
            });

            await TaskUtils.WaitUntil(() => attClosed);

            PlayerPrefs.SetInt(Constants.PersistenceKey.IOS_ADS_TRACKING_ASKED, 1);
        }

        private static void CacheAttStatus(AttAuthorizationStatus authStatus)
        {
            PlayerPrefs.SetString(AdsTrackingTransparencyConsentStatus, authStatus.ToString());
        }

        private static AttAuthorizationStatus GetAttStatusFromCache(AttAuthorizationStatus fallbackValue)
        {
            var result = fallbackValue;
            var prefsStatus = PlayerPrefs.GetString(AdsTrackingTransparencyConsentStatus, null);

           if (Enum.TryParse<AttAuthorizationStatus>(prefsStatus, true, out var authStatus))
           {
                result = authStatus;
           }

            return result;
        }

        public static void OverrideConsent()
        {
            _shouldOverrideConsent = true;
        }

        public static async Task ApplyDataAsync()
        {
            ApplyConsent();

            if (Initialized)
            {
                var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(TcfDataFetchTimeout));
                await FetchTcfStringAsync(cancellationTokenSource.Token);
            }
        }

        private static void ApplyConsent()
        {
            var consent = InitializationFailed
                          || _shouldOverrideConsent
                          || GetUserConsent();

            PlayerPrefs.SetInt(Constants.PersistenceKey.ABOVE_REQUIRED_AGE, consent ? 1 : 0);
            PlayerPrefs.SetInt(Constants.PersistenceKey.TERMS_AND_CONDITIONS, consent ? 1 : 0);
            PlayerPrefs.SetInt(Constants.PersistenceKey.ANALYTICS_TRACKING, consent ? 1 : 0);
            PlayerPrefs.SetInt(Constants.PersistenceKey.TAILORED_ADS, consent ? 1 : 0);

            PlayerPrefs.Save();
        }

        private static async Task FetchTcfStringAsync(CancellationToken cancellationToken = default)
        {
            var stringFetched = false;

            try
            {
                Usercentrics.Instance.GetTCFData(tcfData =>
                {
                    TcString = tcfData.tcString;
                    stringFetched = true;
                });
            }
            catch (Exception e)
            {
                HomaGamesLog.Error($"Error while fetching TCF string: {e}");
                return;
            }

            await TaskUtils.WaitUntil(() => stringFetched, cancellationToken);
        }

        private static void FetchCmpData()
        {
            ThrowIfNotInitialized();

            string checkStatus = null;

            try
            {
                Analytics.CustomEvent("usercentrics_blacklist_api", null);
                CmpData = Usercentrics.Instance.GetCmpData();
            }
            catch (Exception e)
            {
                HomaGamesLog.Error($"Error while fetching CMP data: {e}");
                checkStatus = "failed";
            }

            checkStatus ??= IsApplicationBlacklisted ? "blacklisted" : "allowed";

            Analytics.CustomEvent("usercentrics_blacklist_check", new Dictionary<string, object>
            {
                { "status", checkStatus }
            });
        }

        private static void ThrowIfNotInitialized()
        {
            if (!Initialized)
                throw new InvalidOperationException(
                    "You must initialize UserCentrics before calling any other function.");
        }

        private static AttAuthorizationStatus ToPublicApiEnum(AuthorizationStatus authorizationStatus)
        {
            return authorizationStatus switch
            {
                AuthorizationStatus.AUTHORIZED => AttAuthorizationStatus.Authorized,
                AuthorizationStatus.DENIED => AttAuthorizationStatus.Denied,
                AuthorizationStatus.NOT_DETERMINED => AttAuthorizationStatus.NotDetermined,
                AuthorizationStatus.RESTRICTED => AttAuthorizationStatus.Restricted,
                _ => throw new ArgumentOutOfRangeException(nameof(authorizationStatus), authorizationStatus, null)
            };
        }
    }
}