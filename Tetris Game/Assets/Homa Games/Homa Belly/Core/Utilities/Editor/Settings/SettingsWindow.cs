using UnityEditor;

namespace HomaGames.HomaBelly
{
    internal static class SettingsWindow
    {
        [MenuItem("Window/Homa Games/Homa Belly/Settings", false, 12)]
        private static void OpenSettings() => Settings.OpenWindow();
    }
}