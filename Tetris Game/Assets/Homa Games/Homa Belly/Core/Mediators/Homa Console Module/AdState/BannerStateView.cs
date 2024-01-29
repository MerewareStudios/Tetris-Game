using HomaGames.HomaBelly;
using HomaGames.HomaConsole.UI;
using UnityEngine.UIElements;

namespace HomaGames.HomaConsole.AdsModule
{
    internal class BannerStateView : AdStateView
    {
        public BannerStateView(VisualElement visualElement, string adUnit, AdPlacementType adPlacementType,
            MediatorBase mediator) :
            base(visualElement, adUnit, adPlacementType, AdType.Banner, mediator)
        {
            var destroyButton = new Button(() =>
            {
                Mediator.DestroyBanner(adUnit);
                RefreshAdState();
            })
            {
                text = "Destroy"
            };
            destroyButton.AddToClassList("btn");
            ActionRoot.Add(destroyButton);
            var hideButton = new Button(() =>
            {
                Mediator.HideBanner(adUnit);
                RefreshAdState();
            })
            {
                text = "Hide"
            };
            hideButton.AddToClassList("btn");
            ActionRoot.Add(hideButton);
            Mediator.BannerAdClickedEvent += (a) => EventReceived(nameof(MediatorBase.BannerAdClickedEvent), a);
            Mediator.BannerAdLoadFailedEvent += (a, errorCode, error) =>
                EventReceived(nameof(MediatorBase.BannerAdLoadFailedEvent), a, errorCode, error);
            Mediator.OnBannerLoadedEvent += (a) => { EventReceived(nameof(MediatorBase.OnBannerLoadedEvent), a); };
        }

        protected override void OnLoadButtonClicked()
        {
            Mediator.LoadBanner(BannerSize.BANNER, BannerPosition.BOTTOM, AdUnit);
        }

        protected override void OnShowButtonClicked()
        {
            Mediator.ShowBanner(AdUnit);
        }
    }
}