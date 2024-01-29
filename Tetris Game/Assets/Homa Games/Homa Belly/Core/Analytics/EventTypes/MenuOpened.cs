namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class MenuOpened : SessionEvent
    {
        public string MenuName { get; }
        public MenuOpened(string menuName)
        {
            MenuName = menuName;
        }

        public override AnalyticsEventValue ToGameAnalyticsFormat()
        {
            return new AnalyticsEventValue($"Menu:{MenuName}:Opened");
        }
    }
}