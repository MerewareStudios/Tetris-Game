using System;
using System.Collections.Generic;
using System.Globalization;
using HomaGames.Geryon;
using HomaGames.HomaBelly.Internal.Analytics;
using HomaGames.HomaBelly.Utilities;
using UnityEngine;

#pragma warning disable CS0162

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Adapter for IAnalytics interface
    /// </summary>
    public class HomaAnalyticsAdapter : AnalyticsBase
    {
        private const string ANALYTICS_CONSENT_GRANTED_KEY = "analytics_consent";
        private const string END_POINT_FORMAT = "{0}/appevent";
        private const float SECONDS_FOR_NEW_SESSION = 600f;
        
        private static HomaAnalytics m_homaAnalytics = null;
         

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AnalyticsPreInitialization()
        {
            // Homa analytics supports to be pre initialized.
            // The main purpose is to not miss any event since the application starts.
            // Homa Analytics will store all events prior to the initialization and it will send them later.
            // Missing information will be filled just before sending the event (like Manifest ID, or Token ID)

            #if DISABLE_ANALYTICS_AUTO_INITIALIZATION
            // Needed for dev environments 
            return;
            #endif
            
            var temporalHomaAnalyticsOptions = GenerateHomaAnalyticsOptions();
            m_homaAnalytics = new HomaAnalytics(temporalHomaAnalyticsOptions);
        }

        public override void Initialize(Action onInitialized = null)
        {
            #if DISABLE_ANALYTICS_AUTO_INITIALIZATION
            // Needed for dev environments
            return;
            #endif
            
            Config.OnInitialized += delegate
            {
                // Override options with token identifier and manifest version id
                var finalHomaAnalyticsOptions = GenerateHomaAnalyticsOptions();
                m_homaAnalytics.Initialize(finalHomaAnalyticsOptions);

                if (PlayerPrefs.HasKey(ANALYTICS_CONSENT_GRANTED_KEY))
                {
                    bool analyticsConsentGranted = PlayerPrefs.GetInt(ANALYTICS_CONSENT_GRANTED_KEY) == 1;
                    m_homaAnalytics.ToggleAnalytics(analyticsConsentGranted);
                }

                onInitialized?.Invoke();
            };
        }
        
        private static HomaAnalyticsOptions GenerateHomaAnalyticsOptions()
        {
            var validateEvents = false;
            bool useCsvTool = false;
            #if HOMA_DEVELOPMENT
            validateEvents = true;
            useCsvTool = true;
            #endif
            
            var url = HomaBellyConstants.API_HOST;
            var endPoint = string.Format(END_POINT_FORMAT, url);
            
            // Gather token and manifest version id
            HomaBellyManifestConfiguration.TryGetString(out var tokenIdentifier,
                HomaBellyManifestConfiguration.MANIFEST_TOKEN_KEY);
            HomaBellyManifestConfiguration.TryGetString(out var manifestVersionId,
                HomaBellyManifestConfiguration.MANIFEST_VERSION_ID_KEY);

            if (string.IsNullOrEmpty(tokenIdentifier))
            {
                HomaAnalyticsLogger.LogError(
                    "tokenIdentifier can't be null. Ignore this error if you are testing without manifest token in a test project.");
                tokenIdentifier = "NullManifestId";
            }

            if (string.IsNullOrEmpty(manifestVersionId))
            {
                HomaAnalyticsLogger.LogError(
                    "manifestVersionId can't be null. Ignore this error if you are testing without manifest token in a test project.");
                manifestVersionId = "NullTokenId";
            }

            var nTestingId = Config.Initialized ? Config.NTESTING_ID : "NTestingIdNotSet";
            var nTestingOverrideId = Config.Initialized ? Config.OverrideId : "NTestingOverrideIdNotSet";

            // We create a temporal options object so we can start storing 
            // event prior to HomaAnalytics initialization
            
            var temporalHomaAnalyticsOptions = new HomaAnalyticsOptions(false,
                validateEvents,
                endPoint,
                SECONDS_FOR_NEW_SESSION,
                tokenIdentifier,
                manifestVersionId,
                true,
                useCsvTool,
                nTestingId,
                nTestingOverrideId);
            
            return temporalHomaAnalyticsOptions;
        }

        public override void SetAnalyticsTrackingConsentGranted(bool consent)
        {
            m_homaAnalytics.ToggleAnalytics(consent);
            
            PlayerPrefs.SetInt(ANALYTICS_CONSENT_GRANTED_KEY,consent ? 1 : 0);
            PlayerPrefs.Save();
        }

        public override void TrackErrorEvent(ErrorSeverity severity, string message)
        {
            var errorEvent = new ErrorAnalyticsEvent(severity, message);
            
            m_homaAnalytics.TrackEvent(errorEvent);
        }

        #region AnalyticsBase
        public override void TrackAdRevenue(AdRevenueData adRevenueData)
        {
            var adRevenueEvent = new AdRevenueAnalyticsEvent(adRevenueData);
            
            m_homaAnalytics.TrackEvent(adRevenueEvent);
        }

        public override void TrackEvent(AnalyticsEvent analyticsEvent)
        {
            m_homaAnalytics.TrackEvent(new HomaBellyRuntimeAnalyticsEvent(analyticsEvent));
        }

        #endregion
    }
}