using System;
using System.Collections.Generic;
using System.Linq;
using HomaGames.Geryon;
using HomaGames.HomaConsole.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace HomaGames.HomaConsole.Geryon
{
    public class NTestingModule : IHomaConsoleModule
    {
        private readonly ScrollView _contentRoot;
        private readonly InfoLine _initStatusLabel;
        private static bool IsNTestInitialized => Config.Initialized;

        private NTestingModule()
        {
            var visualTree = Resources.Load<VisualTreeAsset>("Homa Console/NTestingModule");
            Root = visualTree.CloneTree();
            _contentRoot = Root.Query<ScrollView>("ContentRoot");
            _initStatusLabel = new InfoLine("N-Testing initialization status", IsNTestInitialized.ToString(),false);
            _contentRoot.Add(_initStatusLabel);
            UpdateConfigInitStatus();

            Config.OnInitialized += () =>
            {
                UpdateConfigInitStatus();
                DisplayConfigInfo();
                DisplayDvrDatabaseInfo();
            };
        }

        public string Name => "N-Testing";
        public VisualElement Root { get; }

        public bool SupportsInGameDisplayMode()
        {
            return false;
        }

        public void SetDisplayMode(DisplayMode displayMode)
        {
        }

        [RuntimeInitializeOnLoadMethod]
        public static void Init()
        {
            HomaConsole.Instance.AddModule(new NTestingModule());
        }

        private void DisplayConfigInfo()
        {
            var foldout = new UnityEngine.UIElements.Foldout();
            foldout.text = "Configuration Information";
            foldout.AddToClassList("collapse");
            _contentRoot.Add(foldout);
            foldout.Add(new InfoLine("N-Testing ID", Config.NTESTING_ID, true));
            foldout.Add(new InfoLine("Variant ID", Config.VariantId, true));
            foldout.Add(new InfoLine("Scope ID", Config.ScopeId, true));
            foldout.Add(new InfoLine("Override ID", Config.OverrideId, true));
            foldout.Add(new InfoLine("External Token 0", Config.ExternalToken0, true));
            foldout.Add(new InfoLine("External Token 1", Config.ExternalToken1, true));
            foldout.Add(new InfoLine("External Token 2", Config.ExternalToken2, true));
            foldout.Add(new InfoLine("External Token 3", Config.ExternalToken3, true));
            foldout.Add(new InfoLine("External Token 4", Config.ExternalToken4, true));
        }

        private void UpdateConfigInitStatus()
        {
            _initStatusLabel.InfoText = IsNTestInitialized.ToString();
        }

        private void DisplayDvrDatabaseInfo()
        {
            var foldout = new UnityEngine.UIElements.Foldout();
            foldout.text = "N-Testing Values";
            foldout.AddToClassList("collapse");
            _contentRoot.Add(foldout);

            var editableLines = CollectAllEditableLines();
            editableLines.Sort((a, b) => string.CompareOrdinal(a.InfoName, b.InfoName));

            foreach (var editableLine in editableLines)
                foldout.Add(editableLine);
        }

        private List<InfoLine> CollectAllEditableLines()
        {
            var editableLines = new List<InfoLine>();
            editableLines.AddRange(CreateEditableBoolLines(Config.DvrDatabase.Booleans));
            editableLines.AddRange(CreateEditableDoubleLines(Config.DvrDatabase.Doubles));
            editableLines.AddRange(CreateEditableIntegerLines(Config.DvrDatabase.Ints));
            editableLines.AddRange(CreateEditableStringLines(Config.DvrDatabase.Strings));

            return editableLines;
        }

        private List<T> CreateEditableLines<T, TU>(DvrCollection<TU> dvrs, Func<string, TU, T> createLine)
            where T : EditableLine<TU>
        {
            var editableLines = new List<T>();
            var sortedDvrs = dvrs.OrderBy(d => d.Key);

            foreach (var dvr in sortedDvrs)
            {
                var dvrWithoutPrefix = dvr.Key.Substring(dvr.Key.IndexOf('_') + 1);
                var editableLine = createLine(dvrWithoutPrefix, dvr.Value.Value);
                editableLine.OnValueChanged += value =>
                {
                    // Supporting legacy DVRs
                    DynamicVariable<TU>.Set(dvr.Key, value);
                    dvr.Value.Value = value;
                };
                editableLines.Add(editableLine);
            }

            return editableLines;
        }

        private List<EditableDoubleLine> CreateEditableDoubleLines(DvrCollection<double> dvrs)
        {
            return CreateEditableLines(dvrs, (key, value) => new EditableDoubleLine(key, value.ToString()));
        }

        private List<EditableBoolLine> CreateEditableBoolLines(DvrCollection<bool> dvrs)
        {
            return CreateEditableLines(dvrs, (key, value) => new EditableBoolLine(key, value));
        }

        private List<EditableStringLine> CreateEditableStringLines(DvrCollection<string> dvrs)
        {
            return CreateEditableLines(dvrs, (key, value) => new EditableStringLine(key, value, true));
        }

        private List<EditableIntegerLine> CreateEditableIntegerLines(DvrCollection<int> dvrs)
        {
            return CreateEditableLines(dvrs, (key, value) => new EditableIntegerLine(key, value));
        }
    }
}