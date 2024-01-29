namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class BannerAdExpanded : BannerAdEvent
    {
        public BannerAdExpanded(string impressionId, AdPlacementType adPlacementType) : base(impressionId, adPlacementType)
        {
        }

        public override AnalyticsEventValue ToGameAnalyticsFormat()
        {
            return new AnalyticsEventValue($"Banners:Expanded:{AdPlacementType}");
        }
    }
}