using System.Text;
using UnityEditor;
using UnityEngine;

namespace HomaGames.Edm4uExtensions
{
    internal static class ReportAction
    {
        [MenuItem("Window/Homa Games/Homa Belly/EDM4U Extensions/Report")]
        public static void CheckAndroidResolver()
        {
            var mainGradleTemplateEnabled = GradleConfigManager.IsMainGradleTemplateEnabled();
            var isGradleTemplatePropertiesEnabled = GradleConfigManager.IsGradleTemplatePropertiesEnabled();
            var gradleTemplateEnabled = GradleConfigManager.IsAndroidResolverSettingProperlyConfigured("gradleTemplateEnabled", "True");
            var patchMainTemplateGradleEnabled =
                GradleConfigManager.IsAndroidResolverSettingProperlyConfigured("patchMainTemplateGradle", "True");
            var gradlePropertiesTemplateEnabled =
                GradleConfigManager.IsAndroidResolverSettingProperlyConfigured("gradlePropertiesTemplateEnabled", "True");
            var useJetifierEnabled = GradleConfigManager.IsAndroidResolverSettingProperlyConfigured("useJetifier", "True");

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"[EDM4U Extensions] Build pipeline report");
            stringBuilder.AppendLine($"mainGradle.template enabled: {mainGradleTemplateEnabled}");
            stringBuilder.AppendLine($"gradleTemplate.properties enabled: {isGradleTemplatePropertiesEnabled}");
            stringBuilder.AppendLine($"External Dependency Manager: 'gradleTemplateEnabled' - {gradleTemplateEnabled}");
            stringBuilder.AppendLine(
                $"External Dependency Manager: 'patchMainTemplateGradle' - {patchMainTemplateGradleEnabled}");
            stringBuilder.AppendLine(
                $"External Dependency Manager: 'gradlePropertiesTemplateEnabled' - {gradlePropertiesTemplateEnabled}");
            stringBuilder.AppendLine($"External Dependency Manager: 'useJetifier' - {useJetifierEnabled}");
            Debug.Log(stringBuilder.ToString());
        }
    }
}