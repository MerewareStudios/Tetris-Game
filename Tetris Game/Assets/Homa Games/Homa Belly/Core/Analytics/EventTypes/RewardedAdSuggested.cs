namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class RewardedAdSuggested : RewardedAdEvent
    {
        public RewardedAdSuggested(string adName, string impressionId, int levelId, AdPlacementType adPlacementType) : base(adName, impressionId, levelId, adPlacementType)
        {
        }

        public override AnalyticsEventValue ToGameAnalyticsFormat()
        {
            return new AnalyticsEventValue($"Rewarded:Suggested:{RewardedAdName}:{LevelId}:{AdPlacementType}");
        }
    }
}