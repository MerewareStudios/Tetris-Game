namespace HomaGames.HomaBelly.Internal.Analytics
{
    public abstract class FetchEvent : AnalyticsEvent
    {
        protected FetchEvent() : base(HomaGames.HomaBelly.EventCategory.remote_configuration_fetch_event.ToString())
        {
        }
    }
}