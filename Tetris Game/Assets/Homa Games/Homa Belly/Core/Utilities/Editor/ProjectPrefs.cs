using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    [FilePath("ProjectSettings/Homa Games/ProjectPrefs.asset", FilePathAttribute.Location.ProjectFolder)]
    public class ProjectPrefs : ScriptableSingleton<ProjectPrefs>
    {
        private const bool SaveAsString = true;
        
        [SerializeField]
        private IntKeyStore IntStore = new IntKeyStore();
        [SerializeField]
        private StringKeyStore StringStore = new StringKeyStore();
        [SerializeField]
        private BoolKeyStore BoolStore = new BoolKeyStore();
        [SerializeField]
        private FloatKeyStore FloatStore = new FloatKeyStore();

        [SerializeField]
        private List<string> ToggleList = new List<string>();

        [PublicAPI]
        private KeyStore<T> GetStore<T>()
        {
            List<object> allStores = new List<object>
            {
                IntStore, StringStore, BoolStore, FloatStore
            };

            foreach (object store in allStores)
            {
                if (store is KeyStore<T> keyStore)
                    return keyStore;
            }

            return null;
        }

        /// <summary>
        /// Retrieves a values from the project preferences
        /// </summary>
        /// <param name="key">The key to search for</param>
        /// <param name="value">The output value</param>
        /// <typeparam name="T">Either <c>bool</c>, <c>int</c>, <c>float</c> or <c>string</c></typeparam>
        /// <returns><c>true</c> if the key is found in the preferences, <c>false</c> otherwise</returns>
        [ContractAnnotation("=>true; =>false, value: null")]
        [Pure]
        [PublicAPI]
        public static bool TryGet<T>([NotNull] string key, out T value)
        {
            return instance.GetStore<T>().TryGet(key, out value);
        }

        /// <summary>
        /// Sets a value in the project preferences
        /// </summary>
        /// <param name="key">The key to search for</param>
        /// <param name="value">The value to associate to the key</param>
        /// <typeparam name="T">Either <c>bool</c>, <c>int</c>, <c>float</c> or <c>string</c></typeparam>
        [PublicAPI]
        public static void Set<T>([NotNull] string key, [CanBeNull] T value)
        {
            instance.GetStore<T>().Set(key, value);
            instance.Save(SaveAsString);
        }
        

        /// <summary>
        /// Unsets a value in the project preferences
        /// </summary>
        /// <param name="key">The key to remove the associated value to</param>
        /// <typeparam name="T">Either <c>bool</c>, <c>int</c>, <c>float</c> or <c>string</c></typeparam>
        [PublicAPI]
        public static void Unset<T>([NotNull] string key)
        {
            instance.GetStore<T>().Unset(key);
            instance.Save(SaveAsString);
        }
        
        /// <summary>
        /// Gives the state of a toggle
        /// </summary>
        /// <param name="key">The toggle's key</param>
        /// <returns><c>true</c> if the toggle is set, <c>false</c> otherwise</returns>
        [PublicAPI]
        [Pure]
        public static bool IsToggleSet([NotNull] string key)
        {
            if (instance.ToggleList.Contains(key))
                return true;

            if (BackwardCompatibility.GetToggleCompat(key))
            {
                instance.ToggleList.Add(key);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets a toggle
        /// </summary>
        /// <param name="key">The toggle's key</param>
        [PublicAPI]
        public static void SetToggle([NotNull] string key)
        {
            if (! IsToggleSet(key))
            {
                instance.ToggleList.Add(key);
                instance.Save(SaveAsString);
            }
        }

        /// <summary>
        /// Unsets a toggle
        /// </summary>
        /// <param name="key">The toggle's key</param>
        [PublicAPI]
        public static void UnsetToggle([NotNull] string key)
        {
            instance.ToggleList.Remove(key);
            instance.Save(SaveAsString);
        }

        [Serializable]
        private class IntKeyStore : KeyStore<int> {}
        [Serializable]
        private class StringKeyStore : KeyStore<string> {}
        [Serializable]
        private class BoolKeyStore : KeyStore<bool> {}
        [Serializable]
        private class FloatKeyStore : KeyStore<float> {}

        [Serializable]
        private class KeyStore<T>
        {
            [SerializeField]
            private List<string> Keys = new List<string>();
            [SerializeField]
            private List<T> Values = new List<T>();

            public bool TryGet([NotNull] string key, [CanBeNull] out T value)
            {
                int index = Keys.IndexOf(key);
                value = default;

                if (index < 0)
                {
                    if (BackwardCompatibility.TryGetCompat(key, out value))
                    {
                        Set(key, value);
                        return true;
                    }

                    return false;
                }

                value = Values[index];
                return true;
            }

            public void Set([NotNull] string key, [CanBeNull] T value)
            {
                int index = Keys.IndexOf(key);

                if (index < 0)
                {
                    Keys.Add(key);
                    Values.Add(value);
                }
                else
                {
                    Values[index] = value;
                }
            }

            public void Unset([NotNull] string key)
            {
                int index = Keys.IndexOf(key);

                if (index >= 0)
                {
                    Keys.RemoveAt(index);
                    Values.RemoveAt(index);
                }
            }
        }

        private static class BackwardCompatibility
        {
            [ContractAnnotation("=>true; =>false, value: null")]
            public static bool TryGetCompat<T>([NotNull] string key, [CanBeNull] out T value)
            {
                value = default;
                
                if (! EditorPrefs.HasKey(key))
                    return false;

                if (typeof(T) == typeof(string))
                    value = (T)(object)EditorPrefs.GetString(key);
                
                else if (typeof(T) == typeof(int))
                    value = (T)(object)EditorPrefs.GetInt(key);
                
                else if (typeof(T) == typeof(bool))
                    value = (T)(object)EditorPrefs.GetBool(key);
                
                else if (typeof(T) == typeof(float))
                    value = (T)(object)EditorPrefs.GetFloat(key);

                return ! (value?.Equals(default(T)) ?? false);
            }

            public static bool GetToggleCompat([NotNull] string key)
            {
                return
                    TryGetCompat(key, out bool b) && b
                    || TryGetCompat(key, out int i) && i != 0;
            }
        }
    }
}
