using UnityEngine;

namespace Internal.Core
{
    public abstract class Lazyingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T THIS
        {
            get => instance;
            set => instance = value;
        }
        private static T instance;
    }
}
