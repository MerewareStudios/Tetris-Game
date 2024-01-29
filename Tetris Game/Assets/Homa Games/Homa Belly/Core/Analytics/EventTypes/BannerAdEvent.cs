namespace HomaGames.HomaBelly.Internal.Analytics
{
    public abstract class BannerAdEvent : AdEvent
    {
        protected BannerAdEvent(string impressionId, AdPlacementType adPlacementType) : base(impressionId, adPlacementType)
        {
        }
    }
}