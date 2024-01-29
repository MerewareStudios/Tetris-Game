using UnityEditor;
using UnityEngine;

namespace HomaGames.HomaBelly.DataPrivacy
{
    [CustomEditor(typeof(Settings))]
    public class SettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            using (var changeCheck = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(Settings.iOSIdfaPopupMessage)),
                    new GUIContent("[iOS only] Privacy Popup Message",
                        "Customizable popup message to be displayed on the native popup"));
                if (changeCheck.changed)
                    serializedObject.ApplyModifiedProperties();
            }
            ResetButton();
        }

        private void ResetButton()
        {
            if (GUILayout.Button("Reset"))
            {
                // Remove focus from input fields so that they can show their new content
                GUI.FocusControl("");
                Undo.RecordObject(target,"Reset Settings");
                Unsupported.SmartReset(target);
                EditorUtility.SetDirty(target);
                serializedObject.Update();
            }
        }
    }
}