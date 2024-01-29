using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public static class Settings
    {
        public const string BASE_DIRECTORY = "Project/HOMA";

        public static string GetUserReadableSettingsPath(string settingsPath)
        {
            const string projectPathSegment = "Project";
            if (settingsPath.StartsWith(projectPathSegment))
            {
                return settingsPath.Insert(projectPathSegment.Length, " Settings");
            }

            return settingsPath;
        }
        
        private static HashSet<ISettingsProvider> _settingsList = new HashSet<ISettingsProvider>();
        public static IEnumerable<ISettingsProvider> AllSettings => _settingsList;
        
        public static void OpenWindow()
        {
            SettingsService.OpenProjectSettings(BASE_DIRECTORY);
        }
        
        /// <summary>
        /// Deprecated, the preferred way to register analytics is now through Unity's SettingsProvider
        /// </summary>
        public static void RegisterSettings(string name,string version,Func<ScriptableObject> scriptableObject)
        {
            _settingsList.Add(new ScriptableObjectSettingsProvider(name,version,scriptableObject));
        }

        /// <summary>
        /// Deprecated, the preferred way to register analytics is now through Unity's SettingsProvider
        /// </summary>
        public static void RegisterSettings(ISettingsProvider settingsProvider)
        {
            _settingsList.Add(settingsProvider);
        }
        
        [SettingsProviderGroup]
        private static SettingsProvider[] RegisterLegacyProviders()
        {
            return AllSettings
                .OrderBy(s => s.Order)
                .Select(settingsProvider => (SettingsProvider)new LegacySettingsProvider(settingsProvider))
                .ToArray();
        }

        private class LegacySettingsProvider : SettingsProvider
        {
            private readonly ISettingsProvider _legacySettingsProvider;

            public LegacySettingsProvider(ISettingsProvider legacySettingsProvider) : base(
                $"{BASE_DIRECTORY}/{legacySettingsProvider.Name}", SettingsScope.Project)
            {
                _legacySettingsProvider = legacySettingsProvider;
            }

            public override void OnGUI(string searchContext)
            {
                _legacySettingsProvider.Draw();
            }
        }
    }   
}