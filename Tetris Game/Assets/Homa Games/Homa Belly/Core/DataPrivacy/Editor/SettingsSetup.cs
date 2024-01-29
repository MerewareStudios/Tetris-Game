using System;
using JetBrains.Annotations;
using Unity.Usercentrics;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace HomaGames.HomaBelly.DataPrivacy
{
    internal class SettingsSetup : AssetPostprocessor
    {
        [SettingsProvider]
        private static SettingsProvider RegisterSettingsProvider()
        {
            const string settingsPath = HomaGames.HomaBelly.Settings.BASE_DIRECTORY + "/Data Privacy";
            
            return AssetSettingsProvider.CreateProviderFromObject(settingsPath, LoadOrCreateSettings());
        }

        public static DataPrivacy.Settings LoadOrCreateSettings()
        {
            DataPrivacy.Settings settings = LoadSettings();
            
            if (! settings)
                settings = CreateNewSettings();

            return settings;
        }
        
        [CanBeNull]
        private static DataPrivacy.Settings LoadSettings()
        {
            return AssetDatabase.LoadAssetAtPath<DataPrivacy.Settings>(Constants.DATA_PRIVACY_SETTINGS_ASSET_PATH);
        }
        
        private static DataPrivacy.Settings CreateNewSettings()
        {
            var settings = ScriptableObject.CreateInstance<DataPrivacy.Settings>();
            FileUtilities.CreateIntermediateDirectoriesIfNecessary(Constants.DATA_PRIVACY_SETTINGS_ASSET_PATH);
            AssetDatabase.CreateAsset(settings, Constants.DATA_PRIVACY_SETTINGS_ASSET_PATH);
            AssetDatabase.SaveAssets();

            return settings;
        }
    }
}