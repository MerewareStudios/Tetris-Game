using UnityEngine;

public class SSingleton<T> : ScriptableObject where T : ScriptableObject
{
    private static T instance = null;

    public static T THIS
    {
        get
        {
            // if (!instance)
            // {
            //     T[] scriptableObjects = Resources.LoadAll<T>("");
            //     if (scriptableObjects.Length > 0)
            //     {
            //         instance = scriptableObjects[0];
            //     }
            //     else
            //     {
            //         Debug.LogError("Singleton instance of type " + typeof(T).Name + " not found in resources.");
            //     }
            // }
            return instance;
        }
        set => instance = value;
    }
}