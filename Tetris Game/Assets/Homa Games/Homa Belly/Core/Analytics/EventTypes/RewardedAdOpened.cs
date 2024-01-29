namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class RewardedAdOpened : RewardedAdEvent
    {
        public RewardedAdOpened(string adName, string impressionId, int levelId, AdPlacementType adPlacementType) 
            : base(adName, impressionId, levelId, adPlacementType)
        {
        }

        public override AnalyticsEventValue ToGameAnalyticsFormat()
        {
            return new AnalyticsEventValue($"Rewarded:Opened:{RewardedAdName}:{LevelId}:{AdPlacementType}");
        }
    }
}