using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public static class HomaAnalyticsLogListener
    {
        private static readonly Queue<LogElement> LogElementBuffer = new Queue<LogElement>();

        private static Application.LogCallback LogMessageReceived;

        /// <summary>
        /// This callback gives pre-filtered logs.
        /// Only logs relevant to HomaAnalytics go through.
        /// </summary>
        public static void Initialize(Application.LogCallback callback)
        {
            Application.logMessageReceived -= OnLogReceived;
            Application.logMessageReceived += OnLogReceived;
            
            LogMessageReceived = callback;
            
            foreach (var logElement in LogElementBuffer)
            {
                LogMessageReceived.Invoke(
                    logElement.condition,
                    logElement.stacktrace,
                    logElement.type
                );
            }
            
            LogElementBuffer.Clear();
        }

        private static void OnLogReceived(string condition, string stacktrace, LogType type)
        {
            if(type == LogType.Log || stacktrace.Contains(nameof(HomaAnalyticsLogger))) return;
            if (LogMessageReceived != null)
            {
                LogMessageReceived.Invoke(condition, stacktrace, type);
            }
            else
            {
                LogElementBuffer.Enqueue(
                    new LogElement { condition = condition, stacktrace = stacktrace, type = type }
                    );
            }
        }
        

        private struct LogElement
        {
            public string condition;
            public string stacktrace; 
            public LogType type;
        }

        public static void Unsubscribe(Action<string, string, LogType> onLogReceived)
        {
            Application.logMessageReceived -= OnLogReceived;
        }
    }
}