using HomaGames.HomaBelly;
using UnityEditor;

namespace HomaGames.Geryon.Editor
{
    [InitializeOnLoad]
    internal static class PackageInitializer
    {
        static PackageInitializer()
        {
            CorePackageImportListener.OnCorePackageImported += CreateDvrFileIfNotPresent;
        }

        private static void CreateDvrFileIfNotPresent()
        {
            var manifest = PluginManifest.LoadFromLocalFile();
            if (manifest == null || Database.TryGetDvrAssetPath(out _))
                return;
            EditorApplication.delayCall += () =>
            {
                DvrScriptManager.CreateOrUpdateDvrFileAsync(false).ListenForErrors();
            };
        }
    }
}