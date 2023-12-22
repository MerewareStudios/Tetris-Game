using System;
using System.Collections.Generic;

using UnityEngine;
using Random = UnityEngine.Random;

public class AudioManager : Internal.Core.Singleton<AudioManager>
{
    
#if UNITY_EDITOR
    [SerializeField] private bool debug = true;
    [SerializeField] private bool onlyPlayDebugSound;
    [SerializeField] public List<Audio> debugSounds;
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
        if (audioSourceData.muted)
        {
            return;
        }
        AudioSource audioSource = audioSourceData.Instance;
        audioSource.pitch = audioSourceData.RandomPitch;
        audioSource.PlayOneShot(audioSource.clip, volumeScale);
        // Debug.LogWarning("Play One Shot Random Pitch" + audioSourceData.audioSourcePrefab.name);
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
        if (audioSourceData.muted)
        {
            return;
        }
        AudioSource audioSource = audioSourceData.Instance;
        audioSource.pitch = pitch;
        audioSource.PlayOneShot(audioSource.clip, volumeScale);
        // Debug.LogWarning("Play One Shot Set Pitch" + audioSourceData.audioSourcePrefab.name);
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
        if (audioSourceData.muted)
        {
            return;
        }
        AudioSource audioSource = audioSourceData.Instance;
        audioSource.Play();
    }
    
    public static void Play(int key, float pitch)
    {
#if UNITY_EDITOR
        if (AudioManager.THIS.onlyPlayDebugSound && !AudioManager.THIS.debugSounds.Contains((Audio)key))
        {
            return;
        }
#endif
        AudioSourceData audioSourceData = AudioManager.THIS.audioSourceDatas[key];
        if (audioSourceData.muted)
        {
            return;
        }
        AudioSource audioSource = audioSourceData.Instance;
        audioSource.pitch = pitch;
        audioSource.Play();
    }
    
    public static void Stop(int key)
    {
        AudioSourceData audioSourceData = AudioManager.THIS.audioSourceDatas[key];
        AudioSource audioSource = audioSourceData.Instance;
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
    public static void Pause(int key)
    {
        AudioSourceData audioSourceData = AudioManager.THIS.audioSourceDatas[key];
        AudioSource audioSource = audioSourceData.Instance;
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
        }
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
    public static void Stop(this Audio audio)
    {
        AudioManager.Stop((int)audio);
    }
    public static void Pause(this Audio audio)
    {
        AudioManager.Pause((int)audio);
    }
    public static void Play(this Audio audio, float pitch)
    {
        if (!HapticManager.THIS.SavedData.canPlayAudio)
        {
            return;
        }
        AudioManager.Play((int)audio, pitch);
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
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    
    [CustomEditor(typeof(AudioManager))]
    [CanEditMultipleObjects]
    public class AudioManagerGUI : Editor
    {
        public override void OnInspectorGUI()
        {
            List<string> debugSoundNames = AudioManager.THIS.debugSounds.Select(item => item.ToString()).ToList();
            
            if (GUILayout.Button(new GUIContent("REFRESH", "Convert to hard coded indexes.")))
            {
                AutoGenerate.GenerateAudioSources();
            }
            if (GUILayout.Button(new GUIContent("SORT", "Alphabetic sorting.")))
            {
                AudioManager.THIS.audioSourceDatas = AudioManager.THIS.audioSourceDatas.OrderBy(x => x.audioSourcePrefab.name, new MixedComparer()).ToList();

                AutoGenerate.GenerateAudioSources();
                
                AudioManager.THIS.debugSounds.Clear();
                foreach (var debugSoundName in debugSoundNames)
                {
                    if (Enum.TryParse(debugSoundName, true, out Audio audio))
                    {
                        AudioManager.THIS.debugSounds.Add(audio);
                    }
                    else
                    {
                        AudioManager.THIS.debugSounds.Add((Audio)0);
                    }
                }
            }

           
            
            DrawDefaultInspector();
        }
    }
    
    class MixedComparer : IComparer<object>
    {
        public int Compare(object x, object y)
        {
            if (x is int && y is int)
            {
                return Comparer<int>.Default.Compare((int)x, (int)y);
            }
            else if (x is string && y is string)
            {
                return StringComparer.CurrentCultureIgnoreCase.Compare((string)x, (string)y);
            }
            else
            {
                // Handle other types, consider numbers greater than strings
                if (x is int)
                {
                    return 1; // x (number) is greater than y (string)
                }
                else if (y is int)
                {
                    return -1; // x (string) is less than y (number)
                }
                else
                {
                    return 0; // No comparison for other types
                }
            }
        }
    }
}
#endif