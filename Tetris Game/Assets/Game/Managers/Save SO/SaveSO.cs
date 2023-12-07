using UnityEngine;

namespace Game
{
    [CreateAssetMenu(fileName = "Save Data", menuName = "Game/Save Data", order = 0)]
    public class SaveSO : ScriptableObject
    {
        [SerializeField] public SaveData saveData;
    }
    
    
}

#if UNITY_EDITOR
namespace  Game.Editor
{
    using UnityEditor;

    [CustomEditor(typeof(SaveSO), true)]
    [CanEditMultipleObjects]
    public class SaveSOGUI : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button(new GUIContent("USE", "Use save point. Disables DELETE_ON_START")))
            {
                SaveManager.Save(((SaveSO)target).saveData);
            }
            DrawDefaultInspector();
        }
    }
}
#endif