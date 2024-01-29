namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class InterstitialAdClosed : InterstitialAdEvent
    {
        public InterstitialAdClosed(string adName, string impressionId, int levelId, AdPlacementType adPlacementType) 
            : base(adName, impressionId, levelId, adPlacementType)
        {
        }

        public override AnalyticsEventValue ToGameAnalyticsFormat()
        {
            return new AnalyticsEventValue($"Interstitials:Closed:{InterstitialAdName}:{LevelId}:{AdPlacementType}");
        }
    }
}