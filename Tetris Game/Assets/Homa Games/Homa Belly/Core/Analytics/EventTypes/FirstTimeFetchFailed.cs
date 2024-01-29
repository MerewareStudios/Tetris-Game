namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class FirstTimeFetchFailed: FetchEvent
    {
        public string Reason { get; }
        
        public FirstTimeFetchFailed(string reason)
        {
            Reason = reason;
        }

        public override AnalyticsEventValue ToGameAnalyticsFormat()
        {
            return new AnalyticsEventValue($"FirstTimeFetch:Failed:{Reason}");
        }
    }
}