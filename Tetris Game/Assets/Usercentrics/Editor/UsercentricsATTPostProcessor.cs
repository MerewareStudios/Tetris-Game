#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEditor.iOS.Xcode;
using System.IO;
using System.Reflection;

namespace Unity.Usercentrics
{
    public class ATTPostProcessor : MonoBehaviour
    {
        [PostProcessBuild]
        public static void UpdatePlistFile(BuildTarget buildTarget, string buildPath)
        {
            AddLocalizations(buildPath);
        }

        private static void AddLocalizations(string buildPath)
        {  
            string projectPath = PBXProject.GetPBXProjectPath(buildPath);
            PBXProject project = new PBXProject();
            project.ReadFromFile(projectPath);

            AddLocalizationForDefaultEnglish(projectPath, project, buildPath);
            AddLocalizationForOtherLanguages(projectPath, project, buildPath);
        }
        
        private static void AddLocalizationForDefaultEnglish(string projectPath, PBXProject project, string path)
        {
            CreateLocalization(projectPath, project, path, "en", AppTrackingTransparency.m_EnglishDefaultMessage);
        }

        private static void AddLocalizationForOtherLanguages(string projectPath, PBXProject project, string path)
        {
            foreach (var localizationMessages in AppTrackingTransparency.m_LocalizationMessages)
            {
                var language = localizationMessages.Language;
                var popupMessage = localizationMessages.PopupMessage;
                string isoCode;

                if (language != Language.Other) { 
                    isoCode = GetIsoCode(language);
                }
                else { 
                    isoCode = localizationMessages.ManualIsoCode;
                }

                CreateLocalization(projectPath, project, path, isoCode, popupMessage);
            }
        }

        private static void CreateLocalization(string projectPath, PBXProject project, string path, string isoCode, string popupMessage)
        {
            if (popupMessage.Trim().Length == 0)
            {
                return;
            }
            
            string infoFile = "InfoPlist.strings";

            string langDir = string.Format("{0}.lproj", isoCode);
            string directory = Path.Combine(path, langDir);
            Directory.CreateDirectory(directory);

            string filePath = Path.Combine(directory, infoFile);
            string relativePath = Path.Combine(langDir, infoFile);

            File.WriteAllText(filePath, $"\"NSUserTrackingUsageDescription\" = \"{popupMessage}\";");

            string data = File.ReadAllText(filePath);

            File.WriteAllText(filePath, data);
            project.AddLocaleVariantFile(infoFile, isoCode, relativePath);

            project.WriteToFile(projectPath);
        }

        private static string GetIsoCode(Language language)
        {
            FieldInfo fieldInfo = language.GetType().GetField(language.ToString());
            IsoCodeAttribute[] attributes = fieldInfo.GetCustomAttributes(typeof(IsoCodeAttribute), false) as IsoCodeAttribute[];

            if (attributes.Length > 0)
            {
                return attributes[0].Code;
            }

            return string.Empty;
        }
    }
}
#endif
