#if UNITY_EDITOR
using System.Text.RegularExpressions;
#endif
using System;
using System.Linq;
using GameAnalyticsSDK;
using UnityEngine;

//ANALYTICS_ENABLED
public static class AnalyticsManager
{
    private const string AnalyticsEnabled = "ANALYTICS_ENABLED";

    private static int _currentTrackedLevel;
    private static int _currentTrackedStartTime;

    public static void Init()
    {
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
        float realTime = Time.realtimeSinceStartup;
        
        GameAnalytics.NewDesignEvent(eventName, realTime);
#if UNITY_EDITOR
        Log(eventName, realTime, EventType.Design);
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
            Debug.LogWarning(message);
        }
        else
        {
            Debug.LogError(message);
        }
    }

    public enum EventType
    {
        Progression,
        Design,
    }
#endif
}
