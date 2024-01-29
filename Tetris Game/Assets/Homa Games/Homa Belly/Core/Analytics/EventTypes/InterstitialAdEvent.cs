namespace HomaGames.HomaBelly.Internal.Analytics
{
    public abstract class InterstitialAdEvent : AdEvent
    {
        public string InterstitialAdName { get; }
        public int LevelId { get; }

        protected InterstitialAdEvent(string adName, string impressionId, int levelId, AdPlacementType adPlacementType)
            : base(impressionId, adPlacementType)
        {
            InterstitialAdName = adName;
            LevelId = levelId;
        }
    }
}