using HomaGames.HomaBelly;
using HomaGames.OneAsset;
using UnityEditor;
using UnityEngine;

namespace HomaGames.Edm4uExtensions
{
    // TODO: Make this class public once OneAsset is outside the core package. This is because we don't want to expose the Core version of OneAsset to our users- so that the transition to UPM is non-breaking.
    [LoadFromAsset(AssetPath, CreateAssetIfMissing = true)]
    [SettingsProviderAsset(SettingsPath)]
    internal class AndroidDependencyResolverSettings : OneScriptableObject<AndroidDependencyResolverSettings>
    {
        public const string SettingsPath = Settings.BASE_DIRECTORY + "/Android Dependency Resolver";
        internal const string AssetPath =
            "Assets/Homa Games/Settings/Editor/Resources/EDM4U Extensions/AndroidDependencyResolverSettings.asset";

        [SerializeField, HideInInspector]
        private bool initialized;

        public AndroidDependencyResolutionSolution dependencyResolutionSolution;

        private void Awake()
        {
            if (!initialized)
                Initialize(this);
        }

        private static void Initialize(AndroidDependencyResolverSettings settings)
        {
            // By default, use Custom Gradle Template
            HomaGamesLog.Debug(
                $"[EDM4U Extensions] Selecting Custom Gradle template build pipeline by default");
            settings.dependencyResolutionSolution = AndroidDependencyResolutionSolution.PatchGradleTemplates;

            // Only if its informed through Homa Belly manifest, select Legacy build pipeline
            if (ShouldInitializeWithLegacyPipeline())
            {
                HomaGamesLog.Debug(
                    $"[EDM4U Extensions] Selecting Legacy build pipeline as per s_mandatory_packages_legacy_build_pipeline_csv list");
                settings.dependencyResolutionSolution = AndroidDependencyResolutionSolution.ImportDependenciesToProject;
            }

            GradleConfigManager.EnableBuildPipeline(settings.dependencyResolutionSolution);
            
            settings.initialized = true;
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssetIfDirty(settings);
        }

        /// <summary>
        /// Decide which build pipeline should be select upon first time configuring it
        /// </summary>
        /// <returns><b>true</b> only if ALL packages in the list informed by Homa Belly Core's
        /// property `s_mandatory_packages_legacy_build_pipeline_csv` are installed into the project. Otherwise <b>false</b>
        /// </returns>
        private static bool ShouldInitializeWithLegacyPipeline()
        {
            // First time configuration logic
            if (!PluginManifest.TryGetCurrentlyInstalled(out var installedManifest))
                return false;
            
            // There is a PluginManifest.json file available and
            // it contains list of packages making optional the Gradle Custom template build pipeline
            if (HomaBellyManifestConfiguration.TryGetString(out var packagesCsv, "homabelly_core",
                    "s_mandatory_packages_legacy_build_pipeline_csv")
                && !string.IsNullOrWhiteSpace(packagesCsv))
            {
                // Manifest must have all informed packages in order to select Legacy build pipeline
                bool shouldEnableLegacyPipeline = true;
                string[] packages = packagesCsv.Split(',');
                foreach (var package in packages)
                {
                    // If at least one of the informed packages is not installed, do not select Legacy build pipeline
                    if (installedManifest.Packages.GetPackageComponent(package) == null)
                    {
                        HomaGamesLog.Debug(
                            $"[EDM4U Extensions] Mandatory package for Legacy build pipeline not found: {package}");
                        shouldEnableLegacyPipeline = false;
                        break;
                    }
                }

                return shouldEnableLegacyPipeline;
            }

            // By default, select Custom Gradle template
            return false;
        }
    }
}