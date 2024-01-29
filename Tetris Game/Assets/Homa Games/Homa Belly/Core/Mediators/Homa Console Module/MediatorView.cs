using System;
using System.Collections.Generic;
using HomaGames.HomaBelly;
using UnityEngine;
using UnityEngine.UIElements;

namespace HomaGames.HomaConsole.AdsModule
{
    internal class MediatorView
    {
        private readonly ScrollView _adStatesRoot;
        private readonly MediatorBase _mediatorBase;
        private readonly Dictionary<string, AdStateView> _adStatesViews = new Dictionary<string, AdStateView>();

        public MediatorView(VisualElement root, MediatorBase mediatorBase)
        {
            _mediatorBase = mediatorBase;
            var template = Resources.Load<VisualTreeAsset>("Homa Console/Mediator");
            VisualElement tree = template.CloneTree();
            root.Add(tree);
            var groupBox = tree.Q<GroupBox>("Name");
            groupBox.text = mediatorBase.GetType().Name;
            _adStatesRoot = groupBox.Q<ScrollView>("AdStates");
            root.Q<Button>("ValidateIntegrationButton").clicked += mediatorBase.ValidateIntegration;
            var extraInterstitialTextField = root.Q<TextField>("ExtraInterstitialField");
            var extraRewardedTextField = root.Q<TextField>("ExtraRewardedField");
            root.Q<Button>("ExtraInterstitialButton").clicked +=
                () => mediatorBase.LoadInterstitial(extraInterstitialTextField.value);
            root.Q<Button>("ExtraRewardedButton").clicked +=
                () => mediatorBase.LoadInterstitial(extraRewardedTextField.value);
            
            BuildDefaultAdStates(mediatorBase);

            mediatorBase.OnBannerLoadedEvent += adUnit =>
            {
                var state = mediatorBase.GetOrCreateAdState(adUnit, AdType.Banner);
                float height = mediatorBase.GetBannerHeight(adUnit);
                if(state.BannerStyle.Position == BannerPosition.BOTTOM)
                    HomaConsole.Instance.SetBottomSafeArea(height);
                if(state.BannerStyle.Position == BannerPosition.TOP)
                    HomaConsole.Instance.SetTopSafeArea(height);
                
                CreateAdStateViewIfNecessary(adUnit, AdType.Banner, AdPlacementType.User);
            };

            mediatorBase.OnInterstitialLoadedEvent += adUnit =>
            {
                CreateAdStateViewIfNecessary(adUnit, AdType.Interstitial, AdPlacementType.User);
            };

            mediatorBase.OnRewardedAdLoadedEvent += adUnit =>
            {
                CreateAdStateViewIfNecessary(adUnit, AdType.RewardedVideo, AdPlacementType.User);
            };
        }

        private void CreateAdStateViewIfNecessary(string adUnit, AdType adType, AdPlacementType adPlacementType)
        {
            if (_adStatesViews.ContainsKey(adUnit)) return;
            switch (adType)
            {
                case AdType.RewardedVideo:
                    _adStatesViews.Add(adUnit,
                        new RewardedStateView(_adStatesRoot, adUnit, adPlacementType, _mediatorBase));
                    break;
                case AdType.Interstitial:
                    _adStatesViews.Add(adUnit,
                        new InterstitialStateView(_adStatesRoot, adUnit, adPlacementType, _mediatorBase));
                    break;
                case AdType.Banner:
                    _adStatesViews.Add(adUnit,
                        new BannerStateView(_adStatesRoot, adUnit, adPlacementType, _mediatorBase));
                    break;
                case AdType.Undefined:
                case AdType.Video:
                case AdType.Playable:
                case AdType.OfferWall:
                    break;
            }
        }

        private void BuildDefaultAdStates(MediatorBase mediatorBase)
        {
            foreach (var adPlacementType in new[] {AdPlacementType.Default, AdPlacementType.HighValue})
            {
                foreach (var adType in new[] {AdType.Interstitial, AdType.RewardedVideo, AdType.Banner})
                {
                    if (mediatorBase.TryGetDefaultAdId(adType, adPlacementType, out var currentPlacementId))
                        CreateAdStateViewIfNecessary(currentPlacementId, adType, adPlacementType);
                }
            }
        }
    }
}