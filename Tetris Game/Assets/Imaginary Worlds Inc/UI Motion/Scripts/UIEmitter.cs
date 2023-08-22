using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        [SerializeField] public MotionSettings motionSettings;
        [SerializeField] public TargetSettings targetSettings;

        void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _canvasRect = _canvas.GetComponent<RectTransform>();
            mainCamera = Camera.main;
            uiCamera = _canvas.worldCamera;
            SetupPool();
        }

        void Start()
        {
            if (motionSettings.playAtStart)
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
            for (int i = 0; i < 50; i++)
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
                
                _imageQueue.Enqueue(image);
            }
        }

        public Image SpawnImage()
        {
            Image image = _imageQueue.Dequeue();
            image.gameObject.SetActive(true);

            return image;
        }

        #region Play Functions

        public void Play()
        {
            Image image = SpawnImage();
            RectTransform imageRectTransform = image.rectTransform;


            // Vector3 canPos;
            //             
            // Vector3 viewportPos = mainCamera.WorldToViewportPoint(targetSettings.startTransform.position);
            //
            // canPos = new Vector3(viewportPos.x.Remap(0.5f, 1.5f, 0f, _canvasRect.rect.width),
            //     viewportPos.y.Remap(0.5f, 1.5f, 0f, _canvasRect.rect.height), 0);
            //             
            // canPos = _canvasRect.transform.TransformPoint(canPos);
            //
            // canPos = transform.parent.InverseTransformPoint(canPos);
            //
            // imageRectTransform.transform.localPosition = canPos;
            
             
            // Vector2 ViewportPosition=uiCamera.WorldToViewportPoint(targetSettings.startTransform.position);
            //
            // Vector2 WorldObject_ScreenPosition=new Vector2(
            //     ((ViewportPosition.x*_canvasRect.sizeDelta.x)-(_canvasRect.sizeDelta.x*0.5f)),
            //     ((ViewportPosition.y*_canvasRect.sizeDelta.y)-(_canvasRect.sizeDelta.y*0.5f)));

            // imageRectTransform.anchoredPosition=WorldObject_ScreenPosition;
            
            // Vector3 pos = Vector3.zero;
            
            // Vector2 viewport = new Vector2(0.5f, 0.5f);
            // Vector2 viewport = uiCamera.WorldToViewportPoint(targetSettings.startTransform.position);
            // Vector3 screen = uiCamera.ViewportToScreenPoint(viewport);
            // Vector3 screen = uiCamera.WorldToScreenPoint(r.position);

            // Vector3 view = mainCamera.WorldToViewportPoint(targetSettings.startTransform.position);
            // Vector3 world = uiCamera.ViewportToWorldPoint(view);
            
            // Vector3 canPos;
            //             
            // Vector3 viewportPos = mainCamera.WorldToViewportPoint(targetSettings.startTransform.position);
            //
            // canPos = new Vector3(viewportPos.x.Remap(0.5f, 1.5f, 0f, _canvasRect.rect.width),
            //     viewportPos.y.Remap(0.5f, 1.5f, 0f, _canvasRect.rect.height), 0);
            //             
            // canPos = _canvasRect.transform.TransformPoint(canPos);
            //
            // canPos = transform.parent.InverseTransformPoint(canPos);
            //
            // // transform.localPosition = canPos;
            //
            // // Vector2 WorldObject_ScreenPosition = new Vector2(
            // //     ((viewport.x*_canvasRect.sizeDelta.x)-(_canvasRect.sizeDelta.x*0.5f)),
            // //     ((viewport.y*_canvasRect.sizeDelta.y)-(_canvasRect.sizeDelta.y*0.5f)));
            //
            // // Debug.Log(viewport);

            // imageRectTransform.position = world;
            // imageRectTransform.anchoredPosition = canPos;
            
            // Vector2 viewport = new Vector2(0.5f, 0.5f);

            // Vector3 worldPoint = r.position;
            
            // Vector3 local = _canvasRect.InverseTransformPoint(worldPoint);
            // Vector3 view = mainCamera.WorldToViewportPoint(targetSettings.startTransform.position);
            Vector3 screen = mainCamera.WorldToScreenPoint(targetSettings.startTransform.position);

            Debug.Log(screen);
            
            // Vector3 world = mainCamera.ScreenToWorldPoint(screen);
            
            // Debug.Log(screen);

            // Vector3 w = _canvasRect.TransformPoint(screen);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, screen, uiCamera, out Vector2 local);

            // Vector3 local = _canvasRect.InverseTransformPoint(worldPoint);
            
            imageRectTransform.localPosition = local;
            // imageRectTransform.position = world;

        }
        public void Restart()
        {
                
        }
        public void Pause()
        {
                
        }
        public void Emit(Vector3 worldPosition)
        {
            SpawnImage();
        }
        public void Emit(Vector2 screenPosition)
        {
            SpawnImage();
        }
        #endregion

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
    }
    [Serializable]
    public class MotionSettings
    {
        [SerializeField] public bool playAtStart = true;
        [SerializeField] public ParticleSystem.MinMaxCurve startSize = new ParticleSystem.MinMaxCurve(50.0f, 75.0f);
    }
    [Serializable]
    public class TargetSettings
    {
        [SerializeField] public Transform startTransform;
        [SerializeField] public Transform endTransform;
    }
    [Serializable]
    public class DebugSettings
    {
        [SerializeField] public bool hideImagesInHierarchy;
    }
    public static class Helper
    {
        public static void SetImageSize(this Image image, float size)
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
