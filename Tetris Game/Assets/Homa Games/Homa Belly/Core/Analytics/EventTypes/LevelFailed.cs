namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class LevelFailed : ProgressionEvent
    {
        public string Reason { get; }
        public float? PercentCompleted { get; }
        
        public LevelFailed(int levelId, string reason, float? percentCompleted = null) : base(levelId)
        {
            Reason = reason;
            PercentCompleted = percentCompleted;
        }

        public override void TrackThroughIAnalytics(IAnalytics implementer, bool isForwardingToHomaAnalytics)
        {
            implementer.TrackProgressionEvent(ProgressionStatus.Fail, "Level_" + LevelId);
        }
    }
}