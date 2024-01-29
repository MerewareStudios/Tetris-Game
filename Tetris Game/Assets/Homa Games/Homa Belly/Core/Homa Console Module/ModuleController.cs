using HomaGames.HomaBelly;
using HomaGames.HomaBelly.DataPrivacy;
using HomaGames.HomaConsole.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace HomaGames.HomaConsole.CoreModule
{
    using Foldout = UnityEngine.UIElements.Foldout;

    public class ModuleController : IHomaConsoleModule
    {
        [RuntimeInitializeOnLoadMethod]
        public static void Init()
        {
            HomaConsole.Instance.AddModule(new ModuleController());
        }

        private readonly ScrollView _contentRoot;

        private ModuleController()
        {
            var visualTree = Resources.Load<VisualTreeAsset>("Homa Console/CoreModule");
            Root = visualTree.CloneTree();
            _contentRoot = Root.Query<ScrollView>("ContentRoot");
            BuildUI();
        }

        private void BuildUI()
        {
            Foldout manifestFoldout = _contentRoot.Q<Foldout>("Manifest");
            _contentRoot.Insert(0, manifestFoldout);
            HomaBellyManifestConfiguration.TryGetString(out var ti, HomaBellyManifestConfiguration.MANIFEST_TOKEN_KEY);
            manifestFoldout.Add(new InfoLine("Manifest Token", ti, true));
            HomaBellyManifestConfiguration.TryGetString(out var vi,
                HomaBellyManifestConfiguration.MANIFEST_VERSION_ID_KEY);
            manifestFoldout.Add(new InfoLine("Manifest Version ID", vi, true));
            Foldout privacyFoldout = _contentRoot.Q<Foldout>("DataPrivacyFoldout");
            BuildPrivacyUI(privacyFoldout);
            var privacyRefreshButton = _contentRoot.Q<Button>("PrivacyRefresh");
            DataPrivacyFlowNotifier.OnFlowCompleted += () => BuildPrivacyUI(privacyFoldout);
            privacyRefreshButton.clicked += () => BuildPrivacyUI(privacyFoldout);
        }

        private void BuildPrivacyUI(Foldout privacyFoldout)
        {
            privacyFoldout.Clear();
            privacyFoldout.Add(
                new InfoLine("GDPR Protected Region", Manager.IsGdprProtectedRegion.ToString(), false));
            privacyFoldout.Add(
                new InfoLine("Analytics Granted", Manager.Instance.IsAnalyticsGranted().ToString(), false));
            privacyFoldout.Add(
                new InfoLine("Above Required Age", Manager.Instance.IsAboveRequiredAge().ToString(), false));
            privacyFoldout.Add(
                new InfoLine("Tailored Ads Granted", Manager.Instance.IsTailoredAdsGranted().ToString(), false));
            privacyFoldout.Add(
                new InfoLine("Terms and Conditions Accepted",
                    Manager.Instance.IsTermsAndConditionsAccepted().ToString(), false));
            privacyFoldout.Add(
                new InfoLine("IOS IDFA Flow Done", Manager.Instance.IsIOSIDFAFlowDone().ToString(), false));
            var showDataPrivacyButton = new Button(() => Manager.Instance.ShowDataPrivacySettings()) {text = "Show Data Privacy Settings"};
            showDataPrivacyButton.AddToClassList("btn");
            privacyFoldout.Add(showDataPrivacyButton);
        }

        public string Name => "Homa Core";
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