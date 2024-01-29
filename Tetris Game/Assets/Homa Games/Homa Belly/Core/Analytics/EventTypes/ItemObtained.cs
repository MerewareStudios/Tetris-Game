namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class ItemObtained : ItemEvent
    {   
        public ItemFlowReason Reason { get; }
        public string Reference { get; }
        
        public ItemObtained(string itemId, int itemLevel, ItemFlowReason reason, string reference = "") : base(itemId, itemLevel)
        {
            this.Reason = reason;
            this.Reference = reference;
        }

        public override void TrackThroughIAnalytics(IAnalytics implementer, bool isForwardingToHomaAnalytics)
        {
            implementer.TrackResourceEvent(ResourceFlowType.Source, "item", 0f, null, ItemId);
        }
    }
}