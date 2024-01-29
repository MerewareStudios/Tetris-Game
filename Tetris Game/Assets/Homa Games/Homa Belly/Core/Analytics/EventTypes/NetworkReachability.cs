namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class NetworkReachability : AnalyticsEvent
    {
        public string Reachability { get; }
        public NetworkReachability(bool reachability) : base(HomaGames.HomaBelly.EventCategory.system_event.ToString())
        {
            Reachability = reachability ? "reachable" : "unreachable";
        }

        public override AnalyticsEventValue ToGameAnalyticsFormat()
        {
            return new AnalyticsEventValue("NetworkReachability:" + (Reachability == "reachable" ? "Reachable" : "NotReachable"));
        }
    }
}