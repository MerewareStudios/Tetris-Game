using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleUnit : MonoBehaviour
{
    [System.NonSerialized] private Transform _thisTransform;
    [SerializeField] public Particle key;
    [SerializeField] public ParticleSystem ps;
    [System.NonSerialized] public ParticleSystem.MainModule MainModule;

    #if UNITY_EDITOR
    private void Reset()
    {
        ps = GetComponent<ParticleSystem>();
        key = (Particle)System.Enum.Parse(typeof(Particle), name.Replace(" ", "_"));

    }
    #endif

    void Awake()
    {
        _thisTransform = this.transform;
        MainModule = ps.main;
    }

    public void SetForward(Vector3 forward)
    {
        _thisTransform.forward = forward;
    }
    public void Set(Quaternion rotation, Vector3 scale)
    {
        _thisTransform.rotation = rotation;
        _thisTransform.localScale = scale;
    }
    public void PlayAtPosition(Vector3 position)
    {
        _thisTransform.position = position;
        ps.Play();
    }
    
    public void EmitAtPosition(Vector3 position, int count)
    {
        _thisTransform.position = position;
        ps.Emit(count);
    }
    public void EmitAtPosition(Vector3 position, int count, Color color)
    {
        _thisTransform.position = position;
        MainModule.startColor = color;
        ps.Emit(count);
    }
    void OnParticleSystemStopped()
    {
        // Debug.Log("despawn " + Time.realtimeSinceStartup + " " + this.gameObject.activeInHierarchy, this.gameObject);
        // ps.Stop();
        // this.gameObject.SetActive(false);
        this.Despawn(key);    
    }
}
