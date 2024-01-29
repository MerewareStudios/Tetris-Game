namespace HomaGames.HomaBelly.Internal.Analytics
{
    public abstract class IAPEvent : AnalyticsEvent
    {
        public IAPEvent() : base(HomaGames.HomaBelly.EventCategory.iap_event.ToString())
        {
        }
    }
}