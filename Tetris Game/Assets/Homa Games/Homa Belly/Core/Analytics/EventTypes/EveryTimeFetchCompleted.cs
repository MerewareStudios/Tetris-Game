namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class EveryTimeFetchCompleted : FetchEvent
    {
        public EveryTimeFetchCompleted()
        {
        }

        public override AnalyticsEventValue ToGameAnalyticsFormat()
        {
            return new AnalyticsEventValue("EveryTimeFetch:Completed");
        }
    }
}