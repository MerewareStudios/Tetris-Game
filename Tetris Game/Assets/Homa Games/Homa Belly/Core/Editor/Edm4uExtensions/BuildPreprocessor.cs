using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace HomaGames.Edm4uExtensions
{
    internal class BuildPreprocessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 100;

        public void OnPreprocessBuild(BuildReport report)
        {
            // Safety check. We do not want to act in batch mode builds
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android ||
                AndroidDependencyResolverSettings.Instance == null || 
                Application.isBatchMode)
                return;

            if ((AndroidDependencyResolutionSolution)AndroidDependencyResolverSettings.Instance.dependencyResolutionSolution ==
                AndroidDependencyResolutionSolution.PatchGradleTemplates)
            {
                // Before building, if Custom Gradle template is selected but files do not exist:
                // create them
                if (!GradleConfigManager.IsMainGradleTemplateEnabled() || !GradleConfigManager.IsGradleTemplatePropertiesEnabled())
                    GradleConfigManager.EnableCustomGradleBuildPipeline();
            }
        }
    }
}