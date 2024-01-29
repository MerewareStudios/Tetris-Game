using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public static class AttributionEventCounter
    {
        private const string DATA_STORAGE_KEY = "com.homagames.homabelly.attribution_event_counter_data";
        private const char DATA_STORAGE_SEPARATOR = '|';

        private const string INSTALL_DATE_STORAGE_KEY = "com.homagames.homabelly.attribution_event_counter_install_date";
        
        private static readonly IReadOnlyList<EventType> AllEventTypes = new List<EventType>
        {
            EventType.Retention,
            EventType.RvWatched,
            EventType.IsWatched,
            EventType.SessionTime,
            EventType.LevelAchieved,
        };

        private static readonly Dictionary<EventType, int> CountPerEvent = new Dictionary<EventType, int>();

        static AttributionEventCounter()
        {        
            // Generic storage
            string storedData = PlayerPrefs.GetString(DATA_STORAGE_KEY, string.Empty);
            int[] storedCounts = storedData
                .Split(DATA_STORAGE_SEPARATOR)
                .Select(s => int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i) ? i : 0)
                .ToArray();

            for (int i = 0; i < AllEventTypes.Count; i++)
            {
                if (i < storedCounts.Length)
                    CountPerEvent.Add(AllEventTypes[i], storedCounts[i]);
                else
                    CountPerEvent.Add(AllEventTypes[i], 0);
            }
        }

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            SessionTimer().ListenForErrors();

            ComputeRetention();
        }

        // Playtime
        private static async Task SessionTimer()
        {
            int minutesElapsed = 0;

            while (true)
            {
                await Task.Delay(TimeSpan.FromMinutes(1));
                minutesElapsed++;
                
                SetValue(EventType.SessionTime, minutesElapsed);
            }
        }

        private static void ComputeRetention()
        {
            string installDateString = PlayerPrefs.GetString(INSTALL_DATE_STORAGE_KEY, string.Empty);
            DateTime installDate;
            
            if (! DateTime.TryParse(installDateString, out installDate))
            {
                installDate = DateTime.Today;
                PlayerPrefs.SetString(INSTALL_DATE_STORAGE_KEY, installDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                PlayerPrefs.Save();
            }

            int retention = Mathf.FloorToInt((float)(DateTime.Today - installDate).TotalDays);
            
            SetValue(EventType.Retention, retention);
        }
        
        [PublicAPI]
        public static void IncrementValue(EventType eventType)
        {
            if (!eventType.CanBeIncremented)
                throw new ArgumentException("Event type cannot be incremented.");
            
            if (CountPerEvent.TryGetValue(eventType, out var count))
            {
                CountPerEvent[eventType] = count + 1;
                TrackEventIfNecessary(eventType, count + 1);

                SaveData();
            }
        }

        [PublicAPI]
        public static void SetValue(EventType eventType, int count)
        {
            if (CountPerEvent.TryGetValue(eventType, out var currentCount) && currentCount < count)
            {
                CountPerEvent[eventType] = count;
                
                for (int i = currentCount + 1; i <= count; i++)
                    TrackEventIfNecessary(eventType, i);

                SaveData();
            }
        }

        private static void TrackEventIfNecessary(EventType eventType, int eventCount)
        {
            if (eventType.DoesEventCountNeedsToBeTracked(eventCount))
            {
                HomaBelly.Instance.TrackAttributionEvent(eventType.GetEventNameFor(eventCount));
            }
        }

        private static void SaveData()
        {
            PlayerPrefs.SetString(DATA_STORAGE_KEY, string.Join(DATA_STORAGE_SEPARATOR.ToString(), CountPerEvent.Values.Select(i => i.ToString(CultureInfo.InvariantCulture))));
            PlayerPrefs.Save();
        }


        public class EventType
        {
            public static readonly EventType Retention = new EventType(
                "retention",
                "Retention_D{0}",
                new []{1, 3, 7, 14},
                false
            );
            
            public static readonly EventType RvWatched = new EventType(
                "rv_watched",
                "RV_watched_{0}",
                new []{2, 5, 10, 20, 30, 40, 50, 60, 70},
                true
            );
            
            public static readonly EventType IsWatched = new EventType(
                "is_watched",
                "IS_watched_{0}",
                new []{1, 2, 3, 5, 10, 15, 20, 25, 30, 40, 50, 60, 70, 80},
                true
            );
            
            public static readonly EventType SessionTime = new EventType(
                "session_time",
                "Session_time_{0}",
                new []{5, 10, 20, 30},
                false
            );
            
            public static readonly EventType LevelAchieved = new EventType(
                "level_achieved",
                "Level_achieved_{0}",
                new []{5, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 130, 160, 190, 250, 300, 400, 500, 600, 700},
                true
            );

            public readonly bool CanBeIncremented;
                
            private readonly string AnalyticsEventName;
            private readonly string FormatString;
            private readonly int[] PossibleValues;

            private EventType(string analyticsEventName, string formatString, int[] possibleValues, bool canBeIncremented)
            {
                AnalyticsEventName = analyticsEventName;
                FormatString = formatString;
                PossibleValues = possibleValues;
                CanBeIncremented = canBeIncremented;
            }

            public bool DoesEventCountNeedsToBeTracked(int count) => PossibleValues.Contains(count);

            public string GetEventNameFor(int count) => string.Format(FormatString, count.ToString(CultureInfo.InvariantCulture));
        }
    }
}
