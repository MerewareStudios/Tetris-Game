using HomaGames.HomaBelly.Utilities;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Model representing a remote package configuration sent by the server inside "ao_packages".
    /// </summary>
    public class RemotePackage
    {
        public string PackageKey { get; }
        public string Version { get; }

        public JsonObject Data { get; }

        private RemotePackage(string packageKey, string version, JsonObject data)
        {
            PackageKey = packageKey;
            Version = version;
            Data = data;
        }

        public static RemotePackage FromJson(JsonObject package)
        {
            string packageKey = null;
            string versionNumber = null;
            JsonObject data = null;

            if (package.TryGetNotNull<string>("s_package_key", out var key))
            {
                packageKey = key;
            }

            if (package.TryGetNotNull<string>("s_version_number", out var version))
            {
                versionNumber = version;
            }

            if (package.TryGetNotNull<JsonObject>("o_data", out var oData))
            {
                data = oData;
            }

            return new RemotePackage(packageKey, versionNumber, data);
        }
        
        /// <summary>
        /// Try to obtain a parameter of type <see cref="T"/> identified by the given key. 
        /// </summary>
        /// <param name="parameterKey">The key identifying the parameter.</param>
        /// <param name="value">The nullable <see cref="T"/> output</param>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <returns>True if the parameter of type <see cref="T"/> was successfully retrieved, false otherwise.</returns>
        public bool TryGetParameter<T>(string parameterKey, out T value)
        {
            if (Data.TryGetNotNull(parameterKey, out value))
            {
                return true;
            }

            HomaGamesLog.Warning($"[Remote Package Configuration]: {parameterKey} not found in package {PackageKey}");
            return false;
        }
    }
}