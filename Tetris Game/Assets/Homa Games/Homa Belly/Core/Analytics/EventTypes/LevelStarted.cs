using System.Collections.Generic;

namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class LevelStarted : ProgressionEvent
    {
        public LevelStarted(int levelId) : base(levelId)
        {
        }

        public override void TrackThroughIAnalytics(IAnalytics implementer, bool isForwardingToHomaAnalytics)
        {
            implementer.TrackProgressionEvent(ProgressionStatus.Start, "Level_" + LevelId);
        }
    }
}