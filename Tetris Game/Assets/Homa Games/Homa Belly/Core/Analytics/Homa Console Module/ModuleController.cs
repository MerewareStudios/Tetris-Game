using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using HomaGames.HomaBelly;
using HomaGames.HomaBelly.Internal.Analytics;
using HomaGames.HomaConsole.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace HomaGames.HomaConsole.AnalyticsModule
{
    public class ModuleController : IHomaConsoleModule
    {
        [RuntimeInitializeOnLoadMethod]
        public static void Init()
        {
            HomaConsole.Instance.AddModule(new ModuleController());
        }

        private readonly VisualTreeAsset _eventTemplate;
        private readonly List<(AnalyticsEvent, VisualElement)> _events = new List<(AnalyticsEvent, VisualElement)>();
        private readonly HashSet<string> _eventCategories = new HashSet<string>();
        private readonly DropdownField _dropdownField;
        private readonly TextField _searchField;
        private readonly ScrollView _eventsScrollView;

        private ModuleController()
        {
            _eventTemplate = Resources.Load<VisualTreeAsset>("Homa Console/EventLine");
            var template = Resources.Load<VisualTreeAsset>("Homa Console/AnalyticsModule");
            VisualElement root = template.CloneTree();
            _searchField = root.Q<TextField>();
            _searchField.RegisterValueChangedCallback(OnSearchFieldChange);
            _dropdownField = root.Q<DropdownField>();
            _dropdownField.RegisterValueChangedCallback(OnCategorySelected);
            _eventCategories.Add("All");
            RefreshCategoryDropdownChoices();
            _dropdownField.index = 0;
            _eventsScrollView = root.Q<ScrollView>();
            Root = root;
            AnalyticsEventTracker.EventTracked += OnEventTracked;
        }

        private void OnCategorySelected(ChangeEvent<string> evt)
        {
            UpdateAllEventsVisibility();
        }

        private void OnSearchFieldChange(ChangeEvent<string> evt)
        {
            UpdateAllEventsVisibility();
        }

        private void UpdateAllEventsVisibility()
        {
            foreach (var eventToInspector in _events)
            {
                UpdateEventVisibility(eventToInspector.Item1, eventToInspector.Item2);
            }
        }

        private void UpdateEventVisibility(AnalyticsEvent analyticsEvent, VisualElement eventToInspector)
        {
            eventToInspector.style.display =
                ShouldBeDisplayed(analyticsEvent) ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private bool ShouldBeDisplayed(AnalyticsEvent analyticsEvent)
        {
            var isMatchingCategory =
                _dropdownField.value == "All" || _dropdownField.value == analyticsEvent.EventCategory;
            return isMatchingCategory && Regex.Match(analyticsEvent.EventName, _searchField.value).Success;
        }

        private void OnEventTracked(AnalyticsEvent analyticsEvent)
        {
            if (!_eventCategories.Contains(analyticsEvent.EventCategory))
            {
                _eventCategories.Add(analyticsEvent.EventCategory);
                RefreshCategoryDropdownChoices();
            }

            VisualElement eventElement = _eventTemplate.CloneTree();
            var nameElement = eventElement.Q<Label>("Name");
            nameElement.text = analyticsEvent.EventName;
            var categoryElement = eventElement.Q<Label>("Category");
            categoryElement.text = analyticsEvent.EventCategory;
            categoryElement.style.backgroundColor = ColorFromHash(analyticsEvent.EventCategory);
            var timeStampElement = eventElement.Q<Label>("TimeStamp");
            timeStampElement.text = DateTime.Now.ToString("HH:mm:ss",CultureInfo.InvariantCulture);
            var content = eventElement.Q<VisualElement>("Content");
            PopulateEventDataContent(content, analyticsEvent);

            _eventsScrollView.Add(eventElement);
            _events.Add((analyticsEvent, eventElement));
            UpdateEventVisibility(analyticsEvent, eventElement);
        }

        private void PopulateEventDataContent(VisualElement element,AnalyticsEvent analyticsEvent)
        {
            var data = analyticsEvent.GetData();
            if (data != null && data.Count > 0)
            {
                foreach (var variables in data)
                {
                    var line = new InfoLine(variables.Key, variables.Value.ToString(),
                        variables.Value is string);
                    line.Q<Button>().focusable = false;
                    element.Add(line);
                }
            }
        }

        private void RefreshCategoryDropdownChoices()
        {
            _dropdownField.choices = new List<string>(_eventCategories);
        }

        private Color ColorFromHash(object obj)
        {
            System.Random rand = new System.Random(obj.GetHashCode());
            return Color.HSVToRGB(rand.Next(0, 255) / 255f, 0.4f, 1);
        }

        public string Name => "Analytics";
        public VisualElement Root { get; }

        public bool SupportsInGameDisplayMode()
        {
            return false;
        }

        public void SetDisplayMode(DisplayMode displayMode)
        {
        }
    }
}