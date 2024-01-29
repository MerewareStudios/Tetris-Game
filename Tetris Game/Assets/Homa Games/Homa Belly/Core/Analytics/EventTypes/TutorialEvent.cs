namespace HomaGames.HomaBelly.Internal.Analytics
{
    public abstract class TutorialEvent : AnalyticsEvent
    {
        public int TutorialStep { get; }

        protected TutorialEvent(int tutorialStep) : base(HomaGames.HomaBelly.EventCategory.progression_event.ToString())
        {
            TutorialStep = tutorialStep;
        }
    }
}