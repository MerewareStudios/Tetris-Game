namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class InterstitialAdShowSucceeded : InterstitialAdEvent
    {
        public InterstitialAdShowSucceeded(string adName, string impressionId, int levelId,
            AdPlacementType adPlacementType) : base(adName, impressionId, levelId, adPlacementType)
        {
        }

        public override AnalyticsEventValue ToGameAnalyticsFormat()
        {
            return new AnalyticsEventValue($"Interstitials:ShowSucceeded:{InterstitialAdName}:{LevelId}:{AdPlacementType}");
        }
    }
}