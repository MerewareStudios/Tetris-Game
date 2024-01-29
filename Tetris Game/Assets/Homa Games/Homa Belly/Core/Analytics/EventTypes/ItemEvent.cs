namespace HomaGames.HomaBelly.Internal.Analytics
{
    public abstract class ItemEvent : AnalyticsEvent 
    {
        public string ItemId { get; }
        public int ItemLevel { get; }

        protected ItemEvent(string itemId, int itemLevel) : base(HomaGames.HomaBelly.EventCategory.item_event.ToString())
        {
            this.ItemId = itemId;
            this.ItemLevel = itemLevel;
        }
    }
}