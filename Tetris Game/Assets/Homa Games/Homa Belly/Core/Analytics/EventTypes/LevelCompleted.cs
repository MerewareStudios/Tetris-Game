namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class LevelCompleted : ProgressionEvent
    {
        public LevelCompleted(int levelId) : base(levelId)
        {
        }

        public override void TrackThroughIAnalytics(IAnalytics implementer, bool isForwardingToHomaAnalytics)
        {
            implementer.TrackProgressionEvent(ProgressionStatus.Complete, "Level_" + LevelId);
        }
    }
}