using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToggleButton : Toggle
{
    [SerializeField] private Animator toggleAnimator;

    public new void SetIsOnWithoutNotify(bool value)
    {
        base.SetIsOnWithoutNotify(value);
        toggleAnimator.SetTrigger(isOn ? "Selected" : "Disabled");
    }    
    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        toggleAnimator.SetTrigger(isOn ? "Selected" : "Disabled");
    }
}


#if UNITY_EDITOR
namespace  Game.Editor
{
    using UnityEditor;

    [CustomEditor(typeof(ToggleButton))]
    [CanEditMultipleObjects]
    public class ToggleButtonEditor : UnityEditor.UI.ToggleEditor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("toggleAnimator"), new GUIContent("Animator"));
            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
}
#endif