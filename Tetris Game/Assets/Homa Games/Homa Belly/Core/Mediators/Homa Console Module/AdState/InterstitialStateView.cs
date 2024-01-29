using HomaGames.HomaBelly;
using UnityEngine.UIElements;

namespace HomaGames.HomaConsole.AdsModule
{
    internal class InterstitialStateView : AdStateView
    {
        public InterstitialStateView(VisualElement visualElement, string adUnit, AdPlacementType adPlacementType,
            MediatorBase mediator) : base(visualElement, adUnit, adPlacementType, AdType.Interstitial, mediator)
        {
            Mediator.InterstitialFailedToDisplayEvent += (a, errorCode, error) =>
                EventReceived(nameof(MediatorBase.InterstitialFailedToDisplayEvent), a, errorCode, error);
            Mediator.OnInterstitialClickedEvent +=
                (a) => EventReceived(nameof(MediatorBase.OnInterstitialClickedEvent), a);
            Mediator.OnInterstitialDismissedEvent +=
                (a) => EventReceived(nameof(MediatorBase.OnInterstitialDismissedEvent), a);
            Mediator.OnInterstitialFailedEvent += (a, errorCode, error) =>
                EventReceived(nameof(MediatorBase.OnInterstitialFailedEvent), a, errorCode, error);
            Mediator.OnInterstitialLoadedEvent +=
                (a) => { EventReceived(nameof(MediatorBase.OnInterstitialLoadedEvent), a); };
            Mediator.OnInterstitialShownEvent += (a) => EventReceived(nameof(MediatorBase.OnInterstitialShownEvent), a);
        }

        protected override void OnLoadButtonClicked()
        {
            Mediator.LoadInterstitial(AdUnit);
        }

        protected override void OnShowButtonClicked()
        {
            Mediator.ShowInterstitial(AdUnit);
        }
    }
}