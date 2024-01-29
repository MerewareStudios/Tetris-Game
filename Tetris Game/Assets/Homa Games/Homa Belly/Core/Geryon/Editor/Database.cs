using System;
using System.IO;
using HomaGames.HomaBelly;
using UnityEditor;
using UnityEngine;

namespace HomaGames.Geryon.Editor
{
    internal static class Database
    {
        private const string DVR_FILE_PATH = "Homa Games/Homa Belly/Preserved/Geryon/DVR.cs";

        /// <summary>
        ///     Determines if DVR.cs file is present in the project
        /// </summary>
        /// <returns></returns>
        public static bool TryGetDvrAssetPath(out string assetPath)
        {
            var dvrFiles = AssetDatabase.FindAssets("DVR");
            if (dvrFiles != null)
                foreach (var dvrFile in dvrFiles)
                {
                    var innerAssetPath = AssetDatabase.GUIDToAssetPath(dvrFile);
                    if (innerAssetPath.EndsWith("/DVR.cs"))
                    {
                        assetPath = innerAssetPath;
                        return true;
                    }
                }

            assetPath = null;
            return false;
        }

        public static void WriteDvrFile(string fileContent, DvrScriptVersion version)
        {
            var dvrFilePath = Path.Combine(Application.dataPath, DVR_FILE_PATH);
            EditorFileUtilities.CreateIntermediateDirectoriesIfNecessary(dvrFilePath);
            AssetDatabase.ReleaseCachedFileHandles();
            File.WriteAllText(dvrFilePath, fileContent);
            SetDvrScriptVersion(dvrFilePath, version);
            AssetDatabase.Refresh();
        }

        private static void SetDvrScriptVersion(string dvrPath, DvrScriptVersion version)
        {
            var content = File.ReadAllText(dvrPath);
            var versionString = GetDvrScriptVersionString(version);
            File.WriteAllText(dvrPath, $"// {versionString}\n{content}");
        }

        public static bool TryGetDvrScriptVersion(string dvrPath, out DvrScriptVersion version)
        {
            if (!File.Exists(dvrPath))
            {
                version = DvrScriptVersion.Unknown;
                return false;
            }

            var fileContent = File.ReadAllText(dvrPath);
            foreach (DvrScriptVersion dvrVer in Enum.GetValues(typeof(DvrScriptVersion)))
                if (fileContent.Contains(GetDvrScriptVersionString(dvrVer)))
                {
                    version = dvrVer;
                    return true;
                }

            // In Core pre v1.10.0 DVR script did not contain version string
            // Check for text unique to the scripts generated in older version of the package
            if (fileContent.Contains("#if UNITY_IOS && HOMA_BELLY"))
            {
                version = DvrScriptVersion.DynamicVariable;
                return true;
            }

            version = DvrScriptVersion.Unknown;
            return false;
        }

        private static string GetDvrScriptVersionString(DvrScriptVersion version)
        {
            return $"gen version: {(int)version}";
        }
    }
}