using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HomaGames.Geryon;
using HomaGames.HomaBelly.Internal.Analytics;
using HomaGames.HomaBelly.Utilities;
using JetBrains.Annotations;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Class used to fetch Damysus Remote Configuration.
    ///
    /// By sending to the server some useful information about Damysus
    /// configuration (app token, app identifier and dependencies), the
    /// server will return a configuration for the app at runtime
    /// </summary>
    public static class RemoteConfiguration
    {
        private static readonly string FIRST_TIME_CONFIG_ENDPOINT = $"{HomaBellyConstants.API_HOST}/appfirsttime"; 
        private static readonly string EVERY_TIME_CONFIG_ENDPOINT = $"{HomaBellyConstants.API_HOST}/appeverytime";

        private const int TimeoutDelayMs = 3000;

        private static string advertisingID;

        /// <summary>
        /// Response model fetched from Remote Configuration endpoints
        /// </summary>
        public abstract class RemoteConfigurationModel
        {
            public string AppToken;
            public CrossPromotionConfigurationModel CrossPromotionConfigurationModel;
            public GeryonConfigurationModel GeryonConfigurationModel;
        }
        
        public class RemoteConfigurationModelFirstTime : RemoteConfigurationModel 
        { }

        public class RemoteConfigurationModelEveryTime : RemoteConfigurationModel
        {
            public ForceUpdateConfigurationModel ForceUpdateConfigurationModel;
            public AttributionConfigurationModel AttributionConfigurationModel;
            public PackagesConfiguration PackagesConfiguration;
        }

        public static RemoteConfigurationModelFirstTime FirstTime;
        public static RemoteConfigurationModelEveryTime EveryTime;

        #region Public methods

        private static bool? _firstTimeConfigurationNeededCache;
        public static bool FirstTimeConfigurationNeededThisSession()
        {
            if (_firstTimeConfigurationNeededCache == null)
                _firstTimeConfigurationNeededCache =
                    PlayerPrefs.GetInt(RemoteConfigurationConstants.FIRST_TIME_ALREADY_REQUESTED, 0) == 0;

            return _firstTimeConfigurationNeededCache.Value;
        }

        public static void PrepareRemoteConfigurationFetching()
        {
            try
            {
                if (FirstTimeConfigurationNeededThisSession())
                {
                    PlayerPrefs.SetInt(RemoteConfigurationConstants.FIRST_TIME_ALREADY_REQUESTED, 1);
                    PlayerPrefs.Save();
                }
            }
            catch (Exception e)
            {
                HomaGamesLog.Warning($"[Remote Configuration] Could not prepare for remote configuration fetching: {e}");
            }
            
            // At this point, Identifiers are set thanks to CoreInitializer.
#if UNITY_IOS
			advertisingID = Identifiers.Idfa;
#else
            advertisingID = Identifiers.Gaid;
#endif
        }

        #endregion

        #region Private helpers

        private static string GenerateRequestUri(string endpoint, string ti, string mvi)
        {
            return UriHelper.AddGetParameters(endpoint, new Dictionary<string, string>
            {
                {"cv", HomaBellyConstants.API_VERSION}, // Configuration version
                {"ti", ti}, // Token Identifier
                {"av", Application.version}, // Application version
                {"sv", HomaBellyConstants.PRODUCT_VERSION}, // SDK version
                {"ua", SystemConstants.UserAgent}, // User agent
                {"ai", SystemConstants.ApplicationIdentifier}, // Application identifier
                {"mvi", mvi}, // Manifest version ID
                {"di", advertisingID} // device advertising ID
            });
        }

        public static async Task GetFirstTimeConfigurationAsync()
        {
            if (! FirstTimeConfigurationNeededThisSession())
            {
                HomaGamesLog.Error($"{nameof(GetFirstTimeConfigurationAsync)} called in a non-first session.");
                return;
            }
            
            InternalAnalytics.FirstTimeFetchStarted();
            var firstTimeUri = CreateConfigurationRequestUri(FIRST_TIME_CONFIG_ENDPOINT);
            HomaGamesLog.Debug($"[Remote Configuration] Requesting first time config {firstTimeUri}...");

            var response = await FetchAsync(firstTimeUri, InternalAnalytics.FirstTimeFetchFailed);
            FirstTime = ParseFirstTimeConfiguration(response);
            
            if (response.Count != JsonObject.Empty.Count)
            {
                InternalAnalytics.FirstTimeFetchCompleted();
                HomaGamesLog.Debug($"[Remote Configuration] first time config fetched");
            }
        }
        
        public static async Task GetEveryTimeConfigurationAsync()
        {
            InternalAnalytics.EveryTimeFetchStarted();
            var everyTimeUri = CreateConfigurationRequestUri(EVERY_TIME_CONFIG_ENDPOINT);
            HomaGamesLog.Debug($"[Remote Configuration] Requesting every time config {everyTimeUri}...");
            
            var response = await FetchAsync(everyTimeUri, InternalAnalytics.EveryTimeFetchFailed);
            EveryTime = ParseEveryTimeConfiguration(response);
            
            if (response.Count != JsonObject.Empty.Count)
            {
                InternalAnalytics.EveryTimeFetchCompleted();
                HomaGamesLog.Debug($"[Remote Configuration] every time config fetched");
            }
        }
        
        private static string CreateConfigurationRequestUri(string endpoint)
        {
            HomaBellyManifestConfiguration.TryGetString(out var ti, HomaBellyManifestConfiguration.MANIFEST_TOKEN_KEY);
            HomaBellyManifestConfiguration.TryGetString(out var mvi, HomaBellyManifestConfiguration.MANIFEST_VERSION_ID_KEY);
            return GenerateRequestUri(endpoint, ti, mvi);
        }

        private static async Task<JsonObject> FetchAsync(string uri, Action<string> fetchFailedHandler)
        {
            var response = JsonObject.Empty;
            try
            {
                response = await GetAsync(uri) ?? JsonObject.Empty;
            }
            catch (FetchFailedException e)
            {
                fetchFailedHandler(e.Message);
            }

            return response;
        }

        /// <summary>
        /// Asynchronous Http GET request
        /// </summary>
        /// <param name="uri">The URI to query</param>
        /// <returns></returns>
        private static async Task<JsonObject> GetAsync(string uri)
        {
            using (HttpClient client = HttpCaller.GetHttpClient())
            {
                try
                {
                    HttpResponseMessage response = await GetWithRetriesAsync(client, uri, 3);

                    if (response!.IsSuccessStatusCode)
                    {
                        string resultString = await response.Content.ReadAsStringAsync();

                        // Return empty manifest if json string is not valid
                        if (string.IsNullOrEmpty(resultString))
                        {
                            throw new FetchFailedException("content is null or empty");
                        }

                        // Basic info
                        JsonObject jsonObject = await Task.Run(() => Json.DeserializeObject(resultString));

                        HomaGamesLog.Debug($"[Remote Configuration] Request result to {uri}\n {resultString}");
                        return jsonObject;
                    }
                }
                catch (FetchFailedException e)
                {
                    HomaGamesLog.Error($"[Remote Configuration] Exception while requesting {uri}: {e}");
                    throw;
                }
                catch (Exception e)
                {
                    HomaGamesLog.Error($"[Remote Configuration] Exception while requesting {uri}: {e}");
                }
            }

            return default;
        }

        [ItemCanBeNull]
        private static async Task<HttpResponseMessage> GetWithRetriesAsync(HttpClient client, string uri, int attempts)
        {
            for (var i = 0; i < attempts; i++)
            {
                var requestTask = client.GetAsync(uri);
                var delayTask = Task.Delay(TimeoutDelayMs);

                var firstTaskToFinish = await Task.WhenAny(requestTask, delayTask);

                if (firstTaskToFinish == requestTask && firstTaskToFinish.Exception == null)
                {
                    return requestTask.Result;
                }

                HandleUnsuccessfulAttempt(uri, i, attempts, requestTask, client);
            }

            throw new FetchFailedException("timeout");
        }

        private static void HandleUnsuccessfulAttempt(string uri, int currentAttempt, int totalAttempts,
            Task<HttpResponseMessage> requestTask, HttpClient httpClient)
        {
            if (requestTask.Exception != null)
                HomaGamesLog.Error($"Error while fetching remote configuration: {requestTask.Exception}");

            if (currentAttempt == totalAttempts - 1)
                HomaGamesLog.Error($"Could not fetch remote configuration at \"{uri}\". Moving on.");
            else
                HomaGamesLog.Warning(
                    $"Could not fetch remote configuration on attempt {currentAttempt + 1}. Retrying...");

            httpClient.CancelPendingRequests();
        }
        
        private static RemoteConfigurationModelFirstTime ParseFirstTimeConfiguration(JsonObject rawResponse)
        {
            var output = ParseConfigurationCommon<RemoteConfigurationModelFirstTime>(rawResponse);
            
            // First time specific parsing

            return output;
        }
        
        private static RemoteConfigurationModelEveryTime ParseEveryTimeConfiguration(JsonObject rawResponse)
        {
            var output = ParseConfigurationCommon<RemoteConfigurationModelEveryTime>(rawResponse);
            
            if (rawResponse != null)
            {
                if (rawResponse.TryGetNotNull("res", out JsonObject resultObject))
                {

                    resultObject.TryGetNotNull<JsonObject>("o_force_update", forceUpdateData =>
                    {
                        output.ForceUpdateConfigurationModel =
                            ForceUpdateConfigurationModel.FromServerResponse(forceUpdateData);
                    });

                    resultObject.TryGetNotNull<JsonObject>("o_attributions", attributionConfigurationData =>
                    {
                        output.AttributionConfigurationModel =
                            AttributionConfigurationModel.FromRemoteConfigurationDictionary(
                                attributionConfigurationData);
                    });

                    resultObject.TryGetNotNull<JsonList>("ao_packages", packagesData =>
                    {
                        output.PackagesConfiguration = PackagesConfiguration.FromJson(packagesData);
                    });
                }
            }

            return output;
        }

        private static TModel ParseConfigurationCommon<TModel>(JsonObject rawResponse)
            where TModel : RemoteConfigurationModel, new()
        {
            var output = new TModel();

            if (rawResponse != null)
            {
                rawResponse.TryGetNotNull<string>("ti", s => output.AppToken = s);


                if (rawResponse.TryGetNotNull("res", out JsonObject resultObject))
                {
                    resultObject.TryGetNotNull<JsonObject>("o_cross_promotion", crossPromotionData =>
                    {
                        output.CrossPromotionConfigurationModel = CrossPromotionConfigurationModel.FromRemoteConfigurationDictionary(crossPromotionData);
                    });
                    
                    resultObject.TryGetNotNull<JsonObject>("o_geryon", geryonData =>
                    {
                        output.GeryonConfigurationModel =
                            GeryonConfigurationModel.FromServerResponse(geryonData);
                    });
                }
            }

            return output;
        }
#endregion

    }
}
