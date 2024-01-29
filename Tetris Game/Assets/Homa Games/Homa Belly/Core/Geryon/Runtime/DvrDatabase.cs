using System;
using System.Collections.Generic;
using System.Globalization;
using HomaGames.HomaBelly;

namespace HomaGames.Geryon
{
    public class DvrDatabase
    {
        public DvrCollection<bool> Booleans { get; } = new DvrCollection<bool>();
        public DvrCollection<int> Ints { get; } = new DvrCollection<int>();
        public DvrCollection<double> Doubles { get; } = new DvrCollection<double>();
        public DvrCollection<string> Strings { get; } = new DvrCollection<string>();

        public int Count => Booleans.Count + Ints.Count + Doubles.Count + Strings.Count;

        /// <summary>
        /// key: B_EASIER
        /// value: true
        /// </summary>
        /// <param name="parameters"></param>
        public void Update(Dictionary<string, object> parameters)
        {
            if (parameters == null)
                return;
            try
            {
                foreach (var pair in parameters)
                {
                    // Obtain the variable key and the variable type (flag)
                    var key = pair.Key.ToUpperInvariant();
                    var prefixFlag = key.Substring(0, 2);

                    if (!DvrTypeDefinition.TryGet(prefixFlag, out var type))
                    {
                        HomaGamesLog.Warning(
                            $"Cannot recognize standard type {pair.Value.GetType()} : please get in touch with your publishing manager.");
                        continue;
                    }

                    SetCollectionParameter(pair.Key, pair.Value, type);
                }
            }
            catch (Exception e)
            {
                HomaGamesLog.Error($"There was an error trying to update DVR database: {e.Message}");
            }
        }

        private void SetCollectionParameter(string key, object value, DvrTypeDefinition typeDefinition)
        {
            if (typeDefinition == null || !DvrTypeDefinition.SupportedTypes.Contains(typeDefinition))
                return;

            var invariantCulture = CultureInfo.InvariantCulture;
            key = key.ToUpperInvariant();
            var legacyKey = key.ToUpperInvariant();

            if (typeDefinition == DvrTypeDefinition.Boolean)
            {
                var val = Convert.ToBoolean(value, invariantCulture);
                Booleans.AddOrSet(key, val);
                DynamicVariable<bool>.Set(legacyKey, val);
            }
            else if (typeDefinition == DvrTypeDefinition.Integer)
            {
                var val = Convert.ToInt32(value, invariantCulture);
                Ints.AddOrSet(key, val);
                DynamicVariable<int>.Set(legacyKey, val);
            }
            else if (typeDefinition == DvrTypeDefinition.Double)
            {
                var val = Convert.ToDouble(value, invariantCulture);
                Doubles.AddOrSet(key, val);
                DynamicVariable<double>.Set(legacyKey, val);
            }
            else if (typeDefinition == DvrTypeDefinition.String)
            {
                var val = value.ToString();
                Strings.AddOrSet(key, val);
                DynamicVariable<string>.Set(legacyKey, val);
            }
        }
    }
}