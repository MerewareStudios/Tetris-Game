using HomaGames.OneAsset;
using UnityEngine;

namespace HomaGames.Geryon.Editor
{
    [LoadFromAsset(ProjectPath, CreateAssetIfMissing = true)]
    [SettingsProviderAsset("Project/HOMA/N Testing")]
    internal class GeryonEditorSettings : OneScriptableObject<GeryonEditorSettings>
    {
        private const string ProjectPath =
            "Assets/Homa Games/Settings/Editor/Resources/com.homagames.geryon/Geryon Editor Settings.asset";
        [SerializeField]
        internal DeveloperSettings developerSettings = new DeveloperSettings();
    }
}