using System.Collections.Generic;
using UnityEditor;

namespace HomaGames.HomaBelly
{
    internal static class HomaSettingsProvider
    {
        [SettingsProvider]
        private static SettingsProvider RegisterSettings()
        {
            return
                new DefineSymbolsSettingsProvider(
                    Settings.BASE_DIRECTORY,
                    new List<DefineSymbolSettingsElement>
            {
                new DefineSymbolSettingsElement
                {
                    DefineSymbolName = "HOMA_DEVELOPMENT",
                    SettingsName = "Development Mode",
                    SettingsTooltip = "Development Mode will enable Unity Logs. Disable it for release builds to benefit from multiple Homa Belly optimizations",
                    DefaultValue = true,
                    DefaultValuePrefKey = "homagames.development_mode_enabled_on_first_install"
                },
                new DefineSymbolSettingsElement
                {
                    DefineSymbolName = "HOMA_BELLY_DEBUG_CLASS_OVERRIDE_ENABLED",
                    SettingsName = "Disable Debug.Logs",
                    SettingsTooltip = "Homa Belly by default does override Unity's Debug class for optimization reasons. Disabling it will affect performance on mobile devices",
                    DefaultValue = true,
                    DefaultValuePrefKey = "homagames.debug_override_enabled_on_first_install"
                }
            });
        }
    }
}