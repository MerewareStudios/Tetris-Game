using System.IO;
using Game;
using Internal.Core;
using UnityEditor;
using UnityEngine;

public class SaveManagerBase<T> : Singleton<T> where T : MonoBehaviour
{
    [SerializeField] public SaveData saveData;
    [SerializeField] public bool DELETE_AT_START;
    
    private static string SavePath => Path.Combine(Application.persistentDataPath, "Data.json");

    public virtual void Awake()
    {
        if (DELETE_AT_START)
        {
            Delete();
        }
        Load();
    }

    public void Save()
    {
        Save(saveData);
    }

    public static void Save(SaveData data)
    {
        Save(JsonUtility.ToJson(data));
    }
    public static void Save(string content)
    {
        if (!string.IsNullOrEmpty(content))
        {
            File.WriteAllText(SavePath, content);
        }
    }

    public static void CreateSavePoint(string prefix)
    {
        SaveSO saveSo = ScriptableObject.CreateInstance<SaveSO>();
        saveSo.saveData = SaveManager.THIS.saveData.Clone() as SaveData;

        string path = Path.Combine("Assets", "Game", "Managers", "Save SO");
        string subPath = Path.Combine(path, saveSo.saveData.accountData.guid);

        if (!AssetDatabase.IsValidFolder(subPath))
        {
            Debug.Log("create folder");
            AssetDatabase.CreateFolder(path, saveSo.saveData.accountData.guid);
            AssetDatabase.SaveAssets();
        }
        
        AssetDatabase.CreateAsset(saveSo, Path.Combine(path, saveSo.saveData.accountData.guid, prefix + ".asset"));
        AssetDatabase.SaveAssets();
    }

    private void Load()
    {
        saveData = null;
        if (!File.Exists(SavePath))
        {
            return;
        }
        string inputString = File.ReadAllText(SavePath);

        if (string.IsNullOrEmpty(inputString))
        {
            return;
        }
        saveData = JsonUtility.FromJson<SaveData>(inputString);
    }

    public static void Delete()
    {
        if (!File.Exists(SavePath))
        {
            return;
        }
        File.Delete(SavePath);
        Debug.LogWarning("Save file deleted.");
    }

#if UNITY_EDITOR
    private void OnApplicationQuit()
    {
        Save();
    }
#else
    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            Save();
        }
    }
#endif
}

[System.Serializable]
public partial class SaveData : System.ICloneable
{
    // [SerializeField] public string baseInfo = "SAVE END";
}

#if UNITY_EDITOR
namespace  Game.Editor
{
    using UnityEditor;

    [CustomEditor(typeof(SaveManagerBase<>), true)]
    [CanEditMultipleObjects]
    public class SaveManagerBaseGUI : Editor
    {
        public override void OnInspectorGUI()
        {
            if (Application.isPlaying && GUILayout.Button(new GUIContent("Create Save Point", "Create save point.")))
            {
                SaveManager.CreateSavePoint("Manual");
            }
            if (!Application.isPlaying && GUILayout.Button(new GUIContent("Delete Save", "Delete save file.")))
            {
                SaveManager.Delete();
            }
            DrawDefaultInspector();
        }
    }
}
#endif