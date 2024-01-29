using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Google;
using GooglePlayServices;
using HomaGames.HomaBelly;
using UnityEditor;

namespace HomaGames.Edm4uExtensions
{
    internal static class GradleConfigManager
    {
        private const string MainTemplateFile = "mainTemplate.gradle";
        private const string GradleTemplatePropertiesFile = "gradleTemplate.properties";

        private const string AndroidPluginsFolderPath = "Assets/Plugins/Android";
        private const string GradleTemplatePath = AndroidPluginsFolderPath + "/" + MainTemplateFile;

        private const string GradleTemplatePropertiesPath = AndroidPluginsFolderPath + "/" + GradleTemplatePropertiesFile;

        private const string AndroidResolverSettingsPath = "ProjectSettings/AndroidResolverDependencies.xml";
        
#if UNITY_ANDROID
        public static string EmbeddedGradleTemplateFolderPath => $"{PlayServicesResolver.AndroidPlaybackEngineDirectory}/Tools/GradleTemplates";
#else
        public static string EmbeddedGradleTemplateFolderPath => "";
#endif

        public static string CurrentGradleTemplateFolderPath = EmbeddedGradleTemplateFolderPath;

        // Using Google Play ProjectSettings to modify properties.
        // Inspired by https://github.com/googlesamples/unity-jar-resolver/blob/5b98ca03e2da377417f0a5f6f6dad4c0e24652d3/source/AndroidResolver/src/SettingsDialog.cs#L157
        private static readonly ProjectSettings ProjectSettings = new ProjectSettings("GooglePlayServices.");
        
        /// <summary>
        /// Determines if the value of `CurrentGradleTemplateFolderPath` properly
        /// contains the desired Gradle templates
        /// </summary>
        /// <returns>true if Gradle templates are detected, false otherwise</returns>
        internal static bool IsValidCurrentGradleTemplatesPath()
        {
            var folderExists = Directory.Exists(CurrentGradleTemplateFolderPath);
            var mainGradleTemplateFileExists =
                File.Exists(Path.Combine(CurrentGradleTemplateFolderPath, MainTemplateFile));
            var gradleTemplatePropertiesFileExists =
                File.Exists(Path.Combine(CurrentGradleTemplateFolderPath, GradleTemplatePropertiesFile));

            return folderExists && mainGradleTemplateFileExists && gradleTemplatePropertiesFileExists;
        }

        /// <summary>
        /// Enable the desired build pipeline
        /// </summary>
        /// <param name="pipeline">Any value represented in AvailableBuildPipelinesEnum</param>
        public static void EnableBuildPipeline(AndroidDependencyResolutionSolution pipeline)
        {
            switch (pipeline)
            {
                case AndroidDependencyResolutionSolution.ImportDependenciesToProject:
                    EnableLegacyBuildPipeline();
                    break;
                case AndroidDependencyResolutionSolution.PatchGradleTemplates:
                    EnableCustomGradleBuildPipeline();
                    break;
            }
        }

        /// <summary>
        /// Enables Legacy build pipeline by setting `patchMainTemplateGradle` property to false.
        /// Gradle template files from Assets/Plugins/Android will remain
        /// </summary>
        private static void EnableLegacyBuildPipeline()
        {
            try
            {
                CloseAndroidResolverSettingsWindowIfOpen();
                ProjectSettings.SetBool("GooglePlayServices.PatchMainTemplateGradle", false);
            }
            catch (Exception e)
            {
                HomaGamesLog.Error($"[EDM4U Extensions] Could not enable custom gradle pipeline: {e}");
            }
        }

