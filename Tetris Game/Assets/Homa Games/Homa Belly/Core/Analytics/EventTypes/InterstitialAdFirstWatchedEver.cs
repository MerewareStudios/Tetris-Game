namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class InterstitialAdFirstWatchedEver : InterstitialAdEvent
    {
        public long GameplaySeconds { get; }
        public InterstitialAdFirstWatchedEver(string adName, string impressionId, int levelId,
            AdPlacementType adPlacementType, long gameplaySeconds) : base(adName, impressionId, levelId, adPlacementType)
        {
            GameplaySeconds = gameplaySeconds;
        }

        public override AnalyticsEventValue ToGameAnalyticsFormat()
        {
            return new AnalyticsEventValue($"Interstitials:FirstWatched:{InterstitialAdName}:{LevelId}:{AdPlacementType}", GameplaySeconds);
        }
    }
}