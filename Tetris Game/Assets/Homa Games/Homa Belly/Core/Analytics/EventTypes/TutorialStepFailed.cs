namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class TutorialStepFailed : TutorialEvent
    {
        public TutorialStepFailed(int tutorialStep) : base(tutorialStep)
        {
        }

        public override AnalyticsEventValue ToGameAnalyticsFormat()
        {
            return new AnalyticsEventValue($"Tutorial:{TutorialStep}:Failed");
        }
    }
}