namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class MissionFailed : MissionEvent
    {
        public MissionFailed(string missionId, int levelId) : base(missionId, levelId)
        {
        }

        public override AnalyticsEventValue ToGameAnalyticsFormat()
        {
            return new AnalyticsEventValue($"Mission:{MissionId}:Failed");
        }
    }
}