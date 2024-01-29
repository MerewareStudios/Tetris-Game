using UnityEngine;

namespace HomaGames.Geryon.Editor
{
    // Sorted from highest to lowest for UI dropdown display order
    public enum DvrScriptVersion
    {
        [InspectorName("2.0")]
        Observables = 2,

        [InspectorName("1.0 (Legacy)")]
        DynamicVariable = 1,

        [InspectorName("")]
        Unknown = 0
    }
}