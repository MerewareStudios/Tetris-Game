using Internal.Core;
using System.Collections.Generic;
using UnityEngine;
using Lean.Pool;

public class PoolManager : Singleton<PoolManager>
{
#if UNITY_EDITOR
    [SerializeField] public bool debug = true;
#endif
    [SerializeField] public List<PoolData> pools;

    void Awake()
    {
        GeneratePool();
    }

    private void GeneratePool()
    {
        for (int i = 0; i < pools.Count; i++)
        {
            PoolData poolData = pools[i];
            GameObject go = new GameObject(poolData.gameObject.name + " Pool");
            go.hideFlags = HideFlags.HideInHierarchy;
            go.transform.SetParent(null);
            LeanGameObjectPool leanGameObjectPool = go.AddComponent<LeanGameObjectPool>();
            leanGameObjectPool.Prefab = poolData.gameObject;
        #if UNITY_EDITOR
            leanGameObjectPool.Warnings = true;
        #else
            leanGameObjectPool.Warnings = false;
        #endif
            
#if UNITY_EDITOR
            go.hideFlags = debug ? HideFlags.None : HideFlags.HideInHierarchy;
#endif
    
            leanGameObjectPool.Notification = LeanGameObjectPool.NotificationType.None;
            leanGameObjectPool.Strategy = LeanGameObjectPool.StrategyType.DeactivateViaHierarchy;
            leanGameObjectPool.Preload = poolData.preload;
            leanGameObjectPool.Capacity = poolData.capacity;
            leanGameObjectPool.Recycle = false;
            leanGameObjectPool.Persist = false;

            poolData.pool = leanGameObjectPool;
        }
    }

    #region Spawn
    public static GameObject Prefab(Pool key)
    {
        return PoolManager.THIS.pools[((int)key)].gameObject;
    }
    public static GameObject Spawn(Pool key, Transform parent = null)
    {
        return PoolManager.THIS.pools[((int)key)].pool.Spawn(parent);
    }
    public static T Spawn<T>(Pool key, Transform parent = null) where T : Component
    {
        return PoolManager.THIS.pools[(int)key].pool.Spawn(parent).GetComponent<T>();
    }
    public static void Despawn(Pool key, GameObject gameObject)
    {
        PoolManager.THIS.pools[((int)key)].pool.Despawn(gameObject);
    }
    #endregion

    [System.Serializable]
    public class PoolData
    {
        [SerializeField] public GameObject gameObject;
        [SerializeField] public int preload = 0;
        [SerializeField] public int capacity = 50;
        [System.NonSerialized] public LeanGameObjectPool pool;
    }
}

public static class PoolManagerExtensions
{
    public static T Prefab<T>(this Pool key)
    {
        return PoolManager.Prefab(key).GetComponent<T>();
    }
    public static GameObject Spawn(this Pool key, Transform parent = null)
    {
        return PoolManager.Spawn(key, parent);
    }
    public static T Spawn<T>(this Pool key, Transform parent = null) where T : Component
    {
        return PoolManager.Spawn<T>(key, parent);
    }
    public static void Despawn(this GameObject gameObject, Pool key)
    {
        PoolManager.Despawn(key, gameObject);
    }
    public static void Despawn(this GameObject gameObject, int key)
    {
        PoolManager.Despawn((Pool)key, gameObject);
    }
    public static void Despawn(this GameObject gameObject)
    {
        PoolManager.Despawn((Pool)System.Enum.Parse(typeof(Pool), gameObject.name.Replace(" ", "_").Replace("-", "_")), gameObject);
    }
    public static void Despawn(this Transform transform)
    {
        PoolManager.Despawn((Pool)System.Enum.Parse(typeof(Pool), transform.name.Replace(" ", "_").Replace("-", "_")), transform.gameObject);
    }
    public static void Despawn(this MonoBehaviour mono)
    {
        PoolManager.Despawn((Pool)System.Enum.Parse(typeof(Pool), mono.gameObject.name.Replace(" ", "_").Replace("-", "_")), mono.gameObject);
    }
    public static void Despawn(this MonoBehaviour mono, Pool key)
    {
        PoolManager.Despawn(key, mono.gameObject);
    }
    public static void Despawn(this int key, GameObject gameObject)
    {
        PoolManager.Despawn((Pool)key, gameObject);
    }
    public static void Despawn(this Pool key, GameObject gameObject)
    {
        PoolManager.Despawn(key, gameObject);
    }
    public static Pool ToPoolKey(this string name)
    {
        return (Pool)System.Enum.Parse(typeof(Pool), name);
    }
}

#if UNITY_EDITOR
namespace  Game.Editor
{
    using UnityEditor;
    using Internal.Core;

    [CustomEditor(typeof(PoolManager))]
    [CanEditMultipleObjects]
    public class PoolManagerGUI : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button(new GUIContent("REFRESH", "Convert to hard coded indexes.")))
            {
                AutoGenerate.GeneratePool();
            }
            DrawDefaultInspector();
        }
    }
}
#endif