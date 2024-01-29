using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace HomaGames.Geryon.Editor.CodeGen
{
    internal static class CodeStyleUtils
    {
        /// <summary>
        /// Converts a string to a valid C# identifier. UpperCamelCase, starting with a letter, and only containing letters, numbers and underscores. Prefix is added as suffix if it's not a valid identifier
        /// </summary>
        public static string ToValidDvrIdentifier(string original)
        {
            var invalidCharsRgx = new Regex("[^_a-zA-Z0-9]");
            var startsWithLowerCaseChar = new Regex("^[a-z]");
            var startsWithNotLetter = new Regex("^[^a-zA-Z]+");
            var lowerCaseNextToNumber = new Regex("(?<=[0-9])[a-z]");

            var nonLetterPrefix = startsWithNotLetter.Match(original).Value;
            // remove nonLetterPrefix from the beginning of the original
            original = startsWithNotLetter.Replace(original, string.Empty);
            nonLetterPrefix = invalidCharsRgx.Replace(nonLetterPrefix, string.Empty);

            var pascalCase = string.Concat(
                original.Split(new char[] { '_', '-', '.', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    // replace all invalid chars with empty string
                    .Select(w => invalidCharsRgx.Replace(w, string.Empty))
                    // set first letter to uppercase
                    .Select(w => startsWithLowerCaseChar.Replace(w, m => m.Value.ToUpperInvariant()))
                    // set upper case the first lower case following a number (Ab9cd -> Ab9Cd)
                    .Select(w => lowerCaseNextToNumber.Replace(w, m => m.Value.ToUpperInvariant())));

            var result = pascalCase;
            // append nonLetterPrefix as suffix
            if (!string.IsNullOrEmpty(nonLetterPrefix))
                result = pascalCase + $"_{nonLetterPrefix}".TrimEnd('_');

            return result;
        }
    }
}