namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class SessionPlayed : SessionEvent
    {
        public int SessionNumber { get; }
        public long SessionLength { get; }
        public SessionPlayed(int sessionNumber, long sessionLength)
        {
            SessionNumber = sessionNumber;
            SessionLength = sessionLength;
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
            return new AnalyticsEventValue($"Session:{SessionNumber}:Played", SessionLength);
        }
    }
}