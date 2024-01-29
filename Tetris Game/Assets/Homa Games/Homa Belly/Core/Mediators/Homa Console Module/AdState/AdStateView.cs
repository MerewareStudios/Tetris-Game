using System;
using System.Globalization;
using HomaGames.HomaBelly;
using HomaGames.HomaConsole.UI;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;

namespace HomaGames.HomaConsole.AdsModule
{
    using Foldout = UnityEngine.UIElements.Foldout;

    internal abstract class AdStateView
    {
        [PublicAPI] public MediatorBase.AdState AdState => Mediator.GetOrCreateAdState(AdUnit, _adType);

        private readonly AdType _adType;
        protected readonly string AdUnit;
        protected readonly MediatorBase Mediator;
        private readonly ObjectInspector _adStateInspector;
        protected readonly VisualElement ActionRoot;
        private readonly ScrollView _eventsRoot;

        protected AdStateView(VisualElement visualElement, string adUnit, AdPlacementType adPlacementType,
            AdType adType, MediatorBase mediator)
        {
            _adType = adType;
            AdUnit = adUnit;
            Mediator = mediator;
            var visualTree = Resources.Load<VisualTreeAsset>("Homa Console/AdState");
            var template = visualTree.CloneTree();
            visualElement.Add(template);
            ActionRoot = template.Q<VisualElement>("Actions");
            _eventsRoot = template.Q<ScrollView>("EventsScrollView");
            var titleRoot = template.Q<Label>("Title");
            titleRoot.text = $"{adType} - {adUnit} - {adPlacementType}";
            var loadButton = ActionRoot.Q<Button>("Load");
            loadButton.clicked += LoadButtonClicked;
            var showButton = ActionRoot.Q<Button>("Show");
            showButton.clicked += ShowButtonClicked;
            _adStateInspector = template.Q<ObjectInspector>("State");
            _adStateInspector.InspectedInstance = AdState;
        }

        private void ShowButtonClicked()
        {
            OnShowButtonClicked();
            RefreshAdState();
        }

        private void LoadButtonClicked()
        {
            OnLoadButtonClicked();
            RefreshAdState();
        }


        protected abstract void OnLoadButtonClicked();
        protected abstract void OnShowButtonClicked();

        protected void RefreshAdState()
        {
            _adStateInspector.Refresh();
        }

        protected void EventReceived(string name, string placementId, int errorCode = 0, string error = null)
        {
            if (placementId != AdUnit) return;
            name = AddTimeToEvent(name);
            _eventsRoot.Add(errorCode == 0
                ? new InfoLine(name, "", false)
                : new InfoLine(name, $"[{errorCode}]:{error}", true));
            RefreshAdState();
        }

        protected void RewardedEventReceived(string name, string placementId, VideoAdReward reward)
        {
            if (placementId != AdUnit) return;
            name = AddTimeToEvent(name);
            _eventsRoot.Add(new InfoLine(name, reward.ToString(), true));
            RefreshAdState();
        }
        
        private string AddTimeToEvent(string name)
        {
            return $"[{DateTime.Now.ToString("HH:mm:ss",CultureInfo.InvariantCulture)}]  - {name}";
        }
    }
}