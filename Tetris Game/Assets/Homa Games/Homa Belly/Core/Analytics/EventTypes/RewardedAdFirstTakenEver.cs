namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class RewardedAdFirstTakenEver : RewardedAdEvent
    {
        public long GameplaySeconds { get; }

        public RewardedAdFirstTakenEver(string adName, string impressionId, int levelId,
            AdPlacementType adPlacementType, long gameplaySeconds) 
            : base(adName, impressionId, levelId, adPlacementType)
        {
            GameplaySeconds = gameplaySeconds;
        }

        public override AnalyticsEventValue ToGameAnalyticsFormat()
        {
            return new AnalyticsEventValue($"Rewarded:FirstWatched:{RewardedAdName}:{AdPlacementType}", GameplaySeconds);
        }
    }
}