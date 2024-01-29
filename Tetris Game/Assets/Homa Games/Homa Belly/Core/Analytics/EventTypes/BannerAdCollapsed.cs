namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class BannerAdCollapsed : BannerAdEvent
    {
        public BannerAdCollapsed(string impressionId, AdPlacementType adPlacementType) : base(impressionId, adPlacementType)
        {
        }

        public override AnalyticsEventValue ToGameAnalyticsFormat()
        {
            return new AnalyticsEventValue($"Banners:Collapsed:{AdPlacementType}");
        }
    }
}