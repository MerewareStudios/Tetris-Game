namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class FirstTimeFetchStarted : FetchEvent
    {
        public FirstTimeFetchStarted()
        {
        }

        public override AnalyticsEventValue ToGameAnalyticsFormat()
        {
            return new AnalyticsEventValue("FirstTimeFetch:Started");
        }
    }
}