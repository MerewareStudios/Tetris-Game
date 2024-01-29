namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class GameLaunched : SessionEvent
    {
        public GameLaunched()
        {
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
            return new AnalyticsEventValue("GameLaunched");
        }
    }
}