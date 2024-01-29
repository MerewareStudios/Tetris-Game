namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class HomaBellyInitialized : AnalyticsEvent
    {
        public long TotalGameplaySeconds { get; }
        public HomaBellyInitialized(long totalGameplaySeconds) : base(HomaGames.HomaBelly.EventCategory.system_event.ToString())
        {
            TotalGameplaySeconds = totalGameplaySeconds;
        }

        public override AnalyticsEventValue ToGameAnalyticsFormat()
        {
            return new AnalyticsEventValue("HomaBelly_Initialized", UnityEngine.Mathf.Max(0, TotalGameplaySeconds));
        }
    }
}