using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Globalization;
using System.IO;
using HomaGames.Geryon.Editor;
using HomaGames.HomaBelly;

namespace HomaGames.Geryon.Editor.CodeGen
{
    internal readonly struct DvrField
    {
        private static readonly CodeDomProvider CsharpCodeDomProvider = CodeDomProvider.CreateProvider("Csharp");
        
        public readonly string Name;
        public readonly string Key;
        public readonly object Value;
        public readonly DvrTypeDefinition TypeDefinition;

        public string TypeName => TypeDefinition.ValueType.Name;

        public string ValueLiteral
        {
            get
            {
                using var writer = new StringWriter();
                CsharpCodeDomProvider.GenerateCodeFromExpression(new CodePrimitiveExpression(Value), writer, null);
                return writer.ToString();
            }
        }

        public DvrField(string configKey, object value)
        {
            Key = configKey.ToUpperInvariant();
            var flag = Key.Substring(0, 2);
            if (!DvrTypeDefinition.TryGet(flag, out TypeDefinition))
                throw new InvalidDvrTypeException();

            Name = configKey.Substring(2);
            Value = Convert.ChangeType(value, TypeDefinition.ValueType, CultureInfo.InvariantCulture);
        }
    }
}