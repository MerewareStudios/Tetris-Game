namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class MissionStarted : MissionEvent
    {
        public MissionStarted(string missionId, int levelId) : base(missionId, levelId)
        {
        }

        public override AnalyticsEventValue ToGameAnalyticsFormat()
        {
            return new AnalyticsEventValue($"Mission:{MissionId}:Started");
        }
    }
}