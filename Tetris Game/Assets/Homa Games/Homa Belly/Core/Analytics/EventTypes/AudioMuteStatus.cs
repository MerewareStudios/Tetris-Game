namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class AudioMuteStatus : AnalyticsEvent
    {
        public string MuteStatus { get; }
        public AudioMuteStatus(bool muteStatus) : base(HomaGames.HomaBelly.EventCategory.system_event.ToString())
        {
            MuteStatus = muteStatus ? "muted" : "unmuted";
        }

        public override AnalyticsEventValue ToGameAnalyticsFormat()
        {
            return new AnalyticsEventValue("Audio:" + (MuteStatus == "muted" ? "Muted" : "Unmuted"));
        }
    }
}