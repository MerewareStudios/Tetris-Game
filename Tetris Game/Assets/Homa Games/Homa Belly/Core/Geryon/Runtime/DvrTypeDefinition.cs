using System;
using System.Collections.Generic;
using System.Linq;

namespace HomaGames.Geryon
{
    internal class DvrTypeDefinition
    {
        public static readonly HashSet<DvrTypeDefinition> SupportedTypes = new HashSet<DvrTypeDefinition>(new[]
        {
            Boolean = new DvrTypeDefinition("B_", typeof(bool), nameof(DvrDatabase.Booleans)),
            Integer = new DvrTypeDefinition("I_", typeof(int), nameof(DvrDatabase.Ints)),
            Double = new DvrTypeDefinition("F_", typeof(double), nameof(DvrDatabase.Doubles)),
            String = new DvrTypeDefinition("S_", typeof(string), nameof(DvrDatabase.Strings))
        });

        public static readonly DvrTypeDefinition Boolean;
        public static readonly DvrTypeDefinition Integer;
        public static readonly DvrTypeDefinition Double;
        public static readonly DvrTypeDefinition String;

        public readonly Type ValueType;
        public readonly string DatabaseCollectionFieldName;
        private readonly string _prefixFlag;

        public static bool TryGet(string prefixFlag, out DvrTypeDefinition dvrTypeDefinition)
        {
            dvrTypeDefinition = SupportedTypes.FirstOrDefault(t => t._prefixFlag == prefixFlag.ToUpperInvariant());
            return dvrTypeDefinition != null;
        }

        private DvrTypeDefinition(string prefixFlag, Type valueType, string databaseCollectionFieldName)
        {
            _prefixFlag = prefixFlag;
            ValueType = valueType;
            DatabaseCollectionFieldName = databaseCollectionFieldName;
        }
    }
}