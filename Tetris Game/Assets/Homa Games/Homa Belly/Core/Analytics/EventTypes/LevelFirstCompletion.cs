namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class LevelFirstCompletion : ProgressionEvent
    {
        public long LevelDuration { get; }
        public int Attempts { get; }
        public LevelFirstCompletion(int levelId, long levelDuration, int attempts) : base(levelId)
        {
            LevelDuration = levelDuration;
            Attempts = attempts;
        }

        public override void TrackThroughIAnalytics(IAnalytics implementer, bool isForwardingToHomaAnalytics)
        {
            if (isForwardingToHomaAnalytics) return;

            implementer.TrackDesignEvent($"Levels:Duration:{LevelId}", LevelDuration);
            implementer.TrackDesignEvent($"Levels:Attempts:{LevelId}", Attempts);
        }
    }
}