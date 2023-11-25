using Internal.Core;
using System.Collections.Generic;
using Lean.Pool;
using UnityEngine;


public class ParticleManager : Singleton<ParticleManager>
{
#if UNITY_EDITOR
    [SerializeField] private bool debug = true;
#endif
    [SerializeField] public List<ParticleUnitData> particleUnitDatas;

    private ParticleUnit Clone(ParticleUnit go)
    {
        return Instantiate(go, this.transform);
    }
    
    public static ParticleUnit Prefab(Particle key)
    {
        return ParticleManager.THIS.particleUnitDatas[((int)key)].particleUnit;
    }
    public static ParticleUnit Prefab(int key)
    {
        return ParticleManager.THIS.particleUnitDatas[key].particleUnit;
    }
    
    public static ParticleUnit Spawn(Particle key)
    {
        return ParticleManager.THIS.particleUnitDatas[((int)key)].New;
    }
    public static ParticleUnit Spawn(int key)
    {
        return ParticleManager.THIS.particleUnitDatas[key].New;
    }
    
    public static void Despawn(Particle key, ParticleUnit particleUnit)
    {
        ParticleManager.THIS.particleUnitDatas[((int)key)].Despawn(particleUnit);
    }
    public static void Despawn(int key, ParticleUnit particleUnit)
    {
        ParticleManager.THIS.particleUnitDatas[key].Despawn(particleUnit);
    }
    
    
    
    [System.Serializable]
    public class ParticleUnitData
    {
        [SerializeField] public ParticleUnit particleUnit;
        [SerializeField] public bool emitOnly = false;
        [SerializeField] public bool asInstance = false;
        [SerializeField] public int preload = 0;
        [SerializeField] public int capacity = 50;
        [System.NonSerialized] private LeanGameObjectPool _pool;
        [System.NonSerialized] private ParticleUnit _emitInstance;

        public ParticleUnit New
        {
            get
            {
#if UNITY_EDITOR
                if (emitOnly)
                {
                    if (!_emitInstance)
                    {
                        InstantiateEmitter();
                    }
                    return _emitInstance;
                }
#endif
                if (!this._pool)
                {
                    InstantiatePool();
                }
                return _pool.Spawn(null).GetComponent<ParticleUnit>();
            }
        }

        public void Despawn(ParticleUnit particleUnit)
        {
#if UNITY_EDITOR
            if (emitOnly)
            {
                Debug.LogError("This pool is emit only, cannot despawn!");
                return;
            }
#endif
            
            _pool.Despawn(particleUnit.gameObject);
        }

        private void InstantiateEmitter()
        {
            if (asInstance)
            {
                _emitInstance = particleUnit;
                return;
            }
            _emitInstance = ParticleManager.THIS.Clone(particleUnit);
        }

        private void InstantiatePool()
        {
            GameObject go = new GameObject(particleUnit.gameObject.name + " Particle Pool");
            go.hideFlags = HideFlags.HideInHierarchy;
            go.transform.SetParent(ParticleManager.THIS.transform);
            _pool = go.AddComponent<LeanGameObjectPool>();
            _pool.Prefab = particleUnit.gameObject;
#if UNITY_EDITOR
            _pool.Warnings = true;
#else
            _pool.Warnings = false;
#endif
            
#if UNITY_EDITOR
            go.hideFlags = ParticleManager.THIS.debug ? HideFlags.None : HideFlags.HideInHierarchy;
#endif
    
            _pool.Notification = LeanGameObjectPool.NotificationType.None;
            _pool.Strategy = LeanGameObjectPool.StrategyType.DeactivateViaHierarchy;
            _pool.Preload = preload;
            _pool.Capacity = capacity;
            _pool.Recycle = false;
            _pool.Persist = false;

            var main = particleUnit.ps.main;
            main.stopAction = emitOnly ? ParticleSystemStopAction.None : ParticleSystemStopAction.Callback;
        }
    }
}
public static class ParticleManagerExtensions
{
    public static ParticleUnit Prefab(this int key)
    {
        return ParticleManager.Prefab(key);
    }
    public static ParticleUnit Spawn(this Particle key)
    {
        return ParticleManager.Spawn(key);
    }
    public static void Despawn(this ParticleUnit particleUnit, Particle key)
    {
        ParticleManager.Despawn(key, particleUnit);
    }
    public static void Play(this Particle key, Vector3 position)
    {
        ParticleUnit particleUnit = ParticleManager.Spawn(key);
        particleUnit.PlayAtPosition(position);
    }
    public static void Play(this Particle key, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        ParticleUnit particleUnit = ParticleManager.Spawn(key);
        particleUnit.Set(rotation, scale);
        particleUnit.PlayAtPosition(position);
    }
    public static void Play(this Particle key, Vector3 position, Vector3 forward)
    {
        ParticleUnit particleUnit = ParticleManager.Spawn(key);
        particleUnit.SetForward(forward);
        particleUnit.PlayAtPosition(position);
    }
    public static void Emit(this Particle key, int amount, Vector3 position)
    {
        ParticleUnit particleUnit = ParticleManager.Spawn(key);
        particleUnit.EmitAtPosition(position, amount);
    }
    public static void Emit(this Particle key, int amount, Vector3 position, Vector3 forward)
    {
        ParticleUnit particleUnit = ParticleManager.Spawn(key);
        particleUnit.SetForward(forward);
        particleUnit.EmitAtPosition(position, amount);
    }
    public static void Emit(this Particle key, int amount, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        ParticleUnit particleUnit = ParticleManager.Spawn(key);
        particleUnit.Set(rotation, scale);
        particleUnit.EmitAtPosition(position, amount);
    }
}