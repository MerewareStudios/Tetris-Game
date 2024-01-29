namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class SessionStarted : SessionEvent
    {
        public int SessionNumber { get; }
        public float OfflineTime { get; }
        public SessionStarted(int sessionNumber, float offlineTime)
        {
            SessionNumber = sessionNumber;
            OfflineTime = offlineTime;
        }

        public override void TrackThroughIAnalytics(IAnalytics implementer, bool isForwardingToHomaAnalytics)
        {
            if (!isForwardingToHomaAnalytics)
            {
                base.TrackThroughIAnalytics(implementer, false);
            }
        }

        public override AnalyticsEventValue ToGameAnalyticsFormat()
        {
            return new AnalyticsEventValue($"Session:{SessionNumber}:Started", OfflineTime);
        }
    }
}