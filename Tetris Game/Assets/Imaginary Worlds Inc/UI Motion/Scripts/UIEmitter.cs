using System;
using System.Collections.Generic;
using DG.Tweening;
using IWI.Emitter.Enums;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Space = IWI.Emitter.Enums.Space;
using ValueType = IWI.Emitter.Enums.ValueType;

namespace IWI.UI
{
    [RequireComponent(typeof(Canvas))]
    public class UIEmitter : MonoBehaviour
    {
        // Components
        private Canvas _canvas;
        private RectTransform _canvasRect;
        [System.NonSerialized] public Camera mainCamera;
        [System.NonSerialized] public Camera uiCamera;
        private readonly Queue<Image> _imageQueue = new();
        [SerializeField] public DebugSettings debugSettings;
        [SerializeField] public ImageSettings imageSettings;
        [SerializeField] public ValueSettings valueSettings;
        [SerializeField] public MotionData motionData;
       
        [SerializeField] public TargetSettings targetSettingsStart;
        [SerializeField] public TargetSettings targetSettingsEnd;
        [SerializeField] public CallbackSettings callbackSettings;
        [System.NonSerialized] private float _timeOffset = 0.0f;

        void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _canvasRect = _canvas.GetComponent<RectTransform>();
            mainCamera = Camera.main;
            uiCamera = _canvas.worldCamera;
            SetupPool();

            _timeOffset = Random.Range(0.0f, 100.0f);
        }

        void Start()
        {
            if (motionData.generalSettings.playAtStart)
            {
                Play();
            }
        }

        private void SetupPool()
        {
            if (_imageQueue.Count > 0)
            {
                return;
            }
            for (int i = 0; i < imageSettings.maxImageCount; i++)
            {
                GameObject imagePrefab = new GameObject();
                imagePrefab.SetActive(false);
                #if UNITY_EDITOR
                imagePrefab.name = "Image";
                imagePrefab.hideFlags = debugSettings.hideImagesInHierarchy ? HideFlags.HideInHierarchy : HideFlags.None;
                #endif
                RectTransform rectTransform = imagePrefab.AddComponent<RectTransform>();
                rectTransform.SetParent(this.transform);
                rectTransform.localScale = Vector3.one;
                rectTransform.localPosition = Vector3.zero;
                Image image = imagePrefab.AddComponent<Image>();
                
                image.sprite = imageSettings.sprite;
                image.raycastTarget = imageSettings.raycastTarget;
                image.maskable = imageSettings.maskable;
                
                _imageQueue.Enqueue(image);
            }
        }

        public Image SpawnImage()
        {
            if (_imageQueue.Count == 0)
            {
                return null;
            }
            Image image = _imageQueue.Dequeue();
            image.gameObject.SetActive(true);

            return image;
        }
        public void DespawnImage(Image image)
        {
            _imageQueue.Enqueue(image);
            image.gameObject.SetActive(false);
        }

        #region Play Functions

        public void Play()
        {
            int emitCount = (int)motionData.generalSettings.startCount.Evaluate(Now, Random.value);
            Emit(emitCount, valueSettings, targetSettingsStart, targetSettingsEnd);
        }
        public void Emit(int emitCount, ValueSettings value, TargetSettings? start, TargetSettings? end, MotionData motionDataSO = null)
        {
            MotionData md = motionDataSO ? motionDataSO : this.motionData;
            BurstSettings burstSettings = md.burstSettings;
            GeneralSettings generalSettings = md.generalSettings;
            
            Vector3 startPosition = GetLocal(start ?? this.targetSettingsStart);
            Vector3 endPosition = GetLocal(end ?? this.targetSettingsEnd);

            int valuePerInstance;
            int excess;

            switch (value.valueType)
            {
                case ValueType.TotalValue:
                    valuePerInstance = value.amount / emitCount;
                    excess = value.amount - (valuePerInstance * emitCount);
                    break;
                case ValueType.ValuePerInstance:
                    valuePerInstance = value.amount;
                    excess = 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            for (int i = 0; i < emitCount; i++)
            {
                int index = i;
                Sequence sequence = DOTween.Sequence();
                
                Image image = SpawnImage();
                if (!image)
                {
                    return;
                }
                RectTransform imageRectTransform = image.rectTransform;
                
                float size = generalSettings.startSize.Evaluate(Now, Random.value);
                image.SetSize(size);

                imageRectTransform.DOKill();
            
                imageRectTransform.anchoredPosition = startPosition;

                if (burstSettings.burst)
                {
                    float burstMaxDistance = burstSettings.maxDistance.Evaluate(Now, Random.value);
                    float burstMinDistance = burstSettings.minDistance.Evaluate(Now, Random.value);
                    float burstMotionDuration = burstSettings.duration.Evaluate(Now, Random.value);
                    float burstEndDelay = burstSettings.endDelay.Evaluate(Now, Random.value);

                    Vector2 burstDistance = Random.insideUnitCircle.normalized * Random.Range(burstMinDistance, burstMaxDistance);
                    
                    Tween burstMotion = imageRectTransform.DOAnchorPos(burstDistance, burstMotionDuration).SetRelative(true);
                    _ = burstSettings.ease.Equals(Ease.Unset) ? burstMotion.SetEase(burstSettings.easeCurve) : burstMotion.SetEase(burstSettings.ease, burstSettings.overshoot);
                    burstMotion.onUpdate = () =>
                    {
                        float percent = burstMotion.position / burstMotionDuration;
                        imageRectTransform.localScale = Vector3.one * burstSettings.sizeOverDuration.Evaluate(percent);
                    };
                    sequence.Append(burstMotion).AppendInterval(burstEndDelay);
                }
                
                
                float duration = generalSettings.duration.Evaluate(Now, Random.value);
                
                Tween travelMotion = imageRectTransform.DOAnchorPos(endPosition, duration);
                _ = generalSettings.ease.Equals(Ease.Unset) ? travelMotion.SetEase(generalSettings.easeCurve) : travelMotion.SetEase(generalSettings.ease, generalSettings.overshoot);
                travelMotion.onUpdate = () =>
                {
                    float percent = travelMotion.position / duration;
                    imageRectTransform.localScale = Vector3.one * generalSettings.sizeOverDuration.Evaluate(percent);
                };
                
                sequence.Append(travelMotion);
                
                sequence.onComplete = () =>
                {
                    DespawnImage(image);
                    callbackSettings.OnArrive?.Invoke(valuePerInstance + ((index + 1 == emitCount) ? excess : 0));
                };
            }
        }

        public Vector3 GetLocal(TargetSettings targetSettings)
        {
            Vector2 localPoint = Vector3.zero;
            Camera cam = null;
            switch (targetSettings.space)
            {
                case Space.Screen:
                    cam = uiCamera;
                    break;
                case Space.World:
                    cam = mainCamera;
                    break;
            }

            localPoint = cam.WorldToScreenPoint(targetSettings.transform ? targetSettings.transform.position : targetSettings.Position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, localPoint, uiCamera, out Vector2 local);
            return local;
        }
        public void Restart()
        {
                
        }
        public void Pause()
        {
                
        }
        #endregion

        private float Now => _timeOffset + Time.time;

        void OnDestroy()
        {
            while (_imageQueue.Count > 0)
            {
                DestroyImmediate(_imageQueue.Dequeue().gameObject);
            }
        }
    }

