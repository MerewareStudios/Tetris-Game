using HomaGames.HomaBelly;
using UnityEngine.UIElements;

namespace HomaGames.HomaConsole.AdsModule
{
    internal class RewardedStateView : AdStateView
    {
        public RewardedStateView(VisualElement visualElement, string adUnit, AdPlacementType adPlacementType,
            MediatorBase mediator) :
            base(visualElement, adUnit, adPlacementType, AdType.RewardedVideo, mediator)
        {
            Mediator.OnRewardedAdClickedEvent += (s) => EventReceived(nameof(MediatorBase.OnRewardedAdClickedEvent), s);
            Mediator.OnRewardedAdDismissedEvent +=
                (s) => EventReceived(nameof(MediatorBase.OnRewardedAdDismissedEvent), s);
            Mediator.OnRewardedAdDisplayedEvent +=
                (s) => EventReceived(nameof(MediatorBase.OnRewardedAdDisplayedEvent), s);
            Mediator.OnRewardedAdFailedEvent += (s, errorCode, error) =>
                EventReceived(nameof(MediatorBase.OnRewardedAdFailedEvent), s, errorCode, error);
            Mediator.OnRewardedAdLoadedEvent += (s) =>
            {
                EventReceived(nameof(MediatorBase.OnRewardedAdLoadedEvent), s);
            };
            Mediator.OnRewardedAdReceivedRewardEvent += (s, videoAdReward) =>
                RewardedEventReceived(nameof(MediatorBase.OnRewardedAdReceivedRewardEvent), s, videoAdReward);
            Mediator.OnRewardedAdFailedToDisplayEvent += (s, errorCode, error) =>
                EventReceived(nameof(MediatorBase.OnRewardedAdFailedToDisplayEvent), s, errorCode, error);
        }

        protected override void OnLoadButtonClicked()
        {
            Mediator.LoadRewardedVideoAd(AdUnit);
        }

        protected override void OnShowButtonClicked()
        {
            Mediator.ShowRewardedVideoAd(AdUnit);
        }
    }
}