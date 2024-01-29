using System.Linq;
using UnityEngine;

namespace HomaGames.Geryon
{
    /// <summary>
    /// Deprecated: Use the <see cref="DvrDatabase"/> instead.
    /// Class representing a dynamic variable. This class can only host bool, int, double or string values.
    /// </summary>
    /// <typeparam name="T">The data type of the variable</typeparam>
    public static class DynamicVariable<T>
    {
        static DynamicVariable()
        {
            if (DvrTypeDefinition.SupportedTypes.All(t => t.ValueType != typeof(T)))
                throw new InvalidTypeParameterException(
                    $"{typeof(DynamicVariable<>).Name} can only be used with the given types: " +
                    $"{string.Join(", ", DvrTypeDefinition.SupportedTypes.Select(t => t.ValueType.Name))}");
        }

        private static readonly DvrCollection<T> DvrCollection = new DvrCollection<T>();

        public static T Get(string key, T defaultValue)
        {
            // Check for isPlaying to allow Unit testing
            if (Config.Initialized || !Application.isPlaying)
                return DvrCollection.TryGet(key, out var dvr) ? dvr.Value : defaultValue;
            Debug.LogWarning($"You're trying to access {key} N-Testing value before N-Testing is initialised.");
            return defaultValue;
        }

        /// <summary>
        /// Try getting a N-Testing value from a key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>Returns true if the value is available.</returns>
        public static bool TryGet(string key, out T value)
        {
            // Check for isPlaying to allow Unit testing
            if ((Config.Initialized || !Application.isPlaying) && DvrCollection.TryGet(key, out var observable))
            {
                value = observable.Value;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Updates the dynamic variable referenced by `key`, if it exists.
        /// If not, adds it to the dictionary
        /// </summary>
        /// <param name="key">The key referencing the dynamic variable</param>
        /// <param name="value">The new value</param>
        public static void Set(string key, T value)
        {
            DvrCollection.AddOrSet(key, value);
        }

        internal static void Clear()
        {
            DvrCollection.Clear();
        }
    }
}