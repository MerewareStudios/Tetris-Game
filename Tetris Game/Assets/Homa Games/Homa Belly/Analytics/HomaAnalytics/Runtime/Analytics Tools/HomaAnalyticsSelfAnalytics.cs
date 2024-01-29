using System.Collections.Generic;
using HomaGames.HomaBelly.Internal.Analytics;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Class to monitor Homa Analytics with all installed analytics tools.
    /// </summary>
    public static class HomaAnalyticsSelfAnalytics
    {
        private static readonly HashSet<string> SentEvents = new HashSet<string>();
        public static void TrackInternal(string eventName,bool sessionLimited = true)
        {
            if (sessionLimited && SentEvents.Contains(eventName))
                return;
            new InternalAnalyticsEvent(eventName).TrackEvent();
            SentEvents.Add(eventName);
        }

        private class InternalAnalyticsEvent : AnalyticsEvent
        {
            private const string EVENT_CATEGORY = "internal";
            public override string EventName { get; }

            public InternalAnalyticsEvent(string eventName) : base(EVENT_CATEGORY)
            {
                EventName = ToUnderscoreCase(eventName);
            }
        }
    }
    
}
