using UnityEngine;

namespace HomaGames.Edm4uExtensions
{
    internal enum AndroidDependencyResolutionSolution
    {
        [InspectorName("Import Dependencies To Project. Slow and unreliable")]
        ImportDependenciesToProject = 1,
        [InspectorName("[Recommended] Patch Gradle Template")]
        PatchGradleTemplates = 2,
    };
}