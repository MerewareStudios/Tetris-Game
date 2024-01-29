namespace HomaGames.HomaBelly.Internal.Analytics
{
    public abstract class RewardedAdEvent : AdEvent
    {
        public string RewardedAdName { get; }
        public int LevelId { get; }

        protected RewardedAdEvent(string adName, string impressionId, int levelId, AdPlacementType adPlacementType) 
            : base(impressionId, adPlacementType)
        {
            RewardedAdName = adName;
            LevelId = levelId;
        }
    }
}