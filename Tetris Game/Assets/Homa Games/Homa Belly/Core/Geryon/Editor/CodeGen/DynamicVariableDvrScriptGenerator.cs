using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace HomaGames.Geryon.Editor.CodeGen
{
    /// <summary>
    /// Deprecated: use <see cref="ObservableDvrScriptGenerator"/> instead
    /// Used by <see cref="DvrScriptVersion.DynamicVariable"/>
    /// </summary>
    internal class DynamicVariableDvrScriptGenerator : IDvrScriptGenerator
    {
        private const string IOS_FIELDS_TAG = "#IOS_FIELDS#";
        private const string ANDROID_FIELDS_TAG = "#ANDROID_FIELDS#";
        private const string NO_SDK_FIELDS_TAG = "#NO_SDK_FIELDS#";

        public string GetDvrFileContent(DvrCodeGenModel codeGenModel)
        {
            var templateResourcesPath = "Geryon/DvrScriptTemplate_1";
            var classTemplateAsset = Resources.Load<TextAsset>(templateResourcesPath);
            if (classTemplateAsset == null)
                throw new FileNotFoundException($"{templateResourcesPath}.txt was not found");
            var textContent = classTemplateAsset.text;
            textContent = ReplaceTagWithIndentedMultiline(textContent, IOS_FIELDS_TAG
                , codeGenModel.IOSFields.Select(GetFieldDeclaration));
            textContent = ReplaceTagWithIndentedMultiline(textContent, ANDROID_FIELDS_TAG
                , codeGenModel.AndroidFields.Select(GetFieldDeclaration));
            textContent = ReplaceTagWithIndentedMultiline(textContent, NO_SDK_FIELDS_TAG
                , codeGenModel.UnsupportedFields.Select(GetNoSdkFieldDeclaration));
            return textContent;
        }

        private static string GetFieldDeclaration(DvrField field)
        {
            return
                $"public static {field.TypeName} {field.Name.ToUpperInvariant()} => DynamicVariable<{field.TypeName}>.Get(\"{field.Key}\", {field.ValueLiteral});";
        }

        private static string GetNoSdkFieldDeclaration(DvrField field)
        {
            return $"public static {field.TypeName} {field.Name} => default;";
        }

        private static string ReplaceTagWithIndentedMultiline(string templateContent, string tag,
            IEnumerable<string> newLines)
        {
            var tagWithIndentRegex = $"\\ *{tag}";
            var match = Regex.Match(templateContent, tagWithIndentRegex);
            var indent = match.Value.Substring(0, match.Value.Length - tag.Length);
            var replacement = string.Join(Environment.NewLine + indent, newLines);
            return Regex.Replace(templateContent, tag, replacement);
        }
    }
}