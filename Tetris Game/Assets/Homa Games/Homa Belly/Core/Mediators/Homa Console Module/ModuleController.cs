using HomaGames.HomaBelly;
using UnityEngine;
using UnityEngine.UIElements;

namespace HomaGames.HomaConsole.AdsModule
{
    internal class ModuleController : IHomaConsoleModule
    {
        [RuntimeInitializeOnLoadMethod]
        public static void Init()
        {
            HomaConsole.Instance.AddModule(new ModuleController());
        }

        private ModuleController()
        {
            var visualTree = Resources.Load<VisualTreeAsset>("Homa Console/AdsModule");
            Root = visualTree.CloneTree();
            ScrollView contentRoot = Root.Q<ScrollView>("ContentRoot");
#if UNITY_EDITOR
            contentRoot.Add(new Label("Ads module is not available in the editor."));
#else
            Events.onInitialized += () =>
            {
                HomaBridgeServices.GetMediators(out var mediators);
                foreach (var mediator in mediators)
                {
                    MediatorView mediatorView = new MediatorView(contentRoot, mediator);
                }
            };
#endif
        }

        public string Name => "Ads";
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