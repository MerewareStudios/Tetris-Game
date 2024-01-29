namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class RewardedAdTaken : RewardedAdEvent
    {
        public RewardedAdTaken(string adName, string impressionId, int levelId, AdPlacementType adPlacementType) 
            : base(adName, impressionId, levelId, adPlacementType)
        {
        }

        public override AnalyticsEventValue ToGameAnalyticsFormat()
        {
            return new AnalyticsEventValue($"Rewarded:Taken:{RewardedAdName}:{LevelId}:{AdPlacementType}");
        }
    }
}