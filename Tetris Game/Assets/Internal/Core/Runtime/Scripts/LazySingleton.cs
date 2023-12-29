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
        
        [System.Diagnostics.Conditional("LOG")]
        protected void Log(object o)
        {
            Debug.Log(o.ToString());
        }
        
        [System.Diagnostics.Conditional("LOG")]
        protected void LogError(object o)
        {
            Debug.LogError(o.ToString());
        }
    }
}
