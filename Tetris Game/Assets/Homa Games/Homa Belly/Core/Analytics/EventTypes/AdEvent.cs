namespace HomaGames.HomaBelly.Internal.Analytics
{
    public abstract class AdEvent : AnalyticsEvent
    {
        public string ImpressionId { get; }
        public AdPlacementType AdPlacementType { get; }

        protected AdEvent(string impressionId, AdPlacementType adPlacementType) : base(HomaGames.HomaBelly.EventCategory.ad_event.ToString())
        {
            ImpressionId = impressionId;
            AdPlacementType = adPlacementType;
        }

    }
}