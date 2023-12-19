using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Internal.Core.Singleton<AudioManager>
{
    
#if UNITY_EDITOR
    [SerializeField] private bool debug = true;
    [SerializeField] private bool onlyPlayDebugSound;
    [SerializeField] private List<Audio> debugSounds;
#endif
    [SerializeField] public float overlapProtectionTime = 0.1f;
    [SerializeField] public List<AudioSourceData> audioSourceDatas;


    public static void PlayOneShot(int key, float volumeScale = 1.0f)
    {
#if UNITY_EDITOR
        if (AudioManager.THIS.onlyPlayDebugSound && !AudioManager.THIS.debugSounds.Contains((Audio)key))
        {
            return;
        }
#endif
        AudioSourceData audioSourceData = AudioManager.THIS.audioSourceDatas[key];
        if (!audioSourceData.CanPlay)
        {
            return;
        }
        AudioSource audioSource = audioSourceData.Instance;
        audioSource.pitch = audioSourceData.RandomPitch;
        audioSource.PlayOneShot(audioSource.clip, volumeScale);
        audioSourceData.LastTimePlayed = Time.realtimeSinceStartup;
    }
    public static void PlayOneShotPitch(int key, float volumeScale = 1.0f, float pitch = 1.0f)
    {
#if UNITY_EDITOR
        if (AudioManager.THIS.onlyPlayDebugSound && !AudioManager.THIS.debugSounds.Contains((Audio)key))
        {
            return;
        }
#endif
        AudioSourceData audioSourceData = AudioManager.THIS.audioSourceDatas[key];
        if (!audioSourceData.CanPlay)
        {
            return;
        }
        AudioSource audioSource = audioSourceData.Instance;
        audioSource.pitch = pitch;
        audioSource.PlayOneShot(audioSource.clip, volumeScale);
        audioSourceData.LastTimePlayed = Time.realtimeSinceStartup;
    }
    public static void Play(int key)
    {
#if UNITY_EDITOR
        if (AudioManager.THIS.onlyPlayDebugSound && !AudioManager.THIS.debugSounds.Contains((Audio)key))
        {
            return;
        }
#endif
        AudioSourceData audioSourceData = AudioManager.THIS.audioSourceDatas[key];
        AudioSource audioSource = audioSourceData.Instance;
        audioSource.Play();
    }

    [System.Serializable]
    public class AudioSourceData
    {
        [SerializeField] public AudioSource audioSourcePrefab;
        [SerializeField] public bool muted = false;
        [SerializeField] public Vector2 pitchRange = new Vector2(1.0f, 1.0f);
        [System.NonSerialized] private AudioSource _audioSourceInstance;
        [System.NonSerialized] public float LastTimePlayed = 0.0f;

        public AudioSource Instance
        {
            get
            {
                if (!_audioSourceInstance)
                {
                    Clone();
                }
                return _audioSourceInstance;
            }
        }

        public bool CanPlay => (Time.realtimeSinceStartup - LastTimePlayed) > THIS.overlapProtectionTime;
        public float RandomPitch => Random.Range(pitchRange.x, pitchRange.y);


        private void Clone()
        {
            _audioSourceInstance = Instantiate(audioSourcePrefab, THIS.transform);
#if UNITY_EDITOR
            _audioSourceInstance.hideFlags = THIS.debug ? HideFlags.None : HideFlags.HideInHierarchy;
#endif
        }
    }
}

public static class AudioManagerExtensions
{
    public static void Play(this Audio audio)
    {
        if (!HapticManager.THIS.SavedData.canPlayAudio)
        {
            return;
        }
        AudioManager.Play((int)audio);
    }
    public static void PlayOneShot(this Audio audio, float volume = 1.0f)
    {
        if (!HapticManager.THIS.SavedData.canPlayAudio)
        {
            return;
        }
        AudioManager.PlayOneShot((int)audio, volume);
    }
    public static void PlayOneShotPitch(this Audio audio, float volume = 1.0f, float pitch = 1.0f)
    {
        if (!HapticManager.THIS.SavedData.canPlayAudio)
        {
            return;
        }
        AudioManager.PlayOneShotPitch((int)audio, volume, pitch);
    }
}



#if UNITY_EDITOR
namespace  Game.Editor
{
    using UnityEditor;
    using Internal.Core;

    [CustomEditor(typeof(AudioManager))]
    [CanEditMultipleObjects]
    public class AudioManagerGUI : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button(new GUIContent("REFRESH", "Convert to hard coded indexes.")))
            {
                AutoGenerate.GenerateAudioSources();
            }
            DrawDefaultInspector();
        }
    }
}
#endif