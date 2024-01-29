

using System.Collections.Generic;

namespace HomaGames.HomaBelly
{
    internal static class AttributionEvents
    {
        public static void TrackLevelCompleted(int levelId)
        {
            HomaBelly.Instance.TrackAttributionEvent("level_completed", new Dictionary<string, object>
            {
                { "level_id", levelId }
            });
        }
        
        public static void TrackRewardedWatched()
        {
            HomaBelly.Instance.TrackAttributionEvent("rv_watched");
        }
        
        public static void TrackInterstitialWatched()
        {
            HomaBelly.Instance.TrackAttributionEvent("is_watched");
        }
    }
}