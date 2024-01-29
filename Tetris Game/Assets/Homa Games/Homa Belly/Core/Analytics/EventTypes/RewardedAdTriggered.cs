namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class RewardedAdTriggered : RewardedAdEvent
    {
        public RewardedAdTriggered(string adName, string impressionId, int levelId, AdPlacementType adPlacementType) : base(adName, impressionId, levelId, adPlacementType)
        {
        }

        public override AnalyticsEventValue ToGameAnalyticsFormat()
        {
            return new AnalyticsEventValue($"Rewarded:Triggered:{RewardedAdName}:{LevelId}:{AdPlacementType}");
        }
    }
}