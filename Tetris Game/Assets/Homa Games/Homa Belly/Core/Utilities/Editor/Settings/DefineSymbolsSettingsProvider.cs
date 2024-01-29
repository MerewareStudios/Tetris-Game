using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public class DefineSymbolsSettingsProvider : SettingsProvider, ISettingsProvider
    {
        private readonly List<DefineSymbolSettingsElement> SettingsElements;

        public DefineSymbolsSettingsProvider(string name, List<DefineSymbolSettingsElement> settingsElements)
            : base(name, SettingsScope.Project)
        {
            Name = name;
            SettingsElements = settingsElements ?? new List<DefineSymbolSettingsElement>();

            foreach (DefineSymbolSettingsElement element in SettingsElements)
            {
                DefineSymbolsUtility.TrySetInitialValue(element.DefineSymbolName, element.DefaultValue,
                    element.DefaultValuePrefKey);
            }
        }

        /// <summary>
        /// Deprecated, use DefineSymbolsSettingsProvider(string name, List settingsElements) 
        /// </summary>
        public DefineSymbolsSettingsProvider(string name, int order, List<DefineSymbolSettingsElement> settingsElements)
            : this(name, settingsElements)
        {
            Order = order;
            Version = "";
        }

        /// <summary>
        /// Deprecated, use DefineSymbolsSettingsProvider(string name, List settingsElements) 
        /// </summary>
        public DefineSymbolsSettingsProvider(string name, int order, string version,
            List<DefineSymbolSettingsElement> settingsElements) : this(name, settingsElements)
        {
            Order = order;
            Version = version;
        }

        [PublicAPI]
        public void AddSettingsElement(DefineSymbolSettingsElement element) => SettingsElements.Add(element);

        [PublicAPI]
        public void RemoveSettingsElement(DefineSymbolSettingsElement element) => SettingsElements.Remove(element);
        
        public override void OnGUI(string searchContext)
        {
            foreach (DefineSymbolSettingsElement element in SettingsElements)
            {
                bool value = DefineSymbolsUtility.GetDefineSymbolValue(element.DefineSymbolName);
                bool newValue = EditorGUILayout.Toggle(new GUIContent(element.SettingsName, element.SettingsTooltip),
                    value);

                if (value != newValue)
                    DefineSymbolsUtility.SetDefineSymbolValue(element.DefineSymbolName, newValue);
            }
        }
        
        /// <summary>
        /// Deprecated
        /// </summary>
        public int Order { get; }
        
        /// <summary>
        /// Deprecated
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// Deprecated
        /// </summary>
        public string Version { get; }
        
        /// <summary>
        /// Deprecated, use OnGUI instead.
        /// </summary>
        public void Draw()
        {
            OnGUI("");
        }
    }

    public struct DefineSymbolSettingsElement
    {
        public string DefineSymbolName;

        public string SettingsName;
        [CanBeNull] public string SettingsTooltip;

        public bool DefaultValue;

        // For legacy purposes only
        [CanBeNull] public string DefaultValuePrefKey;
    }
}