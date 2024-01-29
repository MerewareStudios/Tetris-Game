using System;
using System.Collections.Generic;
using System.Diagnostics;
using HomaGames.HomaBelly.Internal.Analytics;
using JetBrains.Annotations;
#if UNITY_PURCHASING
using UnityEngine.Purchasing;
#endif

namespace HomaGames.HomaBelly
{
    public static class AnalyticsEventTracker
    {
        internal static event Action<AnalyticsEvent> EventTracked;
        
        /// <summary>
        /// Internal method to track events. Receives the AnalyticsEvent object and forwards it to implementers.
        /// </summary>
        /// <param name="analyticsEvent"></param>
        /// <param name="analyticsEvent"></param>
        public static EventId TrackEvent(this AnalyticsEvent analyticsEvent)
        {
            foreach (var analyticsBase in GetAllAnalyticsDependencies())
            {
                analyticsBase.TrackEvent(analyticsEvent);
            }

            Log(new object[]
            {
                "eventName", analyticsEvent.EventName,
                "eventCategory", analyticsEvent.EventCategory,
                "eventValue", analyticsEvent.ToJson(),
            });

            EventTracked?.Invoke(analyticsEvent);
            
            return analyticsEvent.EventId;
        }
        
        /// <summary>
        /// Tracks an Ad Revenue event
        /// </summary>
        /// <param name="adRevenueData">Object holding all ad revenue data to be sent</param>
        public static void TrackAdRevenue(AdRevenueData adRevenueData)
        {
            foreach (var analyticsBase in GetAllAnalyticsDependencies())
            {
                analyticsBase.TrackAdRevenue(adRevenueData);
            }
            
            foreach (var attribution in GetAllAttributionDependencies())
            {
                attribution.TrackAdRevenue(adRevenueData);
            }

            Log(new object[]
            {
                nameof(adRevenueData), adRevenueData,
            });
        }
        
        /// <summary>
        /// Tracks an In App Purchase event. Purchase can be verified if
        /// `transactionId` and `payload` are informed for the corresponding platforms
        /// </summary>
        /// <param name="productCategory">The category of the purchased product</param>
        /// <param name="productId">The product id purchased</param>
        /// <param name="currencyCode">The currency code of the purchase</param>
        /// <param name="unitPrice">The unit price</param>
        /// <param name="currencyType">(Optional) The type of currency purchased</param>
        /// <param name="amount">(Optional) The amount of currency purchased</param>
        /// <param name="transactionId">(Optional) The transaction id for the IAP validation</param>
        /// <param name="payload">(Optional - Only Android) Payload for Android IAP validation</param>
        /// <param name="isRestored">(Optional) If the purchase is restored. Default is false</param>
        public static void TrackInAppPurchaseEvent(ProductCategory productCategory, string productId, string currencyCode, double unitPrice, CurrencyType currencyType = CurrencyType.Other, int amount = 0, string transactionId = null, string payload = null, bool isRestored = false)
        {
            foreach (var attribution in GetAllAttributionDependencies())
            {
                attribution.TrackInAppPurchaseEvent(productId, currencyCode, unitPrice, transactionId, payload, isRestored);
            }

            Log(new object[]
            {
                nameof(productId), productId,
                nameof(currencyCode), currencyCode,
                nameof(unitPrice), unitPrice,
                nameof(transactionId), transactionId,
                nameof(payload), payload,
                nameof(isRestored), isRestored,
            });
        }
        
#if UNITY_PURCHASING
        /// <summary>
        /// Tracks an In App Purchase event
        /// </summary>
        /// <param name="product">The Unity IAP Product purchased</param>
        /// <param name="isRestored">(Optional) If the purchase is restored. Default is false</param>
        /// <param name="currencyType">(Optional) The type of currency purchased</param>
        /// <param name="amount">(Optional) The amount of currency purchased</param>
        public static void TrackInAppPurchaseEvent(UnityEngine.Purchasing.Product product, bool isRestored = false, CurrencyType currencyType = CurrencyType.Other, int amount = 0)
        {
            foreach (var attribution in GetAllAttributionDependencies())
            {
                attribution.TrackInAppPurchaseEvent(product, isRestored);
            }

            Log(new object[]
            {
                nameof(product), product,
                nameof(isRestored), isRestored,
            });
        }
#endif

        [Conditional("UNITY_EDITOR")]
        private static void Log(object[] parameters)
        {
            var str = "[Homa Belly] Tracking Event : ";

            for (var i = 0; i < parameters.Length; i += 2)
            {
                string paramName = parameters[i].ToString();

                if (i + 1 < parameters.Length)
                {
                    string paramValue = parameters[i + 1]?.ToString() ?? "null";

                    str += $"{paramName}={paramValue} ";
                }
            }

            HomaGamesLog.Debug(str.Trim());
        }
        
        [NotNull, ItemNotNull, Pure]
        private static List<AnalyticsBase> GetAllAnalyticsDependencies()
        {
#if UNITY_EDITOR
            return new List<AnalyticsBase>();
#else
            return HomaBridgeServices.GetAnalytics(out var analytics) ? analytics : new List<AnalyticsBase>();
#endif
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
    }
}