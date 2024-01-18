#define TOOLS_SANDBOX
#if ADJUST
    using com.adjust.sdk;
#endif
using UnityEngine;

public static class Tools
{
#if ADJUST
    public static class AdjustSDK
    {
        private const string AppToken = "p9oztjpzips0";
        private const string EventToken_SocialShare = "m3qlel";
        private const string EventToken_FirstPurchase = "xxasca";
        private const string EventToken_StoreComment = "wx5vts";
        private const string EventToken_FirstAdWatched = "rndb0x";
        private const string EventToken_OnboardingComplete = "3d674q";
        public static void Init(string externalDeviceID, bool isUnderAgeForCoppa)
        {
            Log("Init");
#if TOOLS_SANDBOX
            AdjustConfig config = new AdjustConfig(AppToken, AdjustEnvironment.Sandbox,true); // override log level true
            config.setLogLevel(AdjustLogLevel.Verbose); // log type supress
#else 
            AdjustConfig config = new AdjustConfig(AppToken, AdjustEnvironment.Production);
#endif
            // config.setLogDelegate(msg => Debug.Log(msg)); // change logging delegate for Windows based
            config.setExternalDeviceId(externalDeviceID);
            config.setCoppaCompliantEnabled(isUnderAgeForCoppa);
            // config.setPlayStoreKidsAppEnabled(false);

            Adjust.start(config);
        }

        public static void Event_SocialShare()
        {
            Adjust.trackEvent(new AdjustEvent(EventToken_SocialShare));
            Log("Event_SocialShare");
        }
        
        public static void Event_FirstPurchase()
        {
            if (Account.Current.firstPurchase)
            {
                return;
            }

            Account.Current.firstPurchase = true;
            Adjust.trackEvent(new AdjustEvent(EventToken_FirstPurchase));
            Log("Event_FirstPurchase");
        }
        
        public static void Event_StoreComment()
        {
            if (Account.Current.commented)
            {
                return;
            }

            Account.Current.commented = true;
            Adjust.trackEvent(new AdjustEvent(EventToken_StoreComment));
            Log("EventToken_StoreComment");
        }
        
        public static void Event_FirstAdWatched()
        {
            if (Account.Current.firstAd)
            {
                return;
            }

            Account.Current.firstAd = true;
            Adjust.trackEvent(new AdjustEvent(EventToken_FirstAdWatched));
            Log("EventToken_FirstAdWatched");
        }
        
        public static void Event_OnboardingComplete()
        {
            Adjust.trackEvent(new AdjustEvent(EventToken_OnboardingComplete));
            Log("EventToken_OnboardingComplete");
        }
        
        [System.Diagnostics.Conditional("TOOLS_SANDBOX")]
        private static void Log(string str)
        {
            Debug.Log("<color=#000000>Tools\nAdjustSDK\n"+ str + "</color>");
        }
    }
#endif
    
    
    
}
