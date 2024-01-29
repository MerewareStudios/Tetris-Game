using System;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Use this utility to log anything in HomaAnalytics.
    /// This ensures no logging loop is triggered when sending logs analytics.
    /// On top of adding consistency to HomaAnalytics logs.
    /// </summary>
    public static class HomaAnalyticsLogger
    {
        private const string PREFIX_BASE = "HomaAnalytics";
        private const string PREFIX = "["+PREFIX_BASE+"]";
        private const string PREFIX_WITH_ERROR = "["+PREFIX_BASE+" ERROR]";
        private const string PREFIX_WITH_WARNING = "["+PREFIX_BASE+" WARNING]";

        public static void Log(string log)
        {
            Debug.Log($"{PREFIX} {log}");
        }

        public static void LogWarning(string log)
        {
            Debug.LogWarning($"{PREFIX_WITH_WARNING} {log}");
        }
        
        public static void LogError(string log)
        {
            Debug.LogError($"{PREFIX_WITH_ERROR} {log}");
        }
    }
}