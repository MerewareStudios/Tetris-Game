using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace HomaGames.HomaBelly.DataPrivacy
{
    public class DataPrivacySceneSetter : IPreprocessBuildWithReport
    {
        private const string MainScenePath = "Assets/Homa Games/Homa Belly/Core/DataPrivacy/Runtime/Scenes/Homa Games DataPrivacy Scene.unity";

        public static int CallbackOrder => 0;

        public int callbackOrder => CallbackOrder;

        public void OnPreprocessBuild(BuildReport report)
        {
            AddSceneToBuildSettings();
        }

        private static void AddSceneToBuildSettings()
        {
            var buildScenes = EditorBuildSettings.scenes;
            if (! NeedToAddSceneToBuildSettings(buildScenes))
                return;
            
            EditorBuildSettings.scenes = buildScenes
                .Where(s => s.path != MainScenePath)
                .Prepend(new EditorBuildSettingsScene(MainScenePath, true))
                .ToArray();
        }

        private static bool NeedToAddSceneToBuildSettings(EditorBuildSettingsScene[] buildSettingsScenes)
        {
            return buildSettingsScenes.Length <= 0
                   || buildSettingsScenes[0].path != MainScenePath
                   || ! buildSettingsScenes[0].enabled;
        }
    }
}