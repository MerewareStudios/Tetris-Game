using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;
using UnityEngine.ResourceManagement.AsyncOperations;
using Random = UnityEngine.Random;

public class AudioManager : Internal.Core.Singleton<AudioManager>
{
    
#if UNITY_EDITOR
    [SerializeField] private bool debug = true;
    [SerializeField] private bool onlyPlayDebugSound;
    [SerializeField] public List<Audio> debugSounds;
    [SerializeField] public List<String> debugNames;
#endif
    [System.NonSerialized] private AudioSource _musicSource;
    [SerializeField] private List<MusicPair> musicData;
    [System.NonSerialized] private AsyncOperationHandle<AudioClip> _currentMusicHandle;
    [System.NonSerialized] private int _currentTrackIndex = -1;
    [SerializeField] public AudioMixerGroup audioMixerMusic;
    // [SerializeField] public float maxVolume;
    // [SerializeField] public float duckVolume;
    [SerializeField] public float minVolume;
    
    [System.NonSerialized] private AudioSource _emptySource = null;
    [SerializeField] public List<AudioSourceData> audioSourceDatas;
    [SerializeField] public AudioMixerGroup audioMixerSfx;
    [SerializeField] public float overlapProtectionTime = 0.1f;
    
    [System.NonSerialized] private Tween _volumeTween;
    [System.NonSerialized] private const string MusicVolumeKey = "musicVolume";

    [System.Serializable]
    public class MusicPair
    {
        [SerializeField] public AssetReference assetReference;
        [SerializeField] public float volume = 1.0f;
    }

    void Awake()
    {
        _emptySource = this.gameObject.AddComponent<AudioSource>();
        _emptySource.loop = false;
        _emptySource.outputAudioMixerGroup = audioMixerSfx;
        _emptySource.hideFlags = HideFlags.HideInInspector;
        
        _musicSource = this.gameObject.AddComponent<AudioSource>();
        _musicSource.loop = true;
        _musicSource.playOnAwake = false;
        _musicSource.outputAudioMixerGroup = audioMixerMusic;
        _musicSource.hideFlags = HideFlags.HideInInspector;
    }

    public void PlayBackgroundTrackByLevel(int level)
    {        
        int trackIndex = ((level - 1) / 3) % musicData.Count;
        if (_currentTrackIndex == trackIndex)
        {
            Duck(false);
            return;
        }

        SetMusicVolume(minVolume, _musicSource.isPlaying ? 1.0f : 0.0f, () =>
        {
            LoadBackgroundTrack(trackIndex);
        });
    }

    private void LoadBackgroundTrack(int trackIndex)
    {
        UnloadBackgroundTrack();
        _currentTrackIndex = trackIndex;
        _currentMusicHandle = Addressables.LoadAssetAsync<AudioClip>(musicData[trackIndex].assetReference);
        _currentMusicHandle.Completed += operationHandle =>
        {
            if (operationHandle.Status == AsyncOperationStatus.Failed)
            {
                return;
            }
            _musicSource.clip = operationHandle.Result as AudioClip;
            // _musicSource.volume = musicData[trackIndex].volume;
            _musicSource.Play();
            Duck(false);
        };
    }

    private void UnloadBackgroundTrack()
    {
        if (!_musicSource.clip)
        {
            return;
        }
        _musicSource.clip = null;
        Addressables.Release(_currentMusicHandle);
    }

    private void SetMusicVolume(float target, float duration, System.Action onComplete = null)
    {
        _volumeTween?.Kill();

        if (duration == 0.0f)
        {
            audioMixerMusic.audioMixer.SetFloat(MusicVolumeKey, target);
            onComplete?.Invoke();
            return;
        }
        
        audioMixerMusic.audioMixer.GetFloat(MusicVolumeKey, out float start);
        float value = 0.0f;
        _volumeTween = DOTween.To(x => value = x, start, target, duration).SetEase(Ease.InSine).SetUpdate(true);
        _volumeTween.onUpdate = () =>
        {
            audioMixerMusic.audioMixer.SetFloat(MusicVolumeKey, value);
        };
        if (onComplete != null)
        {
            _volumeTween.onComplete = onComplete.Invoke;
        }
    }

    public void Duck(bool duck)
    {
        float maxVolume = musicData[Mathf.Max(0, _currentTrackIndex)].volume - 1.85f;
        SetMusicVolume(duck ? (maxVolume - 2.0f) : maxVolume, 0.25f);
    }
    
    
    

    public static void PlayOneShot(AudioClip audioClip, float volume, float pitch)
    {
        AudioSource audioSource = AudioManager.THIS._emptySource;
        audioSource.volume = volume;
        audioSource.pitch = pitch;
        audioSource.PlayOneShot(audioClip, volume);
    }
        
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
        audioSource.pitch = audioSourceData.RandomPitch;
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
            _audioSourceInstance.gameObject.hideFlags = THIS.debug ? HideFlags.None : HideFlags.HideInHierarchy;
#endif
        }
    }
}

public static class AudioManagerExtensions
{
    public static void Play(this Audio audio)
    {
        if (!HapticManager.THIS.CanPlayAudio)
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
        if (!HapticManager.THIS.CanPlayAudio)
        {
            return;
        }
        AudioManager.Play((int)audio, pitch);
    }
    public static void PlayOneShot(this Audio audio, float volume = 1.0f)
    {
        if (!HapticManager.THIS.CanPlayAudio)
        {
            return;
        }
        AudioManager.PlayOneShot((int)audio, volume);
    }
    public static void PlayOneShotPitch(this Audio audio, float volume = 1.0f, float pitch = 1.0f)
    {
        if (!HapticManager.THIS.CanPlayAudio)
        {
            return;
        }
        AudioManager.PlayOneShotPitch((int)audio, volume, pitch);
    }
    
    public static void PlayOneShot(this AudioClip audioClip, float volume = 1.0f, float pitch = 1.0f)
    {
        if (!HapticManager.THIS.CanPlayAudio)
        {
            return;
        }
        AudioManager.PlayOneShot(audioClip, volume, pitch);
    }
}



#if UNITY_EDITOR
namespace  Game.Editor
{
    using UnityEditor;
    using Internal.Core;
    using System.Collections.Generic;

    [CustomEditor(typeof(AudioManager))]
    [CanEditMultipleObjects]
    public class AudioManagerGUI : Editor
    {
        public override void OnInspectorGUI()
        {
            AudioManager.THIS.debugNames = AudioManager.THIS.debugSounds.Select(item => item.ToString()).ToList();
            
            if (GUILayout.Button(new GUIContent("REFRESH", "Convert to hard coded indexes.")))
            {
                AutoGenerate.GenerateAudioSources();
            }
            if (GUILayout.Button(new GUIContent("SORT", "Alphabetic sorting.")))
            {
                AudioManager.THIS.audioSourceDatas.Sort((a, b) => String.Compare(a.audioSourcePrefab.name, b.audioSourcePrefab.name));

                AutoGenerate.GenerateAudioSources();
                
               
            }
            if (GUILayout.Button(new GUIContent("RESTORE DEBUG LIST", "Restore.")))
            {
                AudioManager.THIS.debugSounds.Clear();
                foreach (var debugSoundName in AudioManager.THIS.debugNames)
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
}
#endif