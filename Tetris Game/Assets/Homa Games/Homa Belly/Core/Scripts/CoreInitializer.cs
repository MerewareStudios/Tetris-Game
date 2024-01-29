using System;
using System.Threading.Tasks;
using HomaGames.HomaBelly.DataPrivacy;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public static class CoreInitializer
    {
        private static Task InitializationTask;

#if !HOMA_BELLY_SKIP_CORE_INITIALIZATION
    #if UNITY_2019_3_OR_NEWER
            // According to tests identifier fetching does not start before the splash screen is finished, 
            // but the behaviour is not guaranteed.  
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    #else
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    #endif
#endif
        private static void Initialize()
        {
            // Populate SystemConstants from Unity Main Thread in order
            // to access Application and SystemInfo APIs
            SystemConstants.Populate();
            
            // Always initialize Homa Belly after Data Privacy flow has been completed
            DataPrivacyFlowNotifier.OnFlowCompleted += () => InitializationTask = InitializeAsync();
        }

#if !HOMA_BELLY_SKIP_CORE_INITIALIZATION
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
#endif
        private static void OnGameStarts()
        {
        }

        private static async Task InitializeAsync()
        {
            try
            {
                HomaBellyManifestConfiguration.Initialise();
                var geryonInitTask = Geryon.Config.OnGameLaunch();
                await Identifiers.Initialize();

                RemoteConfiguration.PrepareRemoteConfigurationFetching();

                var firstTimeConfigFetch = RemoteConfiguration.FirstTimeConfigurationNeededThisSession()
                    ? RemoteConfiguration.GetFirstTimeConfigurationAsync()
                    : Task.CompletedTask;
                var everyTimeConfigFetch = RemoteConfiguration.GetEveryTimeConfigurationAsync();

                async Task GeryonEveryTimeAfter(params Task[] previousTasks)
                {
                    await Task.WhenAll(previousTasks);
                    await Geryon.Config.OnEveryTimeConfigurationFetched();
                }
                var geryonEveryTimeTask = GeryonEveryTimeAfter(geryonInitTask, everyTimeConfigFetch);

                async Task ForceUpdatePopupAfter(params Task[] previousTasks)
                {
                    await Task.WhenAll(previousTasks);
                    ForceUpdatePopup.ShowPopupIfRequired();
                }
                var forceUpdatePopupTask = ForceUpdatePopupAfter(everyTimeConfigFetch);

                await Task.WhenAll(firstTimeConfigFetch, geryonInitTask);
                
                if (RemoteConfiguration.FirstTimeConfigurationNeededThisSession())
                    await Geryon.Config.OnFirstTimeConfigurationFetched();
                
                await HomaBelly.Initialize();
                
                await everyTimeConfigFetch;
                await HomaBelly.InitializeRemoteConfigurationDependantComponents(RemoteConfiguration
                    .EveryTime);

                await Task.WhenAll(geryonEveryTimeTask, forceUpdatePopupTask);
            }
            catch (Exception e)
            {
                Debug.LogError("Error while Initializing Homa Belly Core:\n" + e);
                // Just in case someone wants to read the task to check for errors
                throw;
            }
        }
    }
}
