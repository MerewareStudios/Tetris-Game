namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class RewardedAdClosed : RewardedAdEvent
    {
        public RewardedAdClosed(string adName, string impressionId, int levelId, AdPlacementType adPlacementType) 
            : base(adName, impressionId, levelId, adPlacementType)
        {
        }

        public override AnalyticsEventValue ToGameAnalyticsFormat()
        {
            return new AnalyticsEventValue($"Rewarded:Closed:{RewardedAdName}:{LevelId}:{AdPlacementType}");
        }
    }
}