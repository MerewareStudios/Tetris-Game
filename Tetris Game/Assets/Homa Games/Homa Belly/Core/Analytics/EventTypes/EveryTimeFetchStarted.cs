namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class EveryTimeFetchStarted : FetchEvent
    {
        public EveryTimeFetchStarted()
        {
        }

        public override AnalyticsEventValue ToGameAnalyticsFormat()
        {
            return new AnalyticsEventValue("EveryTimeFetch:Started");
        }
    }
}