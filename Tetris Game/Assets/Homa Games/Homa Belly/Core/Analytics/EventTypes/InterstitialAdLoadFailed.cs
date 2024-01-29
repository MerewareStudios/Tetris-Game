namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class InterstitialAdLoadFailed : InterstitialAdEvent
    {
        public InterstitialAdLoadFailed(string adName, string impressionId, int levelId,
            AdPlacementType adPlacementType) : base(adName, impressionId, levelId, adPlacementType)
        {
        }

        public override AnalyticsEventValue ToGameAnalyticsFormat()
        {
            return new AnalyticsEventValue($"Interstitials:LoadFailed:{InterstitialAdName}:{LevelId}:{AdPlacementType}");
        }
    }
}