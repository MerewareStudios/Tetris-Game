namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class RewardedAdFirstTakenSession : RewardedAdEvent
    {
        public long GameplaySeconds { get; }

        public RewardedAdFirstTakenSession(string adName, string impressionId, int levelId,
            AdPlacementType adPlacementType, long gameplaySeconds) 
            : base(adName, impressionId, levelId, adPlacementType)
        {
            GameplaySeconds = gameplaySeconds;
        }

        public override AnalyticsEventValue ToGameAnalyticsFormat()
        {
            return new AnalyticsEventValue($"Rewarded:FirstWatchedSession:{RewardedAdName}:{AdPlacementType}", GameplaySeconds);
        }
    }
}