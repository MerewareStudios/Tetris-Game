#if UNITY_EDITOR
#define LOG_WARNING
#define LOG_ERROR
using System.Text.RegularExpressions;
#endif
using System;
using System.Linq;
using GameAnalyticsSDK;
using UnityEngine;

public static class AnalyticsManager
{
    private const string AnalyticsEnabled = "ANALYTICS_ENABLED";

    private static int _currentTrackedLevel;
    private static int _currentTrackedStartTime;
    
    private static int _shopOpenedCount = 0;
    
    public static void Init()
    {
        _shopOpenedCount = 0;
        GameAnalytics.SetCustomId(Account.Current.guid);
        GameAnalytics.Initialize();
    }
    
    [System.Diagnostics.Conditional(AnalyticsEnabled)]
    public static void LevelStart(int level)
    {
        _currentTrackedLevel = level;
        _currentTrackedStartTime = (int)Time.realtimeSinceStartup;
        string progressionA = "LEVEL_" + _currentTrackedLevel;

        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, progressionA, _currentTrackedStartTime);
#if UNITY_EDITOR
        string trace = GAProgressionStatus.Start.ToString().ToUpper() + " " + progressionA;
        Log(trace, _currentTrackedStartTime, EventType.Progression);
#endif
    }
    
    [System.Diagnostics.Conditional(AnalyticsEnabled)]
    public static void LevelEnd(GAProgressionStatus status)
    {
        int levelDuration = (int)(Time.realtimeSinceStartup - _currentTrackedStartTime);
        string progressionA = "LEVEL_" + _currentTrackedLevel;

        GameAnalytics.NewProgressionEvent(status, progressionA, levelDuration);

#if UNITY_EDITOR
        string trace = status.ToString().ToUpper() + " " + progressionA;
        Log(trace, levelDuration, EventType.Progression);
#endif
    }
    
    [System.Diagnostics.Conditional(AnalyticsEnabled)]
    public static void OnboardingStepComplete(string stepName)
    {
        string eventName = "TUTORIAL:" + stepName;
        int realTime = (int)Time.realtimeSinceStartup;
        
        GameAnalytics.NewDesignEvent(eventName, realTime);
#if UNITY_EDITOR
        Log(eventName, realTime, EventType.Design);
#endif
    }
    
    [System.Diagnostics.Conditional(AnalyticsEnabled)]
    public static void OnBannerEnabled()
    {
        string eventName = "BANNER_ENABLED";
        int realTime = (int)Time.realtimeSinceStartup;
        
        GameAnalytics.NewDesignEvent(eventName, realTime);
#if UNITY_EDITOR
        Log(eventName, realTime, EventType.Design);
#endif
    }
    
    [System.Diagnostics.Conditional(AnalyticsEnabled)]
    public static void ShopOpened()
    {
        string eventName = "SHOP_OPEN";
        _shopOpenedCount++;
        
        GameAnalytics.NewDesignEvent(eventName, _shopOpenedCount);
#if UNITY_EDITOR
        Log(eventName, _shopOpenedCount, EventType.Design);
#endif
    }
    
    [System.Diagnostics.Conditional(AnalyticsEnabled)]
    public static void PurchasedBlockCount(int count)
    {
        string eventName = "UNLOCKED_BLOCK";
        
        GameAnalytics.NewDesignEvent(eventName, count);
#if UNITY_EDITOR
        Log(eventName, count, EventType.Design);
#endif
    }
    
    [System.Diagnostics.Conditional(AnalyticsEnabled)]
    public static void PurchasedWeaponCount(int count)
    {
        string eventName = "UNLOCKED_WEAPON";
        
        GameAnalytics.NewDesignEvent(eventName, count);
#if UNITY_EDITOR
        Log(eventName, count, EventType.Design);
#endif
    }
    
    [System.Diagnostics.Conditional(AnalyticsEnabled)]
    public static void WeaponMaxed(int weaponIndex)
    {
        string eventName = "WEAPON_MAXED";
        
        GameAnalytics.NewDesignEvent(eventName, weaponIndex);
#if UNITY_EDITOR
        Log(eventName, weaponIndex, EventType.Design);
#endif
    }
    
    [System.Diagnostics.Conditional(AnalyticsEnabled)]
    public static void PurchasedUpgrade(string upgradeName, int upgradeInstance)
    {
        string eventName = "UPGRADE:" + upgradeName;
        
        GameAnalytics.NewDesignEvent(eventName, upgradeInstance);
#if UNITY_EDITOR
        Log(eventName, upgradeInstance, EventType.Design);
#endif
    }
    
    [System.Diagnostics.Conditional(AnalyticsEnabled)]
    public static void PiggyBreak(int instance)
    {
        string eventName = "PIGGY_BREAK";
        
        GameAnalytics.NewDesignEvent(eventName, instance);
#if UNITY_EDITOR
        Log(eventName, instance, EventType.Design);
#endif
    }
    
    [System.Diagnostics.Conditional(AnalyticsEnabled)]
    public static void PiggyBreakDouble(int instance)
    {
        string eventName = "PIGGY_BREAK_DOUBLE";
        
        GameAnalytics.NewDesignEvent(eventName, instance);
#if UNITY_EDITOR
        Log(eventName, instance, EventType.Design);
#endif
    }
    
    [System.Diagnostics.Conditional(AnalyticsEnabled)]
    public static void PiggyBreakSkipped(int instance)
    {
        string eventName = "PIGGY_BREAK_SKIP";
        
        GameAnalytics.NewDesignEvent(eventName, instance);
#if UNITY_EDITOR
        Log(eventName, instance, EventType.Design);
#endif
    }
    
    [System.Diagnostics.Conditional(AnalyticsEnabled)]
    public static void PurchasedPower(int instance)
    {
        string eventName = "POWER_PURCHASE";
        
        GameAnalytics.NewDesignEvent(eventName, instance);
#if UNITY_EDITOR
        Log(eventName, instance, EventType.Design);
#endif
    }
    
    [System.Diagnostics.Conditional(AnalyticsEnabled)]
    public static void AdData(AdBreakScreen.AdState adState, AdBreakScreen.AdInteraction adInteraction, int instance)
    {
        string eventName = "SKIP:" + adState.ToString() + ":" + adInteraction.ToString();
        
        GameAnalytics.NewDesignEvent(eventName, instance);
#if UNITY_EDITOR
        Log(eventName, instance, EventType.Design);
#endif
    }
    
    [System.Diagnostics.Conditional(AnalyticsEnabled)]
    public static void OfferShown(OfferScreen.OfferType offerType, OfferScreen.AdPlacement adPlacement, OfferScreen.Mode mode)
    {
        string eventName = "OFFER:" + offerType.ToString() + ":" + adPlacement.ToString() + ":" + mode.ToString();
        int time = (int)Time.realtimeSinceStartup;
        GameAnalytics.NewDesignEvent(eventName, time);
#if UNITY_EDITOR
        Log(eventName, time, EventType.Design);
#endif
    }

