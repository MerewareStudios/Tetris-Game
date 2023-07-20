using UnityEngine;

namespace Internal.Core
{
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T THIS
        {
            get
            {
                if (!instance)
                {
                    instance = FindObjectOfType<T>();
                }
                return instance;
            }
            private set => instance = value;
        }
        private static T instance;
    }
}
