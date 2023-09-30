using Internal.Core;
using System.Collections.Generic;
using UnityEngine;


public class ParticleManager : Singleton<ParticleManager>
{
#if UNITY_EDITOR
    [SerializeField] public bool debug = true;
#endif
    [SerializeField] public List<ParticleSystem> particleSystems;
    [System.NonSerialized] public List<ParticleData> particleData = new();

    void Awake()
    {
        GeneratePool();
    }

    private void GeneratePool()
    {
        for (int i = 0; i < particleSystems.Count; i++)
        {
            particleData.Add(new ParticleData());
        }
    }

    #region Play-Emit
    public static ParticleSystem Play(Particle key, Vector3 position = default, Quaternion rotation = default, Vector3? scale = null, ParticleSystemStopAction particleSystemStopAction = ParticleSystemStopAction.Destroy)
    {
        int index = (int)key;
        ParticleSystem particleSystem = MonoBehaviour.Instantiate(ParticleManager.THIS.particleSystems[index], null);
        particleSystem.name = ParticleManager.THIS.particleSystems[index].name;
        particleSystem.gameObject.hideFlags = HideFlags.HideInHierarchy;
        
#if UNITY_EDITOR
        particleSystem.gameObject.hideFlags = ParticleManager.THIS.debug ? HideFlags.None : HideFlags.HideInHierarchy;
#endif

        Transform pTransform = particleSystem.transform;
        pTransform.position = position;
        pTransform.rotation = rotation;
        pTransform.localScale = scale == null ? Vector3.one : scale.Value;

        var main = particleSystem.main;
        main.stopAction = particleSystemStopAction;

        particleSystem.Play();

        return particleSystem;
    }
    public static ParticleSystem Emit(Particle key, int amount, Color? color = null, Vector3 position = default, Quaternion rotation = default, Vector3? scale = null)
    {
        int index = (int)key;
        ref ParticleSystem particleSystem = ref ParticleManager.THIS.particleData[index].emitInstance;
        if (!particleSystem)
        {
            particleSystem = MonoBehaviour.Instantiate(ParticleManager.THIS.particleSystems[index], null);
            particleSystem.name = ParticleManager.THIS.particleSystems[index].name;
            particleSystem.gameObject.hideFlags = HideFlags.HideInHierarchy;
            
#if UNITY_EDITOR
            particleSystem.gameObject.hideFlags = ParticleManager.THIS.debug ? HideFlags.None : HideFlags.HideInHierarchy;
#endif
        }

        Transform pTransform = particleSystem.transform;
        pTransform.position = position;
        pTransform.rotation = rotation;
        pTransform.localScale = scale == null ? Vector3.one : scale.Value;

        var main = particleSystem.main;

        if (color != null)
        {
            main.startColor = (Color)color;
        }
        particleSystem.Emit(amount);

        return particleSystem;
    }
    
    public static ParticleSystem EmitForward(Particle key, int amount, Vector3 position, Vector3? forward = null, Vector3? scale = null, Color? color = null)
    {
        int index = (int)key;
        ref ParticleSystem particleSystem = ref ParticleManager.THIS.particleData[index].emitInstance;
        if (!particleSystem)
        {
            particleSystem = MonoBehaviour.Instantiate(ParticleManager.THIS.particleSystems[index], null);
            particleSystem.name = ParticleManager.THIS.particleSystems[index].name;
            particleSystem.gameObject.hideFlags = HideFlags.HideInHierarchy;
            
#if UNITY_EDITOR
            particleSystem.gameObject.hideFlags = ParticleManager.THIS.debug ? HideFlags.None : HideFlags.HideInHierarchy;
#endif
        }
    
        Transform pTransform = particleSystem.transform;
        pTransform.position = position;
        pTransform.forward = forward ?? pTransform.forward;
        pTransform.localScale = scale ?? pTransform.localScale;
    
        var main = particleSystem.main;
    
        if (color != null)
        {
            main.startColor = color.Value;
        }
        particleSystem.Emit(amount);
    
        return particleSystem;
    }
    public static void StopAndClear(Particle key)
    {
        int index = (int)key;
        ref ParticleSystem particleSystem = ref ParticleManager.THIS.particleData[index].emitInstance;
        if (particleSystem == null)
        {
            return;
        }

        particleSystem.Stop();
        particleSystem.Clear();
    }
    public static ParticleSystem Emit(Particle key, int amount, Color color, Vector3 position, float radius)
    {
        int index = ((int)key);
        ref ParticleSystem particleSystem = ref ParticleManager.THIS.particleData[index].emitInstance;
        if (!particleSystem)
        {
            particleSystem = MonoBehaviour.Instantiate(ParticleManager.THIS.particleSystems[index], null);
            particleSystem.name = ParticleManager.THIS.particleSystems[index].name;
            particleSystem.gameObject.hideFlags = HideFlags.HideInHierarchy;
            
#if UNITY_EDITOR
            particleSystem.gameObject.hideFlags = ParticleManager.THIS.debug ? HideFlags.None : HideFlags.HideInHierarchy;
#endif
        }

        Transform pTransform = particleSystem.transform;
        pTransform.position = position;

        var main = particleSystem.main;
        var shape = particleSystem.shape;

        main.startColor = color;
        shape.radius = radius;
        
        particleSystem.Emit(amount);

        return particleSystem;
    }
    #endregion

