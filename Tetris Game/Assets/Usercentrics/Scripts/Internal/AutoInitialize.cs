using Unity.Usercentrics;
using UnityEngine;

namespace Unity.Usercentrics
{
    public class AutoInitialize : MonoBehaviour
    {
        [SerializeField] public bool Enabled = true;

        void Awake()
        {
            Debug.Log("[USERCENTRICS] AutoInitialize is " + Enabled);

            if (!Enabled)
            {
                return;
            }

            Usercentrics.Instance.Initialize((usercentricsReadyStatus) =>
            {
                if (usercentricsReadyStatus.shouldCollectConsent)
                {
                    ShowFirstLayer();
                }
            },
            (errorMessage) =>
            {
                Debug.Log("[USERCENTRICS] AutoInitialize is " + errorMessage);
            });
        }

        private void ShowFirstLayer()
        {
            Usercentrics.Instance.ShowFirstLayer((usercentricsConsentUserResponse) => {});
        }
    }
}
