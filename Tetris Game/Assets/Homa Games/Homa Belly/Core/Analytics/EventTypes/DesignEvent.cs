using System.Collections.Generic;
using System.Text;

namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class DesignEvent : AnalyticsEvent
    {
        public override string EventName { get; }
        public Dictionary<string, object> Dimensions { get; }

        public DesignEvent(string eventName, DesignDimensions designDimensions) : base(HomaGames.HomaBelly.EventCategory.design_event.ToString())
        {
            EventName = eventName;
            Dimensions = new Dictionary<string, object>();
            if (designDimensions != null)
            {
                if (!string.IsNullOrEmpty(designDimensions.Key1)) Dimensions.Add(ToUnderscoreCase(nameof(designDimensions.Key1)), designDimensions.Key1);
                if (!string.IsNullOrEmpty(designDimensions.Key2)) Dimensions.Add(ToUnderscoreCase(nameof(designDimensions.Key2)), designDimensions.Key2);
                if (!string.IsNullOrEmpty(designDimensions.Key3)) Dimensions.Add(ToUnderscoreCase(nameof(designDimensions.Key3)), designDimensions.Key3);
                if (!string.IsNullOrEmpty(designDimensions.Key4)) Dimensions.Add(ToUnderscoreCase(nameof(designDimensions.Key4)), designDimensions.Key4);
                if (!string.IsNullOrEmpty(designDimensions.Key5)) Dimensions.Add(ToUnderscoreCase(nameof(designDimensions.Key5)), designDimensions.Key5);
                if (designDimensions.Score.HasValue) Dimensions.Add(ToUnderscoreCase(nameof(designDimensions.Score)), designDimensions.Score.Value);
            }
        }

        public override Dictionary<string, object> GetData()
        {
            return Dimensions;
        }

        public override AnalyticsEventValue ToGameAnalyticsFormat()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(EventName);

            for (int i = 1; i < 6; i++)
            {
                if (Dimensions.TryGetValue($"key{i}", out object key))
                {
                    sb.Append($":{key}");
                }
            }
            
            return new AnalyticsEventValue(sb.ToString(), Dimensions.TryGetValue("score", out var score) ? (float)score : 0);
        }
    }
}