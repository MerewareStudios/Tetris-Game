using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace HomaGames.Geryon.Editor.CodeGen
{
    /// <summary>
    /// Used by <see cref="DvrScriptVersion.Observables"/>
    /// </summary>
    internal class ObservableDvrScriptGenerator : IDvrScriptGenerator
    {
        private const string IOS_FIELDS_TAG = "#IOS_FIELDS#";
        private const string ANDROID_FIELDS_TAG = "#ANDROID_FIELDS#";
        private const string UNSUPPORTED_FIELDS_TAG = "#UNSUPPORTED_FIELDS#";

        private static readonly string ObservableTypeName = GetFullNameWithoutGenericArity(typeof(Observable<>));

        private bool forceDvrUppercaseIdentifierNames;

        public ObservableDvrScriptGenerator()
        {
            forceDvrUppercaseIdentifierNames =
                GeryonEditorSettings.Instance.developerSettings.forceDvrUppercaseIdentifierNames;
        }

        public string GetDvrFileContent(DvrCodeGenModel codeGenModel)
        {
            var templateResourcesPath = "Geryon/DvrScriptTemplate_2";
            var classTemplateAsset = Resources.Load<TextAsset>(templateResourcesPath);
            if (classTemplateAsset == null)
                throw new FileNotFoundException($"{templateResourcesPath}.txt was not found");
            var textContent = classTemplateAsset.text;
            textContent = ReplaceTagWithIndentedMultiline(textContent, IOS_FIELDS_TAG
                , codeGenModel.IOSFields.Select(GetFieldDeclaration));
            textContent = ReplaceTagWithIndentedMultiline(textContent, ANDROID_FIELDS_TAG
                , codeGenModel.AndroidFields.Select(GetFieldDeclaration));
            textContent = ReplaceTagWithIndentedMultiline(textContent, UNSUPPORTED_FIELDS_TAG
                , codeGenModel.UnsupportedFields.Select(GetUnsupportedFieldDeclaration));
            return textContent;
        }

        private string GetFieldDeclaration(DvrField field)
        {
            var collectionFieldName = field.TypeDefinition.DatabaseCollectionFieldName;
            return
                $"{GetPropertyDeclarationWithArrowAccessor(field)} {typeof(Config).FullName}.DvrDatabase.{collectionFieldName}.GetOrCreate(\"{field.Key}\", {field.ValueLiteral});";
        }

        private string GetUnsupportedFieldDeclaration(DvrField field)
        {
            return $"{GetPropertyDeclarationWithArrowAccessor(field)} new {ObservableTypeName}<{field.TypeName}>();";
        }

        private string GetPropertyDeclarationWithArrowAccessor(DvrField field)
        {
            var identifier = forceDvrUppercaseIdentifierNames
                ? field.Name.ToUpperInvariant()
                : CodeStyleUtils.ToValidDvrIdentifier(field.Name);
            
            return $"public static {ObservableTypeName}<{field.TypeName}> {identifier} => ";
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

        private static string GetFullNameWithoutGenericArity(Type t)
        {
            var name = t.FullName;
            var index = name.IndexOf('`');
            return index == -1 ? name : name.Substring(0, index);
        }
    }
}