        /// <summary>
        /// Enables Custom Gradle build pipeline by creating Gradle template files at Assets/Plugins/Android
        /// </summary>
        public static void EnableCustomGradleBuildPipeline()
        {
            try
            {
                // Safety check. Before trying to copy files, check the path contains the templates
                if (!IsValidCurrentGradleTemplatesPath())
                {
                    HomaGamesLog.Error(
                        $"[EDM4U Extensions] Could not find Gradle Templates within your Unity installation at {CurrentGradleTemplateFolderPath}");
                    return;
                }

                FileUtilities.CreateIntermediateDirectoriesIfNecessary(AndroidPluginsFolderPath + "/");

                EnableMainGradleTemplateFile();
                EnableGradleTemplatePropertiesFile();
                
                CloseAndroidResolverSettingsWindowIfOpen();
                ProjectSettings.SetBool("GooglePlayServices.PatchMainTemplateGradle", true);
                
                EditorApplication.delayCall += AssetDatabase.Refresh;
            }
            catch (Exception e)
            {
                HomaGamesLog.Error($"[EDM4U Extensions] Could not enable custom gradle pipeline: {e}");
            }
        }

        private static void EnableMainGradleTemplateFile()
        {
            if (!File.Exists(GradleTemplatePath))
            {
                if (File.Exists(GradleTemplatePath + ".DISABLED"))
                {
                    File.Move(GradleTemplatePath + ".DISABLED", GradleTemplatePath);
                    File.Move(GradleTemplatePath + ".DISABLED.meta", GradleTemplatePath + ".meta");
                }
                else
                {
                    File.Copy(Path.Combine(CurrentGradleTemplateFolderPath, MainTemplateFile),
                        GradleTemplatePath);
                }
            }
        }

        /// <summary>
        /// Close Android Resolver Settings window to prevent UI inconsistency while
        /// modifying Project Settings
        /// </summary>
        private static void CloseAndroidResolverSettingsWindowIfOpen()
        {
            if (EditorWindow.HasOpenInstances<GooglePlayServices.SettingsDialog>())
            {
                var window = EditorWindow.GetWindow(typeof(GooglePlayServices.SettingsDialog));
                window.Close();                    
            }
        }
        
        private static void EnableGradleTemplatePropertiesFile()
        {
            if (!File.Exists(GradleTemplatePropertiesPath))
            {
                if (File.Exists(GradleTemplatePropertiesPath + ".DISABLED"))
                {
                    File.Move(GradleTemplatePropertiesPath + ".DISABLED", GradleTemplatePropertiesPath);
                    File.Move(GradleTemplatePropertiesPath + ".DISABLED.meta", GradleTemplatePropertiesPath + ".meta");
                }
                else
                {
                    File.Copy(Path.Combine(CurrentGradleTemplateFolderPath, GradleTemplatePropertiesFile),
                        GradleTemplatePropertiesPath);
                }
            }
        }

        public static bool IsMainGradleTemplateEnabled()
        {
            return File.Exists(GradleTemplatePath);
        }

        public static bool IsGradleTemplatePropertiesEnabled()
        {
            return File.Exists(GradleTemplatePropertiesPath);
        }

        
        /// <summary>
        /// Checks Android Resolver Settings XML file for a specific setting returning if it has the desired value.
        /// </summary>
        public static bool IsAndroidResolverSettingProperlyConfigured(string settingName, string value)
        {
            var androidResolver = LoadAndroidResolverSettings();
            if (androidResolver != null)
            {
                var elementDependencies = androidResolver.Element("dependencies");
                if (elementDependencies == null)
                {
                    return false;
                }

                // Sanity check: application
                var elementSettings = elementDependencies.Element("settings");
                if (elementSettings == null)
                {
                    return false;
                }

                var gradleTemplateEnabledElement = elementSettings
                    .Descendants()
                    .FirstOrDefault(element => element.Attribute("name")?.Value == settingName);

                var currentValue = gradleTemplateEnabledElement?.Attribute("value")?.Value == value;
                return currentValue;
            }

            return true;
        }

        private static XDocument LoadAndroidResolverSettings()
        {
            XDocument dependencies = null;
            if (File.Exists(AndroidResolverSettingsPath))
            {
                try
                {
                    dependencies = XDocument.Load(AndroidResolverSettingsPath);
                }
                catch (IOException exception)
                {
                    HomaGamesLog.Error($"Could not load Android Resolver Dependencies file: {exception.Message}");
                }
            }

            return dependencies;
        }
    }
}