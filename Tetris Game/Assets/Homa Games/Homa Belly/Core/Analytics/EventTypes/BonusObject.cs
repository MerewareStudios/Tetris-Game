namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class BonusObject : ProgressionEvent
    {
        public string BonusObjectType { get; }
        public string BonusObjectName { get; }
        public string Reward { get; }
        public BonusObject(BonusObjectType bonusObjectType, string bonusObjectName, string reward, int levelId) : base(levelId)
        {
            BonusObjectType = ToUnderscoreCase(bonusObjectType.ToString());
            BonusObjectName = bonusObjectName;
            Reward = reward;
        }

        public override AnalyticsEventValue ToGameAnalyticsFormat()
        {
            return new AnalyticsEventValue($"Bonus:{BonusObjectType}:{BonusObjectName}:Level{LevelId}");
        }
    }
}