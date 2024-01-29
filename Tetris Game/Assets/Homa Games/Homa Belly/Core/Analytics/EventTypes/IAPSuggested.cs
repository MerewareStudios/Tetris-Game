namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class IAPSuggested : IAPEvent
    {
        public string PopupName { get; }
        public string ProductId { get; }
        public IAPSuggested(string popupName, string productId = null)
        {
            PopupName = popupName;
            ProductId = productId;
        }

        public override AnalyticsEventValue ToGameAnalyticsFormat()
        {
            return new AnalyticsEventValue($"IAP:Suggested:{ProductId}");
        }
    }
}