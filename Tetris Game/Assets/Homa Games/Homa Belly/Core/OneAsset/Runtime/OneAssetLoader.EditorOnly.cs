using System;
using System.IO;
using System.Linq;
using Object = UnityEngine.Object;


namespace HomaGames.OneAsset
{
    // Ideally this code would be in editor assembly. But these methods need to be accessible from InitializeOnLoad
    // there is no guarantee that editor callback will be registered like with `CreateAssetAction`
    // could use reflection
    internal static partial class OneAssetLoader
    {
        private static bool TryLoadFromAssetDatabase(Type type, AssetPath path, out Object obj)
        {
#if UNITY_EDITOR
            obj = UnityEditor.AssetDatabase.LoadAssetAtPath(path.OriginalPath, type);
            return obj != null;
#else
            obj = null;
            return false;
#endif
        }

        private static bool TryLoadAndForget(Type type, AssetPath path, out Object obj)
        {
#if UNITY_EDITOR
            if (!File.Exists(path.OriginalPath))
            {
                obj = null;
                return false;
            }
            
            obj = UnityEditorInternal.InternalEditorUtility
                .LoadSerializedFileAndForget(path.OriginalPath)
                .FirstOrDefault(o => o.GetType() == type);

            return obj != null;
#else
            obj = null;
            return false;
#endif
        }
    }
}