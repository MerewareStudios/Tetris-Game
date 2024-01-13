#define LOG
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
        public static void Init()
        {
            Log("Init");
            // AdjustConfig config = new AdjustConfig(AppToken, Environment);
            
            // AdjustConfig config = new AdjustConfig(AppToken, Environment,true); // override log level true
            // config.setLogLevel(AdjustLogLevel.Suppress); // log type supress
            // AdjustConfig config = new AdjustConfig(AppToken, Environment);
            AdjustConfig config = new AdjustConfig(AppToken, Environment,true); // override log level true
            config.setLogLevel(AdjustLogLevel.Verbose); // log type supress
            // config.setLogDelegate(msg => Debug.Log(msg)); // change logging delegate for Windows based
            
            Adjust.start(config);
        }
        
        
        [System.Diagnostics.Conditional("LOG")]
        public static void Log(string str)
        {
            Debug.Log("<color=#000000>Tools\nAdjustSDK\n"+ str + "</color>");
        }
    }
#endif
    
    
    
}
