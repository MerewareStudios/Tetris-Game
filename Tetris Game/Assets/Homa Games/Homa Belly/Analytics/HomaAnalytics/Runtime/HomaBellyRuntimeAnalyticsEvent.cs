using System.Collections;
using System.Collections.Generic;
using HomaGames.HomaBelly.Internal.Analytics;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public class HomaBellyRuntimeAnalyticsEvent : RuntimeAnalyticsEvent
    {
        public HomaBellyRuntimeAnalyticsEvent(AnalyticsEvent bellyEvent)
            : base(bellyEvent.EventName, bellyEvent.EventCategory, bellyEvent.GetData())
        {
            string bellyEventIdString = bellyEvent.EventId?.ToString();
            
            if (!string.IsNullOrWhiteSpace(bellyEventIdString))
                EventId = bellyEventIdString;
        }
    }
}
