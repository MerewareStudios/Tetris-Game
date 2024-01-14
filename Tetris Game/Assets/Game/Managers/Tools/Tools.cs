#define SANBOX
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
        private const AdjustEnvironment Environment = AdjustEnvironment.Sandbox;
        public static void Init(string externalDeviceID, bool isUnderAgeForCoppa)
        {
            Log("Init");
#if SANBOX
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
        
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD"), System.Diagnostics.Conditional("UNITY_EDITOR")]
        private static void Log(string str)
        {
            Debug.Log("<color=#000000>Tools\nAdjustSDK\n"+ str + "</color>");
        }
    }
#endif
    
    
    
}
