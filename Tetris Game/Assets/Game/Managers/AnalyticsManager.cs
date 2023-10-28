#if UNITY_EDITOR
using System.Text.RegularExpressions;
#endif
using System;
using System.Linq;
using UnityEngine;
//ANALYTICS_ENABLED
public static class AnalyticsManager
{
    private const string AnalyticsEnabled = "ANALYTICS_ENABLED";

    private static int _currentTrackedLevel;
    
    [System.Diagnostics.Conditional(AnalyticsEnabled)]
    public static void LevelStart(int level)
    {
        _currentTrackedLevel = level;
        string progression = "LEVEL_" + _currentTrackedLevel;

#if UNITY_EDITOR
        string trace = progression;
        Log(trace, EventType.Progression);
#endif
    }
    
    [System.Diagnostics.Conditional(AnalyticsEnabled)]
    public static void LevelSuccess()
    {
        string progression = "LEVEL_" + _currentTrackedLevel;
#if UNITY_EDITOR
        string trace = progression;
        Log(trace, EventType.Progression);
#endif
    }
    
    [System.Diagnostics.Conditional(AnalyticsEnabled)]
    public static void LevelFail()
    {
        string progression = "LEVEL_" + _currentTrackedLevel;
#if UNITY_EDITOR
        string trace = progression;
        Log(trace, EventType.Progression);
#endif
    }
    
    [System.Diagnostics.Conditional(AnalyticsEnabled)]
    public static void OnboardingStepComplete(string stepName)
    {
        string trace = "TUTORIAL:" + stepName;
        float realTime = Time.realtimeSinceStartup;
#if UNITY_EDITOR
        Log(trace, EventType.Design);
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
    
    private static void Log(string trace, EventType eventType)
    {
        bool isValid = Validate(trace, eventType);
        string tag = isValid ? "<color=#10FF10>VALID" : "<color=red>INVALID";
        string eventTypeTag = "<color=#6060DD>" + eventType.ToString().ToUpper() + " EVENT</color>\n";
        string message = tag + "</color>\n" + eventTypeTag + "<color=yellow>" + trace + "</color>";
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
