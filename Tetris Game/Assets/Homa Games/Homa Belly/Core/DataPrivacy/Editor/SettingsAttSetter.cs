#if UNITY_IOS
using System.IO;
using Unity.Usercentrics;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace HomaGames.HomaBelly.DataPrivacy
{
    internal static class SettingsAttSetter
    {
        /// <summary>
        /// Update ATT popup message in Info.plist file.
        /// We are doing it here instead of relying on Usercentrics SDK that is trying to save those values in the InfoPlist.strings file.
        /// This is because there can be only one InfoPlist.strings in the xCode project. Other packages like I2 Localization might be already using InfoPlist.strings
        /// </summary>
        [PostProcessBuild(0)]
        public static void SetAppTrackingTransparencyParameters(BuildTarget target, string pathToBuiltProject)
        {
            // Set default message to empty to avoid Usercentrics SDK writing it in InfoPlist.strings
            AppTrackingTransparency.m_EnglishDefaultMessage = "";
            var attPopupMessage = SettingsSetup.LoadOrCreateSettings().iOSIdfaPopupMessage;
            WritePlistFile(pathToBuiltProject, attPopupMessage);
        }

        private static void WritePlistFile(string buildPath, string attPopupMessage)
        {
            var shouldWriteToFile = attPopupMessage.Trim().Length != 0;
            if (!shouldWriteToFile)
            {
                return;
            }

            var plistPath = Path.Combine(buildPath, "Info.plist");
            var plist = new PlistDocument();
            plist.ReadFromFile(plistPath);

            var rootDict = plist.root;
            rootDict.SetString("NSUserTrackingUsageDescription", attPopupMessage);

            File.WriteAllText(plistPath, plist.WriteToString());
        }
    }
}
#endif