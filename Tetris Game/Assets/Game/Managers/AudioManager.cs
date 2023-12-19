using System.Collections.Generic;
using Internal.Core;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    
#if UNITY_EDITOR
    [SerializeField] private bool debug = true;
#endif
    [SerializeField] public List<AudioSourceData> audioSourceDatas;


    public static void PlayOneShot(int key, float volume = 1.0f)
    {
        AudioSourceData audioSourceData = AudioManager.THIS.audioSourceDatas[key];
        AudioSource audioSource = audioSourceData.Instance;
        audioSource.PlayOneShot(audioSource.clip, volume);
    }
    public static void Play(int key, float volume = 1.0f)
    {
        AudioSourceData audioSourceData = AudioManager.THIS.audioSourceDatas[key];
        AudioSource audioSource = audioSourceData.Instance;
        audioSource.Play();
    }
    
    [System.Serializable]
    public class AudioSourceData
    {
        [SerializeField] public AudioSource audioSourcePrefab;
        [SerializeField] public bool muted = false;
        [System.NonSerialized] private AudioSource _audioSourceInstance;

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
    public static void Play(this Audio audio, float volume = 1.0f)
    {
        if (!HapticManager.THIS.SavedData.canPlayAudio)
        {
            return;
        }
        AudioManager.Play((int)audio, volume);
    }
    public static void PlayOneShot(this Audio audio, float volume = 1.0f)
    {
        if (!HapticManager.THIS.SavedData.canPlayAudio)
        {
            return;
        }
        AudioManager.PlayOneShot((int)audio, volume);
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