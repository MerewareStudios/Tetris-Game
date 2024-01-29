namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class InterstitialAdShowFailed : InterstitialAdEvent
    {
        public InterstitialAdShowFailed(string adName, string impressionId, int levelId,
            AdPlacementType adPlacementType) : base(adName, impressionId, levelId, adPlacementType)
        {
        }

        public override AnalyticsEventValue ToGameAnalyticsFormat()
        {
            return new AnalyticsEventValue($"Interstitials:ShowFailed:{InterstitialAdName}:{LevelId}:{AdPlacementType}");
        }
    }
}