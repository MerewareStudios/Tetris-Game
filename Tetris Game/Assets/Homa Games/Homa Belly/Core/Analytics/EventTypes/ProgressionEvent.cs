namespace HomaGames.HomaBelly.Internal.Analytics
{
    public abstract class ProgressionEvent : AnalyticsEvent
    {
        public int LevelId { get; }
        protected ProgressionEvent(int levelId) : base(HomaGames.HomaBelly.EventCategory.progression_event.ToString())
        {
            LevelId = levelId;
        }
    }
}