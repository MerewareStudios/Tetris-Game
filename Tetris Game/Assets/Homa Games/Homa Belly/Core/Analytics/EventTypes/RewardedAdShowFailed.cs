namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class RewardedAdShowFailed : RewardedAdEvent
    {
        public RewardedAdShowFailed(string adName, string impressionId, int levelId, AdPlacementType adPlacementType) 
            : base(adName, impressionId, levelId, adPlacementType)
        {
        }

        public override AnalyticsEventValue ToGameAnalyticsFormat()
        {
            return new AnalyticsEventValue($"Rewarded:Failed:{RewardedAdName}:{LevelId}:{AdPlacementType}");
        }
    }
}