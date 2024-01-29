namespace HomaGames.HomaBelly.Internal.Analytics
{
    public abstract class ResourceEvent : AnalyticsEvent 
    {
        public string Currency { get; }
        
        public float FinalAmount { get; }

        protected ResourceEvent(string currency, float finalAmount) : base(HomaGames.HomaBelly.EventCategory.resource_event.ToString())
        {
            this.Currency = currency;
            this.FinalAmount = finalAmount;
        }
    }
}