using System;
using System.Collections.Generic;
using HomaGames.HomaBelly.Internal.Analytics;

namespace HomaGames.HomaBelly
{
    public abstract class AnalyticsBase
    {
        public virtual void Initialize(Action onInitialized = null)
        {
            
        }

        public virtual void OnApplicationPause(bool pause)
        {
            
        }

        public virtual void ValidateIntegration()
        {
            
        }

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
        
        public virtual void TrackErrorEvent(ErrorSeverity severity, string message)
        {
            
        }
        
        public virtual void TrackAdRevenue(AdRevenueData adRevenueData)
        {
            
        }


        public virtual void SetCustomDimension01(string customDimension)
        {
            
        }

        public virtual void SetCustomDimension02(string customDimension)
        {
            
        }

        public virtual void SetCustomDimension03(string customDimension)
        {
            
        }
        
        public virtual void TrackEvent(AnalyticsEvent analyticsEvent)
        {
            
        }
    }
}
