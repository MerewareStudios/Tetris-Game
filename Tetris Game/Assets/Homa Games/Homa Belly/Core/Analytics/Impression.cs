using System;

namespace HomaGames.HomaBelly.Internal.Analytics
{
    public class Impression
    {
        public string PlacementName { get; }
        public string ImpressionId { get; }
        public AdPlacementType ADPlacementType { get; }

        public Impression(string placementName, AdPlacementType adPlacementType)
        {
            this.ADPlacementType = adPlacementType;
            this.PlacementName = string.IsNullOrEmpty(placementName) ? "Default" : placementName;
            this.ImpressionId = GenerateNewImpressionId();
        }

        private static string GenerateNewImpressionId()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}