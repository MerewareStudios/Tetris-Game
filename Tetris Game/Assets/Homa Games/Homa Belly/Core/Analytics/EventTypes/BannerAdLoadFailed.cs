namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class BannerAdLoadFailed : BannerAdEvent
    {
        public BannerAdLoadFailed(string impressionId, AdPlacementType adPlacementType) : base(impressionId, adPlacementType)
        {
        }

        public override AnalyticsEventValue ToGameAnalyticsFormat()
        {
            return new AnalyticsEventValue($"Banners:LoadFailed:{AdPlacementType}");
        }
    }
}