    [Serializable]
    public class ImageSettings
    {
        [SerializeField] public Sprite sprite;
        [SerializeField] public bool raycastTarget = false;
        [SerializeField] public bool maskable = false;
        [SerializeField] public int maxImageCount = 50;
    }
    [Serializable]
    public struct ValueSettings
    {
        [SerializeField] public ValueType valueType;
        [SerializeField] public int amount;

        public ValueSettings(ValueType valueType, int amount)
        {
            this.valueType = valueType;
            this.amount = amount;
        }
    }
    [Serializable]
    public class GeneralSettings
    {
        [SerializeField] public bool playAtStart = true;
        [SerializeField] public ParticleSystem.MinMaxCurve startCount = new ParticleSystem.MinMaxCurve(1, 10);
        [SerializeField] public ParticleSystem.MinMaxCurve startSize = new ParticleSystem.MinMaxCurve(50.0f, 75.0f);
        [SerializeField] public ParticleSystem.MinMaxCurve duration = new ParticleSystem.MinMaxCurve(0.5f, 0.6f);
        [SerializeField] public Ease ease;
        [SerializeField] public float overshoot = 1.7f;
        [SerializeField] public AnimationCurve easeCurve;
        [SerializeField] public AnimationCurve sizeOverDuration;
    }
    [Serializable]
    public class BurstSettings
    {
        [SerializeField] public bool burst = true;
        [SerializeField] public Shape shape;
        [SerializeField] public ParticleSystem.MinMaxCurve maxDistance = new ParticleSystem.MinMaxCurve(50, 100);
        [SerializeField] public ParticleSystem.MinMaxCurve minDistance = new ParticleSystem.MinMaxCurve(50, 100);
        [SerializeField] public ParticleSystem.MinMaxCurve duration = new ParticleSystem.MinMaxCurve(0.5f, 0.6f);
        [SerializeField] public ParticleSystem.MinMaxCurve endDelay = new ParticleSystem.MinMaxCurve(0.25f, 0.3f);
        [SerializeField] public Ease ease;
        [SerializeField] public float overshoot = 1.7f;
        [SerializeField] public AnimationCurve easeCurve;
        [SerializeField] public AnimationCurve sizeOverDuration;
    }
    [Serializable]
    public struct TargetSettings
    {
        [SerializeField] public IWI.Emitter.Enums.Space space;
        [SerializeField] public Transform transform;
        [System.NonSerialized] public Vector3 Position;


        public TargetSettings(Space space, Transform transform, Vector3 position)
        {
            this.space = space;
            this.transform = transform;
            Position = position;
        }
    }
    [Serializable]
    public class CallbackSettings
    {
        [SerializeField] public UnityEvent<int> OnArrive;
    }
    
    [Serializable]
    public class DebugSettings
    {
        [SerializeField] public bool hideImagesInHierarchy;
    }
    
    
    public static class Helper
    {
        public static void SetSize(this Image image, float size)
        {
            image.rectTransform.sizeDelta = new Vector2(size, size);
        }
        public static float Remap (this float value, float from1, float to1, float from2, float to2) {
            float v = (value - from1) / (to1 - from1) * (to2 - from2) + from2;
            if(float.IsNaN(v) || float.IsInfinity(v))
                return 0;
            return v;
        }
    }
    
}

namespace IWI.Emitter.Enums
{
    public enum Space
    {
        Screen,
        World,
    }
    public enum Shape
    {
        Circular,
    }
    public enum ValueType
    {
        TotalValue,
        ValuePerInstance,
    }
}

#if UNITY_EDITOR
namespace IWI.Editor
{
    using UnityEditor;
    using IWI.UI;

    [CustomEditor(typeof(UIEmitter))]
    [CanEditMultipleObjects]
    public class UIMotionEditor : Editor
    {
        private UIEmitter _uiEmitter;
            
        private void OnEnable()
        {
            _uiEmitter = target as UIEmitter;
        }
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (GUILayout.Button(new GUIContent("Play", "Play the system")))
            {
                _uiEmitter.Play();
            }
        }
    }
}
#endif
