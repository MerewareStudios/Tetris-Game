#if UNITY_EDITOR
using Lean.Pool;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEditor.SearchService;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Internal.Core
{
    public static class AutoGenerate
    {
        #region Editor
        [MenuItem("AUTO GENERATE/All")]
        private static void GenerateAll()
        {
            GenerateLayers();
            GeneratePool();
            GenerateParticles();
        }
        [MenuItem("AUTO GENERATE/Layers")]
        private static void GenerateLayers()
        {
            string declerations = "";
            foreach (var layerName in InternalEditorUtility.layers)
            {
                string layerDecleration = GetDecleration(AccessibilityLevel.PUBLIC, StaticModifier.NONSTATIC, ReadModifier.CONSTANT, VariableType.INTEGER, layerName.Replace(" ", "").Replace("/", "_"), LayerMask.NameToLayer(layerName).ToString(), LineEnding.SEMICOLON);
                declerations += layerDecleration + "\n";
            }
            GenerateClass(StaticModifier.STATIC, "Layers", declerations);
        }
        [MenuItem("AUTO GENERATE/Pool")]
        public static void GeneratePool()
        {
            string declerations = "";
            for (int i = 0; i < PoolManager.THIS.pools.Count; i++)
            {
                PoolManager.PoolData poolData = PoolManager.THIS.pools[i];
                string poolDecleration = GetDecleration(AccessibilityLevel.NONE, StaticModifier.NONSTATIC, ReadModifier.NONE, VariableType.NONE, poolData.gameObject.name.Replace(" ", "_").Replace("-", "_"), i.ToString(), LineEnding.COMMA);
                declerations += poolDecleration + "\n";
            }
            GenerateEnum("Pool", declerations);
        }
        [MenuItem("AUTO GENERATE/Particles")]
        public static void GenerateParticles()
        {
            string declerations = "";
            for (int i = 0; i < ParticleManager.THIS.particleSystems.Count; i++)
            {
                ParticleSystem ps = ParticleManager.THIS.particleSystems[i];
                string poolDecleration = GetDecleration(AccessibilityLevel.NONE, StaticModifier.NONSTATIC, ReadModifier.NONE, VariableType.NONE, ps.name.Replace(" ", "_"), i.ToString(), LineEnding.COMMA);
                declerations += poolDecleration + "\n";
            }
            GenerateEnum("Particle", declerations);
        }
        [MenuItem("MANAGERS/Application Manager/Ping")]
        private static void PingApplicationManager()
        {
            EditorGUIUtility.PingObject(MonoBehaviour.FindObjectOfType<ApplicationManager>().gameObject);
        }
        [MenuItem("MANAGERS/Game Manager/Ping")]
        private static void PingGameManager()
        {
            EditorGUIUtility.PingObject(MonoBehaviour.FindObjectOfType<GameManager>().gameObject);
        }
        [MenuItem("MANAGERS/Save Manager/Ping")]
        private static void PingSaveManager()
        {
            EditorGUIUtility.PingObject(MonoBehaviour.FindObjectOfType<SaveManager>().gameObject);
        }
        [MenuItem("MANAGERS/Level Manager/Ping")]
        private static void PingLevelManager()
        {
            EditorGUIUtility.PingObject(MonoBehaviour.FindObjectOfType<LevelManager>().gameObject);
        }
        [MenuItem("MANAGERS/Camera Manager/Ping")]
        private static void PingCameraManager()
        {
            EditorGUIUtility.PingObject(MonoBehaviour.FindObjectOfType<CameraManager>().gameObject);
        }
        [MenuItem("MANAGERS/UI Manager/Ping")]
        private static void PingUIManager()
        {
            EditorGUIUtility.PingObject(MonoBehaviour.FindObjectOfType<UIManager>().gameObject);
        }
        [MenuItem("MANAGERS/Particle Manager/Ping")]
        private static void PingParticleManager()
        {
            EditorGUIUtility.PingObject(MonoBehaviour.FindObjectOfType<ParticleManager>().gameObject);
        }
        [MenuItem("MANAGERS/Pool Manager/Ping")]
        private static void PingPoolManager()
        {
            EditorGUIUtility.PingObject(MonoBehaviour.FindObjectOfType<PoolManager>().gameObject);
        }
        [MenuItem("MANAGERS/Input Manager/Ping")]
        private static void PingInputManager()
        {
            EditorGUIUtility.PingObject(MonoBehaviour.FindObjectOfType<InputManager>().gameObject);
        }
        [MenuItem("MANAGERS/Tick Manager/Ping")]
        private static void PingTickManager()
        {
            EditorGUIUtility.PingObject(MonoBehaviour.FindObjectOfType<TickManager>().gameObject);
        }
        // [MenuItem("MANAGERS/Event Manager/Ping")]
        // private static void PingEventManager()
        // {
        //     EditorGUIUtility.PingObject(MonoBehaviour.FindObjectOfType<EventManager>().gameObject);
        // }
        [MenuItem("MANAGERS/Application Manager/Open")]
        private static void OpenApplicationManager()
        {
            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal("Assets/Game/Managers/ApplicationManager.cs", 1);
        }
        [MenuItem("MANAGERS/Game Manager/Open")]
        private static void OpenGameManager()
        {
            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal("Assets/Game/Managers/GameManager.cs", 1);
        }
        [MenuItem("MANAGERS/Save Manager/Open")]
        private static void OpenSaveManager()
        {
            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal("Assets/Game/Managers/SaveManager.cs", 1);
        }
        [MenuItem("MANAGERS/Level Manager/Open")]
        private static void OpenLevelManager()
        {
            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal("Assets/Game/Managers/LevelManager.cs", 1);
        }
        [MenuItem("MANAGERS/Camera Manager/Open")]
        private static void OpenCameraManager()
        {
            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal("Assets/Game/Managers/CameraManager.cs", 1);
        }
        [MenuItem("MANAGERS/UI Manager/Open")]
        private static void OpenUIManager()
        {
            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal("Assets/Game/Managers/UIManager.cs", 1);
        }
        [MenuItem("MANAGERS/Particle Manager/Open")]
        private static void OpenParticleManager()
        {
            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal("Assets/Game/Managers/ParticleManager.cs", 1);
        }
        [MenuItem("MANAGERS/Pool Manager/Open")]
        private static void OpenPoolManager()
        {
            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal("Assets/Game/Managers/PoolManager.cs", 1);
        }
        [MenuItem("MANAGERS/Input Manager/Open")]
        private static void OpenInputManager()
        {
            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal("Assets/Game/Managers/InputManager.cs", 1);
        }
        [MenuItem("MANAGERS/Tick Manager/Open")]
        private static void OpenTickManager()
        {
            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal("Assets/Game/Managers/TickManager.cs", 1);
        }
        // [MenuItem("MANAGERS/Event Manager/Open")]
        // private static void OpenEventManager()
        // {
        //     UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal("Assets/Game/Managers/EventManager.cs", 1);
        // }
        [MenuItem("PREFS/Clear Player Prefs")]
        private static void ClearPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
        }
        // [MenuItem("PREFS/Clear Editor Prefs")]
        // private static void ClearEditorPrefs()
        // {
        //     EditorPrefs.DeleteAll();
        // }

        [MenuItem("BUILD/APK", priority = 9)]
        private static void BuildAPK()
        {

        }
        [MenuItem("BUILD/AAB", priority = 10)]
        private static void BuildAAB()
        {

        }


        [MenuItem("Assets/Create/IWI Package Hierarchy", priority = 0)]
        private static void AddIWIPackageHierarchy()
        {
            var path = "";
            var obj = Selection.activeObject;
            if (obj == null) path = "Assets";
            else path = AssetDatabase.GetAssetPath(obj.GetInstanceID());
            if (path.Length > 0)
            {
                if (Directory.Exists(path))
                {
                    Directory.CreateDirectory(path + "/+Document README+");
                    Directory.CreateDirectory(path + "/Editor");
                    Directory.CreateDirectory(path + "/Demo");
                    Directory.CreateDirectory(path + "/Scripts");
                    Directory.CreateDirectory(path + "/Textures");
                    Directory.CreateDirectory(path + "/Materials");
                    Directory.CreateDirectory(path + "/Prefabs");
                    Directory.CreateDirectory(path + "/Shaders");
                    Directory.CreateDirectory(path + "/Models");


                    AssetDatabase.Refresh();
                }
                else
                {
                    Debug.Log("File Not Supported");
                }
            }
            else
            {
                Debug.Log("Not in assets folder");
            }
        }
        
        [MenuItem("Assets/Create/Package Hierarchy", priority = 0)]
        private static void AddPackageHierarchy()
        {
            var path = "";
            var obj = Selection.activeObject;
            if (obj == null) path = "Assets";
            else path = AssetDatabase.GetAssetPath(obj.GetInstanceID());
            if (path.Length > 0)
            {
                if (Directory.Exists(path))
                {
                    Directory.CreateDirectory(path + "/Editor");
                    Directory.CreateDirectory(path + "/Runtime");
                    Directory.CreateDirectory(path + "/Runtime/Scripts");
                    Directory.CreateDirectory(path + "/Runtime/Textures");
                    Directory.CreateDirectory(path + "/Runtime/Materials");
                    Directory.CreateDirectory(path + "/Runtime/Prefabs");
                    Directory.CreateDirectory(path + "/Runtime/Scene");
                    Directory.CreateDirectory(path + "/Runtime/Shaders");
                    Directory.CreateDirectory(path + "/Tests");


                    AssetDatabase.Refresh();
                }
                else
                {
                    Debug.Log("File Not Supported");
                }
            }
            else
            {
                Debug.Log("Not in assets folder");
            }
        }
        public static void PingAsset<T>(string path)
        {
            Object @object = AssetDatabase.LoadAssetAtPath(path, typeof(T));
            Selection.activeObject = @object;
            EditorGUIUtility.PingObject(@object);
        }

        [MenuItem("URP/Renderers")]
        private static void PingURPSettings()
        {
            PingAsset<Object>("Assets/URP Settings");
        }
        [MenuItem("EDITOR/EditorGUI")]
        private static void PingEditorGUI()
        {
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath("Assets/Editor/Custom GUI Data.asset", typeof(ScriptableObject)));
        }
        [MenuItem("SCRIPTABLES/Constants")]
        private static void PingGameConstantsManager()
        {
            PingAsset<ScriptableObject>("Assets/Resources/Game Constants.asset");
        }
        [MenuItem("SCRIPTABLES/Onboarding")]
        private static void PingOnboardingSO()
        {
            PingAsset<ScriptableObject>("Assets/Resources/Onboarding.asset");
        }
        [MenuItem("Assets/Create/Txt", priority = 1)]
        private static void CreateEmptyTxt()
        {
            var path = "";
            var obj = Selection.activeObject;
            if (obj == null) path = "Assets";
            else path = AssetDatabase.GetAssetPath(obj.GetInstanceID());
            if (path.Length > 0)
            {
                if (Directory.Exists(path))
                {
                    File.WriteAllText(path + "/Sample.txt", "");

                    AssetDatabase.Refresh();
                }
                else
                {
                    Debug.Log("File Not Supported");
                }
            }
            else
            {
                Debug.Log("Not in assets folder");
            }
        }
        [MenuItem("PROTOTYPE/Joystick/Ping")]
        private static void PingJoystick()
        {
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath("Assets/Game/Prefabs/Joystick.prefab", typeof(GameObject)));
        }

        [UnityEditor.MenuItem("PROTOTYPE/Joystick/Add")]
        [UnityEditor.MenuItem("GameObject/Prototype/Variable Joystick", false, 1)]
        private static void AddJoystick()
        {
            Object joystick = AssetDatabase.LoadAssetAtPath("Assets/Game/Prefabs/Joystick.prefab", typeof(Object));
            GameObject instance = PrefabUtility.InstantiatePrefab(joystick) as GameObject;
            instance.transform.SetParent(UnityEditor.Selection.activeGameObject == null ? null : UnityEditor.Selection.activeGameObject.transform);
            instance.transform.SetAsLastSibling();

            UnityEditor.Undo.RegisterCreatedObjectUndo(instance, "Create Variable Joystick");

            UnityEditor.Selection.activeGameObject = instance;
        }

        private const string RELOAD_DOMAIN_AND_SCENE = "EDITOR/Reload Domain-Scene";
        [MenuItem(RELOAD_DOMAIN_AND_SCENE)]
        private static void ReloadDomain()
        {
            bool state = !EditorPrefs.GetBool(RELOAD_DOMAIN_AND_SCENE);
            EditorPrefs.SetBool(RELOAD_DOMAIN_AND_SCENE, state);
            Menu.SetChecked(RELOAD_DOMAIN_AND_SCENE, state);
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = state ? EnterPlayModeOptions.None : (EnterPlayModeOptions.DisableSceneReload | EnterPlayModeOptions.DisableDomainReload);
        }
        public const string CLEAR_SAVE_ON_AWAKE = "EDITOR/Clear Save On Start";
        [MenuItem(CLEAR_SAVE_ON_AWAKE)]
        private static void ClearSaveOnStart()
        {
            SaveManager.THIS.DELETE_AT_START = !SaveManager.THIS.DELETE_AT_START;
            Menu.SetChecked(CLEAR_SAVE_ON_AWAKE, SaveManager.THIS.DELETE_AT_START);
        }
        public const string SKIP_ONBOARDING = "EDITOR/Skip Onboarding";
        [MenuItem(SKIP_ONBOARDING)]
        private static void SkipOnboarding()
        {
            SaveManager.THIS.SKIP_ONBOARDING = !SaveManager.THIS.SKIP_ONBOARDING;
            Menu.SetChecked(SKIP_ONBOARDING, SaveManager.THIS.SKIP_ONBOARDING);
        }

        #endregion
        #region ClassGen
        public static void GenerateClass(StaticModifier staticMod, string className, string varDec)
        {
            string content = File.ReadAllText("Assets/Auto Generated/Templates/ClassTemplate.txt");
            content = content.Replace("$" + nameof(staticMod), staticMod.Str());
            content = content.Replace("$" + nameof(className), className);
            content = content.Replace("$" + nameof(varDec), varDec);
            File.WriteAllText("Assets/Auto Generated/Generated/" + className + ".cs", content);
            AssetDatabase.Refresh();
        }
        public static void GenerateEnum(string name, string varDec)
        {
            string content = File.ReadAllText("Assets/Auto Generated/Templates/EnumTemplate.txt");
            content = content.Replace("$" + nameof(name), name);
            content = content.Replace("$" + nameof(varDec), varDec);
            File.WriteAllText("Assets/Auto Generated/Generated/" + name + ".cs", content);
            AssetDatabase.Refresh();
        }
        #endregion
        #region Decleration
        public static string GetDecleration(AccessibilityLevel accessibilityLevel, StaticModifier staticModifier, ReadModifier readModifier, VariableType variableType, string variableName, string defaultValue, LineEnding lineEnding)
        {
            return accessibilityLevel.Str() + " " + staticModifier.Str() + " " + readModifier.Str() + " " + variableType.Str() + " " + variableName + " = " + defaultValue + lineEnding.Str();
        }
        public enum AccessibilityLevel
        {
            NONE,
            PUBLIC,
            PRIVATE,
            PROTECTED,
            INTERNAL,
        }
        public enum StaticModifier
        {
            STATIC,
            NONSTATIC,
        }
        public enum ReadModifier
        {
            NONE,
            READONLY,
            CONSTANT,
        }
        public enum VariableType
        {
            NONE,
            VOID,
            INTEGER,
            FLOAT,
        }
        public enum LineEnding
        {
            SEMICOLON,
            COMMA,
            EMPTY,
        }
        public static string Str(this AccessibilityLevel accessibilityLevel)
        {
            switch (accessibilityLevel)
            {
                case AccessibilityLevel.NONE:
                    return "";
                case AccessibilityLevel.PUBLIC:
                    return "public";
                case AccessibilityLevel.PRIVATE:
                    return "private";
                case AccessibilityLevel.PROTECTED:
                    return "protect";
                case AccessibilityLevel.INTERNAL:
                    return "internal";
            }
            return "";
        }
        public static string Str(this StaticModifier staticModifier)
        {
            switch (staticModifier)
            {
                case StaticModifier.STATIC:
                    return "static";
                case StaticModifier.NONSTATIC:
                    return "";
            }
            return "";
        }
        public static string Str(this ReadModifier readModifier)
        {
            switch (readModifier)
            {
                case ReadModifier.NONE:
                    return "";
                case ReadModifier.READONLY:
                    return "readonly";
                case ReadModifier.CONSTANT:
                    return "const";
            }
            return "";
        }
        public static string Str(this VariableType variableType)
        {
            switch (variableType)
            {
                case VariableType.NONE:
                    return "";
                case VariableType.VOID:
                    return "void";
                case VariableType.INTEGER:
                    return "int";
                case VariableType.FLOAT:
                    return "float";
            }
            return "";
        }
        public static string Str(this LineEnding lineEnding)
        {
            switch (lineEnding)
            {
                case LineEnding.EMPTY:
                    return "";
                case LineEnding.SEMICOLON:
                    return ";";
                case LineEnding.COMMA:
                    return ",";
            }
            return "";
        }
        #endregion
    }

}
#endif