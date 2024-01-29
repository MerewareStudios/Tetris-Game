namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class EveryTimeFetchFailed: FetchEvent
    {
        public string Reason { get; }
        
        public EveryTimeFetchFailed(string reason)
        {
            Reason = reason;
        }

        public override AnalyticsEventValue ToGameAnalyticsFormat()
        {
            return new AnalyticsEventValue($"EveryTimeFetch:Failed:{Reason}");
        }
    }
}