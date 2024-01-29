using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;

namespace HomaGames.HomaBelly.Utilities
{
    /// <summary>
    /// This object is a wrapper for dictionaries
    /// returned by <see cref="Json.Deserialize(string)"/>.
    /// </summary>
    public class JsonObject : JsonDataBase<string>
    {
        [NotNull]
        public static JsonObject Empty => new JsonObject(new Dictionary<string, object>());
        
        public int Count => Data.Count;
        
        [NotNull]
        private readonly IDictionary<string, object> Data;

        public JsonObject([NotNull] IDictionary<string, object> data)
        {
            Data = data;
        }
        
        [PublicAPI, NotNull]
        public Dictionary<string, object> ToRawData() => 
            Data.ToDictionary(entry => entry.Key, entry => entry.Value);

        public bool ContainsKey(string key)
        {
            return Data.ContainsKey(key);
        }
        
        protected override bool InnerTryGetCanBeNull<T>(string key, [CanBeNull] out T value)
        {
            if (Data.TryGetValue(key, out object objectDataValue))
            {
                return JsonConversionHelper.TryConvertTo(objectDataValue, out value);
            }

            value = default;
            return false;
        }

        public static explicit operator JsonObject(Dictionary<string, object> dict) =>
            dict == null ? null : new JsonObject(dict);
    }

    public abstract class JsonDataBase<TKey>
    {
        /// <summary>
        /// Try to obtain a value identified by key. If the key
        /// exists but the value is null, this method will return true, and the
        /// value will be set to null. If the value is not of type T, this method
        /// will return false.
        /// </summary>
        /// <param name="key">The key identifying the object</param>
        /// <param name="value">The nullable output of type T</param>
        /// <typeparam name="T">The type of the output</typeparam>
        /// <returns>True if the object was successfully retrieved, false otherwise</returns>
        [PublicAPI]
        public bool TryGetCanBeNull<T>([NotNull] TKey key, [CanBeNull] out T value)
        {
            if (Nullable.GetUnderlyingType(typeof(T)) == null && ! typeof(T).IsClass)
                throw new InvalidOperationException("Type parameter is not nullable or reference type.");
            
            return InnerTryGetCanBeNull(key, out value);
        }


        /// <summary>
        /// Try to obtain a value identified by key. If the key
        /// exists but the value is null, this method will return true, and
        /// the delegate will be called with null. If the value is not of type T, this method
        /// will return false.
        /// </summary>
        /// <param name="key">The key identifying the object</param>
        /// <param name="valueSetter">Action to be invoked with the obtained object. This will not
        /// be invoked if the object could not be obtained</param>
        /// <typeparam name="T">The type of the output</typeparam>
        /// <returns>True if the object was successfully retrieved, false otherwise</returns>
        [PublicAPI]
        public bool TryGetCanBeNull<T>([NotNull] TKey key, [InstantHandle] Action<T> valueSetter)
        {
            if (InnerTryGetCanBeNull(key, out T value))
            {
                valueSetter.Invoke(value);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Try to obtain a value identified by key. If the value is null,
        /// or not of type T, this method will return false.
        /// </summary>
        /// <param name="key">The key identifying the object</param>
        /// <param name="value">The nullable output of type T</param>
        /// <typeparam name="T">The type of the output</typeparam>
        /// <returns>True if the object was successfully retrieved, false otherwise</returns>
        [PublicAPI]
        public bool TryGetNotNull<T>([NotNull] TKey key, out T value)
        {
            return InnerTryGetCanBeNull(key, out value) && value != null;
        }
        
        /// <summary>
        /// Try to obtain a value identified by key. If the value is null,
        /// or the value is not of type T, this method will return false,
        /// and the delegate will not be called.
        /// </summary>
        /// <param name="key">The key identifying the object</param>
        /// <param name="valueSetter">Action to be invoked with the obtained object. This will not
        /// be invoked if the object could not be obtained</param>
        /// <typeparam name="T">The type of the output</typeparam>
        /// <returns>True if the object was successfully retrieved, false otherwise</returns>
        [PublicAPI]
        public bool TryGetNotNull<T>([NotNull] TKey key, [InstantHandle] Action<T> valueSetter)
        {
            if (TryGetNotNull(key, out T value))
            {
                valueSetter.Invoke(value);
                return true;
            }

            return false;
        }

        protected abstract bool InnerTryGetCanBeNull<T>([NotNull] TKey key, [CanBeNull] out T value);
    }

    /// <summary>
    /// This object is a wrapper for lists
    /// returned by <see cref="Json.Deserialize(string)"/>.
    /// </summary>
    public class JsonList : JsonDataBase<int>
    {
        [NotNull]
        public static JsonList Empty => new JsonList(new List<object>());
        
        [NotNull]
        private readonly IList<object> Data;

        public JsonList([NotNull] IList<object> data)
        {
            Data = data;
        }

        public int Count => Data.Count;

        [PublicAPI, NotNull]
        public List<object> ToRawData() => Data.ToList();

        protected override bool InnerTryGetCanBeNull<T>(int key, out T value)
        {
            if (key < 0 || key > Data.Count)
            {
                value = default;
                return false;
            }

            object objectValue = Data[key];

            return JsonConversionHelper.TryConvertTo(objectValue, out value);
        }
        
        public static explicit operator JsonList(List<object> list) =>
            list == null ? null : new JsonList(list);
    }

    internal static class JsonConversionHelper
    {
        public static bool TryConvertTo<T>(object source, [CanBeNull] out T output)
        {
            try
            {
                if (source is T || CanBeAffectedNull(typeof(T)) && source == null)
                {
                    output = (T) source;
                    return true;
                }

                if (source is Dictionary<string, object> sourceDictionary && typeof(T) == typeof(JsonObject))
                {
                    output = (T) (object) (JsonObject) sourceDictionary;
                    return true;
                }

                if (source is List<object> sourceList && typeof(T) == typeof(JsonList))
                {
                    output = (T) (object) (JsonList) sourceList;
                    return true;
                }
                    
                // If we don't test the numeric nature, then 42 can be converted to true
                if (source is IConvertible && IsNumericType(source.GetType()) && IsNumericType(typeof(T)))
                {
                    T convertedValue = (T) Convert.ChangeType(source, typeof(T), CultureInfo.InvariantCulture);
                    if (convertedValue != null)
                    {
                        output = convertedValue;
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                // Ignored
            }

            output = default;
            return false;
        }

        private static bool CanBeAffectedNull(Type type)
            => !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
        
        private static HashSet<Type> NumericTypes = new HashSet<Type>
        {
            typeof(byte),
            typeof(sbyte),
            
            typeof(ushort),
            typeof(uint),
            typeof(ulong),
            
            typeof(short),
            typeof(int),
            typeof(long),
            
            typeof(double),
            typeof(decimal),
            typeof(float),
        };

        private static bool IsNumericType(Type type)
        {
            return NumericTypes.Contains(type) ||
                   NumericTypes.Contains(Nullable.GetUnderlyingType(type));
        }

    }
}

