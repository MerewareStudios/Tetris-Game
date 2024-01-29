using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomaGames.HomaBelly;
using HomaGames.Geryon.Editor.CodeGen;
using HomaGames.HomaBelly.Utilities;
using UnityEngine;

namespace HomaGames.Geryon.Editor
{
    internal static class RemoteDatabase
    {
        private const string DVR_FETCHING_ENDPOINT = HomaBellyConstants.API_HOST + "/appbase";

        private static readonly Dictionary<SupportedPlatform, string> UserAgents =
            new Dictionary<SupportedPlatform, string>
            {
                [SupportedPlatform.IOS] = "IPHONE",
                [SupportedPlatform.Android] = "ANDROID"
            };

        public static async Task<DvrCodeGenModel> GetDvrCodeGenModelAsync(CancellationToken cancellationToken)
        {
            var manifest = PluginManifest.LoadFromLocalFile();
            if (manifest == null)
                throw new FileNotFoundException(
                    "Could not fetch N-Testing values as there is no Homa Belly Manifest installed");

            var iosFieldsTask = GetDvrFieldsAsync(SupportedPlatform.IOS, manifest, cancellationToken);
            var androidFieldsTask = GetDvrFieldsAsync(SupportedPlatform.Android, manifest, cancellationToken);

            await Task.WhenAll(iosFieldsTask, androidFieldsTask);

            var isoFields = iosFieldsTask.Result;
            var androidFields = androidFieldsTask.Result;
            return new DvrCodeGenModel
            {
                IOSFields = isoFields, AndroidFields = androidFields,
                UnsupportedFields = isoFields.Length > androidFields.Length ? isoFields : androidFields
            };
        }

        private static async Task<DvrField[]> GetDvrFieldsAsync(SupportedPlatform platform, PluginManifest manifest
            , CancellationToken cancellationToken = default)
        {
            var configurationModel = await new EditorHttpCaller<GeryonConfigurationModel>()
                .Get(GetDvrFetchUri(UserAgents[platform], manifest), new ConfigurationResponseDeserializer()
                    , cancellationToken);

            if (configurationModel.IsItemNoMatch())
                throw new DvrItemNoMatchException();
            if (!configurationModel.IsStatusOk())
                throw new Exception(
                    $"Error while fetching DVR code {configurationModel.ResultStatus}: {configurationModel.ResultMessage}");

            if (TryParseConfig(configurationModel, out var dvrFields))
                return dvrFields;

            return Array.Empty<DvrField>();
        }

        private static string GetDvrFetchUri(string userAgent, PluginManifest pluginManifest)
        {
            return UriHelper.AddGetParameters(DVR_FETCHING_ENDPOINT, new Dictionary<string, string>
            {
                { "cv", HomaBellyConstants.API_VERSION }, // Configuration version
                { "av", Application.version }, // Application version
                { "sv", HomaBellyConstants.PRODUCT_VERSION }, // SDK version
                { "ua", userAgent }, // User Agent
                { "ai", SystemConstants.ApplicationIdentifier }, // Application identifier
                { "ti", pluginManifest.AppToken } // App Token
            });
        }

        public static bool TryParseConfig(GeryonConfigurationModel geryonConfigurationModel, out DvrField[] result)
        {
            if (geryonConfigurationModel?.Configuration == null)
            {
                result = null;
                return false;
            }

            var fields = new Dictionary<string, DvrField>();

            foreach (var kvp in geryonConfigurationModel.Configuration)
                try
                {
                    var field = new DvrField(kvp.Key, kvp.Value);

                    if (fields.ContainsKey(field.Name))
                        continue;

                    fields.Add(field.Name, field);
                }
                catch (InvalidDvrTypeException)
                {
                    Debug.LogWarning(
                        $"Cannot recognize standard type {kvp.Value.GetType()} : please get in touch with your publishing manager.");
                }
                catch (Exception e)
                {
                    Debug.LogError(
                        $"There was an error reading the geryon model for configuration element {kvp.Key}: {e}");
                }

            result = fields.Values.ToArray();
            return true;
        }
    }
}