    public class ParticleData
    {
        [SerializeField] public ParticleSystem emitInstance;
        [System.NonSerialized] public List<ParticleSystem> particleSystemInstances = new();
    }
    [System.Serializable]
    public struct ParticleEmissionData
    {
        [SerializeField] public Particle particle;
        [SerializeField] public int amount;
    }
    [System.Serializable]
    public struct ParticlePlayData
    {
        [SerializeField] public Particle particle;
        [SerializeField] public ParticleSystemStopAction particleSystemStopAction;
    }
}
public static class ParticleManagerExtensions
{
    public static ParticleSystem EmitForward(this Particle key, int amount, Vector3 position, Vector3? forward = null, Vector3? scale = null, Color? color = null)
    {
        return ParticleManager.EmitForward(key, amount, position, forward, scale, color);
    }
    public static ParticleSystem Emit(this Particle key, int amount, Vector3 position = default, Quaternion rotation = default, Vector3? scale = null)
    {
        return ParticleManager.Emit(key, amount, null, position, rotation, scale);
    }
    public static ParticleSystem Emit(this Particle key, int amount, Color color, Vector3 position = default, Quaternion rotation = default, Vector3? scale = null)
    {
        return ParticleManager.Emit(key, amount, color, position, rotation, scale);
    }
    public static ParticleSystem Emit(this Particle key, int amount, Vector3 position, Color color)
    {
        return ParticleManager.Emit(key, amount, color, position);
    }
    public static ParticleSystem Emit(this Particle key, int amount, Vector3 position, Color color, float radius)
    {
        return ParticleManager.Emit(key, amount, color, position, radius);
    }
    public static void EmitAll(this Particle[] keys, int amount, Vector3 position = default, Quaternion rotation = default, Vector3? scale = null)
    {
        foreach (var key in keys)
        {
            key.Emit(amount, position, rotation, scale);
        }
    }
    public static void EmitAll(this ParticleManager.ParticleEmissionData[] emissionDatas, Vector3 position = default, Quaternion rotation = default, Vector3? scale = null)
    {
        foreach (var particleEmissionData in emissionDatas)
        {
            particleEmissionData.particle.Emit(particleEmissionData.amount, position, rotation, scale);
        }
    }
    public static ParticleSystem Play(this Particle key, Vector3 position = default, Quaternion rotation = default, Vector3? scale = null, ParticleSystemStopAction particleSystemStopAction = ParticleSystemStopAction.Destroy)
    {
        return ParticleManager.Play(key, position, rotation, scale, particleSystemStopAction);
    }

    public static void Play(this ParticleManager.ParticlePlayData playData, Vector3 position = default, Quaternion rotation = default, Vector3? scale = null)
    {
        playData.particle.Play(position, rotation, scale, playData.particleSystemStopAction);
    }
    
    public static void StopAndClear(this Particle key)
    {
        ParticleManager.StopAndClear(key);
    }
}