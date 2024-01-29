using System.Collections.Generic;
using System.Globalization;
using HomaGames.HomaBelly.Utilities;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Model representing the list of packages configuration sent by the server as "ao_packages".
    /// </summary>
    public class PackagesConfiguration
    {
        /// <summary>
        /// Number of packages in the configuration.
        /// </summary>
        public int Count => RemotePackagesByKey.Count;

        /// <summary>
        /// List of raw data of each remote package.
        /// </summary>
        public List<object> RawData { get; private set; }

        private Dictionary<string, RemotePackage> RemotePackagesByKey { get; set; }

        public static PackagesConfiguration FromJson(JsonList packagesData, bool displayLogs = true)
        {
            PackagesConfiguration packagesConfiguration = new PackagesConfiguration
            {
                RemotePackagesByKey = new Dictionary<string, RemotePackage>()
            };

            if (packagesData == null) return packagesConfiguration;

            packagesConfiguration.RawData = packagesData.ToRawData();
            for (int i = 0; i < packagesData.Count; i++)
            {
                if (!packagesData.TryGetNotNull<JsonObject>(i, out var jsonObject))
                    continue;

                RemotePackage remotePackage = RemotePackage.FromJson(jsonObject);
                if (remotePackage != null)
                {
                    packagesConfiguration.RemotePackagesByKey.Add(remotePackage.PackageKey, remotePackage);
                    if (displayLogs) HomaGamesLog.Debug($"[Remote Package Configuration]: {remotePackage.PackageKey} added to configuration.");
                }
            }

            return packagesConfiguration;
        }

        /// <summary>
        /// Try to obtain a <see cref="RemotePackage"/> identified by the given key.
        /// </summary>
        /// <param name="packageKey">The key identifying the remote package.</param>
        /// <param name="remotePackage"></param>
        /// <returns>True if the <see cref="RemotePackage"/> was successfully retrieved, false otherwise.</returns>
        public bool TryGetPackage(string packageKey, out RemotePackage remotePackage)
        {
            if (RemotePackagesByKey.TryGetValue(packageKey, out var package))
            {
                remotePackage = package;
                return true;
            }

            HomaGamesLog.Warning($"[Remote Package Configuration]: Package {packageKey} not found!");
            remotePackage = null;
            return false;
        }
        
        /// <summary>
        /// Try to obtain a parameter of type <see cref="T"/> from the <see cref="RemotePackage"/> identified by the given keys. 
        /// </summary>
        /// <param name="packageKey">The key identifying the remote package.</param>
        /// <param name="parameterKey">The key identifying the parameter.</param>
        /// <param name="value">The nullable <see cref="T"/> output</param>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <returns>True if the parameter of type <see cref="T"/> was successfully retrieved, false otherwise.</returns>
        public bool TryGetParameterFromPackage<T>(string packageKey, string parameterKey, out T value)
        {
            value = default;
            return RemotePackagesByKey.TryGetValue(packageKey, out var remotePackage) &&
                   remotePackage.TryGetParameter(parameterKey, out value);
        }
    }
}