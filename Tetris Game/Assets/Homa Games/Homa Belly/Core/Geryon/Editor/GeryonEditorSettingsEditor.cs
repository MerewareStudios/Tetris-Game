using System.Diagnostics;
using HomaGames.HomaBelly;
using UnityEditor;
using UnityEngine;

namespace HomaGames.Geryon.Editor
{
    [CustomEditor(typeof(GeryonEditorSettings))]
    internal class GeryonEditorSettingsEditor : UnityEditor.Editor
    {
        private const string DevOnlySymbol = "HOMA_DEV_ENV";

        private static readonly GUIContent UpdateDvrScriptButtonContent = new GUIContent("Update Script",
            "Pull latest N-Testing configuration form HOMA Lab");

        private static bool IsScriptUpdateInProgress => Progress.Exists(DvrScriptManager.UpdateProcessProgressId);

        private MonoScript _dvrScript;
        private SerializedProperty _developerModeSettingsProperty;

        private void OnEnable()
        {
            if (Database.TryGetDvrAssetPath(out var path))
                _dvrScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            _developerModeSettingsProperty =
                serializedObject.FindProperty(nameof(GeryonEditorSettings.developerSettings));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            DrawDvrScriptVersionField();
            using (new GUILayout.HorizontalScope())
            {
                DrawDvrScriptField();
                using (new EditorGUI.DisabledScope(IsScriptUpdateInProgress))
                {
                    if (GUILayout.Button(UpdateDvrScriptButtonContent, GUILayout.Width(110)))
                        UpdateDvrScript(DvrScriptManager.GetDvrScriptVersionForAutomaticUpdate());
                }
            }

            DrawDeveloperModeSettings();

            serializedObject.ApplyModifiedProperties();
        }

        [Conditional(DevOnlySymbol)]
        private void DrawDeveloperModeSettings()
        {
            EditorGUILayout.PropertyField(_developerModeSettingsProperty, true);
        }

        private void DrawDvrScriptField()
        {
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField(new GUIContent("DVR Script"), _dvrScript, typeof(MonoScript), false);
            }
        }

        private void DrawDvrScriptVersionField()
        {
            using (new EditorGUI.DisabledScope(IsScriptUpdateInProgress))
            {
                var current = DvrScriptManager.GetDvrScriptVersionForAutomaticUpdate();
                var selected = (DvrScriptVersion)EditorGUILayout
                    .EnumPopup(new GUIContent("DVR Script Version"), current);
                if (current != selected)
                    UpdateDvrScript(selected);
            }
        }

        private void UpdateDvrScript(DvrScriptVersion version)
        {
            DvrScriptManager.CreateOrUpdateDvrFileAsync(version, true)
                .ListenForErrors();
        }
    }
}