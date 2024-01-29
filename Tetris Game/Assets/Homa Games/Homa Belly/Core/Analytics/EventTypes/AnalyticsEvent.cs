using System.Collections.Generic;
using System.Text;
using HomaGames.HomaBelly.Utilities;
using JetBrains.Annotations;

namespace HomaGames.HomaBelly.Internal.Analytics
{
    public abstract class AnalyticsEvent
    {
        public EventId EventId { get; }
        public string EventCategory { get; }
        public virtual string EventName => ToUnderscoreCase(this.GetType().Name);
        
        protected AnalyticsEvent(string eventCategory)
        {
            this.EventCategory = eventCategory;
            this.EventId = new EventId();
        }

        public string ToJson()
        {
            return Json.Serialize(GetData());
        }

        public virtual Dictionary<string, object> GetData()
        {
            Dictionary<string, object> output = new Dictionary<string, object>();
            
            var properties = PropertyHelper.GetProperties(GetType());
            foreach (var property in properties)
            {
                if (property.DeclaringType == typeof(AnalyticsEvent)) continue;
                output.Add(ToUnderscoreCase(property.Name), property.Getter(this));
            }

            return output;
        }
        
        public static string ToUnderscoreCase(string str)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < str.Length; i++)
            {
                if (i > 0 && char.IsUpper(str[i]) && (char.IsLower(str[i - 1]) || (i+1 < str.Length && char.IsLetter(str[i-1]) && char.IsLower(str[i + 1]))))
                    builder.Append('_');
                builder.Append(char.ToLowerInvariant(str[i]));
            }

            return builder.ToString();
        }

        [PublicAPI]
        public virtual void TrackThroughIAnalytics(IAnalytics implementer, bool isForwardingToHomaAnalytics)
        {
            var eventValue = ToGameAnalyticsFormat();
            implementer.TrackDesignEvent(eventValue.Name, eventValue.Value);
        }

        [PublicAPI]
        public virtual AnalyticsEventValue ToGameAnalyticsFormat()
        {
            return new AnalyticsEventValue(EventName);
        }
    }
}