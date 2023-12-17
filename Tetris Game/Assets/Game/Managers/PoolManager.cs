using Internal.Core;
using System.Collections.Generic;
using UnityEngine;
using Lean.Pool;

public class PoolManager : Singleton<PoolManager>
{
#if UNITY_EDITOR
    [SerializeField] private bool debug = true;
#endif
    [SerializeField] public List<PoolData> pools;

    #region Spawn
    public static GameObject Prefab(Pool key)
    {
        return PoolManager.THIS.pools[((int)key)].gameObject;
    }
    public static GameObject Prefab(int key)
    {
        return PoolManager.THIS.pools[key].gameObject;
    }
    
    public static GameObject Spawn(Pool key, Transform parent = null)
    {
        return PoolManager.THIS.pools[((int)key)].Pool.Spawn(parent);
    }
    public static T Spawn<T>(Pool key, Transform parent = null) where T : Component
    {
        return PoolManager.THIS.pools[((int)key)].Pool.Spawn(parent).GetComponent<T>();
    }
    public static void Despawn(Pool key, GameObject gameObject)
    {
        PoolManager.THIS.pools[((int)key)].Pool.Despawn(gameObject);
    }
    public static void Despawn(int key, GameObject gameObject)
    {
        PoolManager.THIS.pools[key].Pool.Despawn(gameObject);
    }
    #endregion

    [System.Serializable]
    public class PoolData
    {
        [SerializeField] public GameObject gameObject;
        [SerializeField] public bool readOnly = false;
        [SerializeField] public int preload = 0;
        [SerializeField] public int capacity = 50;
        [System.NonSerialized] private LeanGameObjectPool _pool;

        public LeanGameObjectPool Pool
        {
            get
            {
#if UNITY_EDITOR
                if (readOnly)
                {
                    Debug.LogError("This pool is readonly, cannot spawn!");
                    return null;
                }
#endif
                if (!this._pool)
                {
                    Instantiate();
                }
                return _pool;
            }
        }

        private void Instantiate()
        {
             
        }
    }
}

public static class PoolManagerExtensions
{
    public static T Prefab<T>(this Pool key)
    {
        return PoolManager.Prefab(key).GetComponent<T>();
    }
    public static T Prefab<T>(this int key)
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
    public static void Despawn(this MonoBehaviour mono, Pool key)
    {
        mono.gameObject.Despawn(key);
    }
    public static void Despawn(this Transform t, Pool key)
    {
        t.gameObject.Despawn(key);
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