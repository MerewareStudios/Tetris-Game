using System;
using System.Threading.Tasks;
using UnityEngine;

namespace HomaGames.HomaBelly.DataPrivacy
{
    internal class UserCentricsController : MonoBehaviour
    {

        private void Start()
        {
            StartAsync()
                .ListenForErrors();
        }

        private async Task StartAsync()
        {
            try
            {
                await DoRunThroughConsentFlowAsync();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            
            try
            {
                await UserCentricsApiWrapper.ShowAttIfNecessaryAsync();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            DataPrivacyUtils.LoadNextGameScene();
        }

        private async Task DoRunThroughConsentFlowAsync()
        {
            if (Application.isEditor)
                return;

            try
            {
                await UserCentricsApiWrapper.InitializeAsync();

                if (UserCentricsApiWrapper.IsApplicationBlacklisted)
                {
                    UserCentricsApiWrapper.OverrideConsent();
                    return;
                }

                if (UserCentricsApiWrapper.InitializationFailed)
                {
                    return;
                }

                if (!UserCentricsApiWrapper.ShouldCollectConsent)
                {
                    return;
                }

                bool consentScreenInteractedWith = await UserCentricsApiWrapper.ShowConsentFirstScreenAndAttAsync();

                if (!consentScreenInteractedWith)
                {
                    Application.Quit();
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                if(!UserCentricsApiWrapper.Initialized)
                    UserCentricsApiWrapper.OverrideConsent();
                await UserCentricsApiWrapper.ApplyDataAsync();
            }
        }
    }
}
