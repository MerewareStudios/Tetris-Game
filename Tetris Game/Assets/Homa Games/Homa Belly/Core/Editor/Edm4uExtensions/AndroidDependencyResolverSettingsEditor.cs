using UnityEditor;

namespace HomaGames.Edm4uExtensions
{
    [CustomEditor(typeof(AndroidDependencyResolverSettings))]
    internal class AndroidDependencyResolverSettingsEditor : Editor
    {
        private SerializedProperty _dependencyResolutionSolutionProp;

        private void OnEnable()
        {
            _dependencyResolutionSolutionProp =
                serializedObject.FindProperty(nameof(AndroidDependencyResolverSettings.dependencyResolutionSolution));
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(_dependencyResolutionSolutionProp);

            CustomGradleTemplatesHelpBox();
            if (serializedObject.hasModifiedProperties)
            {
                ApplyModifiedSettings();
            }
        }

        private void ApplyModifiedSettings()
        {
            var newFlagValue = (AndroidDependencyResolutionSolution)_dependencyResolutionSolutionProp.intValue;
            EditorApplication.delayCall += () => GradleConfigManager.EnableBuildPipeline(newFlagValue);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private void CustomGradleTemplatesHelpBox()
        {
            if (_dependencyResolutionSolutionProp.intValue !=
                (int)AndroidDependencyResolutionSolution.PatchGradleTemplates)
                return;
            // HelpBox to allow users easily check the path correctness
            if (GradleConfigManager.IsValidCurrentGradleTemplatesPath())
            {
                // Templates detected
                EditorGUILayout.HelpBox("Gradle Templates detected", MessageType.Info);
            }
            else
            {
                // Templates not found
                EditorGUILayout.HelpBox(
                    "Missing Gradle Templates. Please make sure to install Android Module along with Unity",
                    MessageType.Error);
            }
        }
    }
}