using System;
using System.Text;
using HomaGames.HomaBelly.Internal.Analytics;
using UnityEngine;
using NetworkReachability = HomaGames.HomaBelly.Internal.Analytics.NetworkReachability;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Acts as a wrapper to allow old <see cref="IAnalytics"/> implementers
    /// to work with the new <see cref="AnalyticsBase"/> based system.
    /// </summary>
    public class AnalyticsInterfaceForwarder : AnalyticsBase
    {
        private readonly IAnalytics Implementer;
        
        private bool IsForwardingToHomaAnalytics { get; }

        public AnalyticsInterfaceForwarder(IAnalytics implementer)
        {
            Implementer = implementer;

            IsForwardingToHomaAnalytics = Implementer.GetType().Name.Contains("HomaAnalytics");
        }

        public override void Initialize(Action onInitialized = null)
        {
            if (Implementer is IAnalyticsWithInitializationCallback analyticsWithInitializationCallback)
            {
                analyticsWithInitializationCallback.Initialize(onInitialized);
            }
            else
            {
                Implementer.Initialize();
                
                onInitialized?.Invoke();
            }
        }

        public override void OnApplicationPause(bool pause) 
            => Implementer.OnApplicationPause(pause);

        public override void ValidateIntegration() 
            => Implementer.ValidateIntegration();

        public override void SetUserIsAboveRequiredAge(bool consent) 
            => Implementer.SetUserIsAboveRequiredAge(consent);

        public override void SetTermsAndConditionsAcceptance(bool consent) 
            => Implementer.SetTermsAndConditionsAcceptance(consent);

        public override void SetAnalyticsTrackingConsentGranted(bool consent) 
            => Implementer.SetAnalyticsTrackingConsentGranted(consent);

        public override void SetTailoredAdsConsentGranted(bool consent) 
            => Implementer.SetTailoredAdsConsentGranted(consent);
        
#if UNITY_PURCHASING
        public override void TrackInAppPurchaseEvent(UnityEngine.Purchasing.Product product, bool isRestored = false)
            => Implementer.TrackInAppPurchaseEvent(product, isRestored);
#endif

        public override void TrackErrorEvent(ErrorSeverity severity, string message) 
            => Implementer.TrackErrorEvent(severity, message);

        public override void TrackAdRevenue(AdRevenueData adRevenueData)
        {
            if (Implementer is IAnalyticsAdRevenue instance)
            {
                instance.TrackAdRevenue(adRevenueData);
            }
        }


        public override void SetCustomDimension01(string customDimension)
        {
            if (Implementer is ICustomDimensions instance)
            {
                instance.SetCustomDimension01(customDimension);
            }
        }

        public override void SetCustomDimension02(string customDimension)
        {
            if (Implementer is ICustomDimensions instance)
            {
                instance.SetCustomDimension02(customDimension);
            }
        }

        public override void SetCustomDimension03(string customDimension)
        {
            if (Implementer is ICustomDimensions instance)
            {
                instance.SetCustomDimension03(customDimension);
            }
        }

        public override void TrackEvent(AnalyticsEvent analyticsEvent)
        {
            analyticsEvent.TrackThroughIAnalytics(Implementer, IsForwardingToHomaAnalytics);
        }
    }
}
