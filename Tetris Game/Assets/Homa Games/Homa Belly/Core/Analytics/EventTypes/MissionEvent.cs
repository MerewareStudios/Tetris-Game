namespace HomaGames.HomaBelly.Internal.Analytics
{
    public abstract class MissionEvent : ProgressionEvent
    {
        public string MissionId { get; }
        public MissionEvent(string missionId, int levelId) : base(levelId)
        {
            MissionId = missionId;
        }
    }
}