#if UNITY_EDITOR
    private static bool Validate(string str, EventType eventType)
    {
        bool valid = true;
        switch (eventType)
        {
            case EventType.Progression:
                valid = true;
                break;
            case EventType.Design:
                bool correctCharSet = Regex.IsMatch(str, @"^[a-zA-Z0-9-_.,:()!?]+$");
                int separationCount = str.Split(':').Length;
                bool doubleSeparation = str.Contains("::");
                bool startOrEndWithSeparation = str.ElementAt(0).Equals(':') || str.ElementAt(str.Length-1).Equals(':');
                bool fitLength = str.Length <= 64;
                valid = correctCharSet && (separationCount is >= 1 and <= 5) && !doubleSeparation && !startOrEndWithSeparation && fitLength;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
        }

        return valid;
    }
    
    private static void Log(string trace, float extra, EventType eventType)
    {
        bool isValid = Validate(trace, eventType);
        string tag = isValid ? "<color=#10FF10>VALID" : "<color=red>INVALID";
        string eventTypeTag = "<color=#6060DD>" + eventType.ToString().ToUpper() + " EVENT</color>\n";
        string message = tag + "</color>\n" + eventTypeTag + "<color=yellow>" + trace + "</color>" + "\n" + extra;
        if (isValid)
        {
#if LOG_WARNING
            Debug.LogWarning(message);
#endif
        }
        else
        {
#if LOG_ERROR
            Debug.LogError(message);
        }
#endif
    }

    public enum EventType
    {
        Progression,
        Design,
    }
#endif
}
