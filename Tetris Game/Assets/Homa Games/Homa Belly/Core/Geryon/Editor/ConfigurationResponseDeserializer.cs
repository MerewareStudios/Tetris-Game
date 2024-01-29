using System;
using HomaGames.Geryon;
using HomaGames.HomaBelly;
using HomaGames.HomaBelly.Utilities;
using JetBrains.Annotations;
using UnityEngine;

namespace HomaGames.Geryon.Editor
{
    internal class ConfigurationResponseDeserializer : IModelDeserializer<GeryonConfigurationModel>
    {
        [NotNull]
        public GeryonConfigurationModel Deserialize(string json)
        {
            try
            {
                if (string.IsNullOrEmpty(json))
                    // Return empty but not null model
                    return new GeryonConfigurationModel();

                var jsonObject = Json.DeserializeObject(json);
                if (jsonObject != null && jsonObject.TryGetNotNull("res", out JsonObject resultObject))
                    if (resultObject.TryGetNotNull("o_geryon", out JsonObject geryonData))
                        return GeryonConfigurationModel.FromServerResponse(geryonData);
            }
            catch (Exception e)
            {
                HomaGamesLog.Warning($"[N-Testing] Could not obtain N-Testing data: {e}");
            }

            // Return empty but not null model
            return new GeryonConfigurationModel();
        }
    }
}