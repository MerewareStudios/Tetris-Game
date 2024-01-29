namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class ResourceFlow : ResourceEvent
    {
        public ResourceFlowType FlowType { get; }
        public ResourceFlowReason FlowReason { get; }
        public float FlowAmount { get; }
        public string Reference  { get; }
        public string ItemType  { get; }
        public string ItemId  { get; }

        public ResourceFlow(ResourceFlowType flowType, string currency, float flowAmount, float finalAmount, string itemType, string itemId, ResourceFlowReason flowReason, string reference = "")
            : base(currency, finalAmount)
        {
            this.FlowType = flowType;
            this.FlowAmount = flowAmount;
            this.FlowReason = flowReason;
            this.Reference = reference;
            this.ItemType = itemType;
            this.ItemId = itemId;
        }

        public override void TrackThroughIAnalytics(IAnalytics implementer, bool isForwardingToHomaAnalytics)
        {
            implementer.TrackResourceEvent(FlowType, Currency, FlowAmount, ItemType, ItemId);
        }
    }
}