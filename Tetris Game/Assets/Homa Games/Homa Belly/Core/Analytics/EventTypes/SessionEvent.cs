namespace HomaGames.HomaBelly.Internal.Analytics
{
    public abstract class SessionEvent :AnalyticsEvent 
    {
        protected SessionEvent() : base(HomaGames.HomaBelly.EventCategory.session_event.ToString())
        {
        }
    }
}