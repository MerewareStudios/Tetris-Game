namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class ItemUpgraded : ItemEvent
    {
        public ItemFlowReason Reason { get; }
        public ItemUpgradeType ItemType { get; }
        public string Reference { get; }
        
        public ItemUpgraded(string itemId, int itemLevel, ItemUpgradeType upgradeType, ItemFlowReason reason, string reference = "") : base(itemId, itemLevel)
        {
            this.Reason = reason;
            this.Reference = reference;
            this.ItemType = upgradeType;
        }

        public override AnalyticsEventValue ToGameAnalyticsFormat()
        {
            return new AnalyticsEventValue($"Item:Upgraded:{ItemId}:{ItemLevel}");
        }
    }
}