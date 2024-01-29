using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HomaGames.HomaBelly.DataPrivacy
{
    public static class DataPrivacyFlowNotifier
    {
        public static bool FlowCompleted { get; private set; }

        private static event Action _onFlowCompleted;
        public static event Action OnFlowCompleted
        {
            add
            {
                if (FlowCompleted)
                    value.Invoke();
                else
                    _onFlowCompleted += value;
            }
            remove
            {
                if (! FlowCompleted)
                    _onFlowCompleted -= value;
            }
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void MakeInitializationGoInEditor()
        {
            if(! DataPrivacyUtils.IsSceneDataPrivacyScene(SceneManager.GetActiveScene().buildIndex))         
                SetFlowCompleted();
        }
#endif

        public static void SetFlowCompleted()
        {
            FlowCompleted = true;
            _onFlowCompleted?.Invoke();
        }
    }
}