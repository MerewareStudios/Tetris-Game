using System.Collections.Generic;

namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class CustomEvent : AnalyticsEvent
    {
        public override string EventName { get; }
        private Dictionary<string, object> Parameters { get; }

        public CustomEvent(string eventName, Dictionary<string, object> parameters) : base(HomaGames.HomaBelly.EventCategory.custom_event.ToString())
        {
            EventName = ToUnderscoreCase(eventName);
            Parameters = new Dictionary<string, object>();

            if (parameters == null) return;
            foreach (var keyValuePair in parameters)
            {
                Parameters.Add(ToUnderscoreCase(keyValuePair.Key), keyValuePair.Value);
            }
        }

        public override Dictionary<string, object> GetData()
        {
            return Parameters;
        }

        public override AnalyticsEventValue ToGameAnalyticsFormat()
        {
            return new AnalyticsEventValue(EventName);
        }
    }
}