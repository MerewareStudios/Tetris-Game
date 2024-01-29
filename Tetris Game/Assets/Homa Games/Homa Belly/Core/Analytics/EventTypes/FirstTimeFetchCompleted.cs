namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class FirstTimeFetchCompleted : FetchEvent
    {
        public FirstTimeFetchCompleted()
        {
        }

        public override AnalyticsEventValue ToGameAnalyticsFormat()
        {
            return new AnalyticsEventValue("FirstTimeFetch:Completed");
        }
    }
}