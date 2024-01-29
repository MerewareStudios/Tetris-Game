namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class MenuClosed : SessionEvent
    {
        public string MenuName { get; }
        public MenuClosed(string menuName)
        {
            MenuName = menuName;
        }

        public override AnalyticsEventValue ToGameAnalyticsFormat()
        {
            return new AnalyticsEventValue($"Menu:{MenuName}:Closed");
        }
    }
}