using DG.Tweening;
using Internal.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Internal.Visuals.Tutorial
{
    public class Highlighter3D : MonoBehaviour
    {
        public static Highlighter3D instance; 
        public static Highlighter3D THIS
        {
                get
                {
                    if (instance == null)
                    {
                        Instantiate();
                    }
                    return instance;
                }
                set
                {
                    instance = value;
                }
            }

        private static void Instantiate()
        {
            THIS = Instantiate(Resources.Load<Highlighter3D>("Menu/Highlighter3D"));
        }
        public static Highlighter3D Highlight(Camera cameraRendersThisObject, GameObject gameObject, Camera sceneUICamera, float blackoutDelay, System.Action onSuccess, float releaseDelay = 0.2f, string infoText = "", Vector3 offset = default)
        {
            Blackout.Show(sceneUICamera, blackoutDelay, () => { THIS.canCheck = true; Finger.Show(sceneUICamera, THIS.GetTargetPosition, releaseDelay).SetText(infoText, offset); });
            THIS.highlightObjects.Clear();
            Add(gameObject);
            THIS.CopyCamera = cameraRendersThisObject;
            THIS.sceneUICamera = sceneUICamera;
            THIS.canCheck = false;
            THIS.OnSuccess = onSuccess;
            THIS.closeTween?.Kill();
            THIS.autoClose = true;
            return THIS;
        }
        public static Highlighter3D HighlightOnly(Camera cameraRendersThisObject, GameObject gameObject)
        {
            THIS.highlightObjects.Clear();
            Add(gameObject);
            THIS.CopyCamera = cameraRendersThisObject;
            THIS.canCheck = false;
            return THIS;
        }
        public static void Add(GameObject gameObject)
        {
            THIS.highlightObjects.Add(new HighlightObject(gameObject));
        }
        public void AddHighlight(GameObject gameObject)
        {
            highlightObjects.Add(new HighlightObject(gameObject));
        }
        public static void Clear()
        {
            THIS.highlightObjects.Clear();
        }
        public static void Close()
        {
            THIS.ClearMaterialRT();
            if (THIS.renderTexture != null)
            {
                THIS.renderTexture.Release();
            }
            Destroy(THIS.gameObject);
            THIS = null;
        }

        [Header("Camera")]
        [SerializeField] private Camera renderCamera;
        [System.NonSerialized] private Camera sceneUICamera;
        [Header("Material")]
        [SerializeField] private Material material;
        [SerializeField] private string renderTextureMaskKey = "_Mask3D";
        [Header("Render Settings")]
        [SerializeField] private UnityEngine.Experimental.Rendering.GraphicsFormat renderFormat;
        [SerializeField] private float renderResolution = 0.25f;
        [System.NonSerialized] private Camera copyCamera;
        [System.NonSerialized] private RenderTexture renderTexture;
        [System.NonSerialized] private bool autoClose;
        [System.NonSerialized] private bool canCheck = false;
        [System.NonSerialized] private System.Action OnSuccess;
        [System.NonSerialized] private Tween closeTween;

        internal Camera CopyCamera
        {
            set
            {
                this.copyCamera = value;
                this.renderCamera.Mimic(copyCamera);
                this.renderCamera.targetTexture = CreateRenderTexture();
                this.renderCamera.enabled = false;
            }
        }
        [System.NonSerialized] private List<HighlightObject> highlightObjects = new();
         
        public void Complete()
        {
            Blackout.Hide(0.35f);
            closeTween = DOVirtual.DelayedCall(0.35f, () => { Highlighter3D.Close(); });
            canCheck = false;
            Finger.Hide();
            OnSuccess?.Invoke();
        }
        void Update()
        {
            if (autoClose && canCheck && Input.GetMouseButtonDown(0))
            {
                Vector2 screenPointObject = copyCamera.WorldToScreenPoint(highlightObjects[0].go.transform.position);
                Vector2 dif = screenPointObject - (Vector2)Input.mousePosition;

                if (dif.magnitude < Screen.width * 0.1f)
                {
                    Complete();
                    return;
                }
            }
            else if (!autoClose && canCheck && Input.GetMouseButtonDown(0))
            {
                Blackout.THIS.Block = false;
            }
            //else if (autoClose && Input.GetMouseButtonUp(0))
            //{
            //    Blackout.THIS.Block = true;
            //}
            if (this.copyCamera == null)
            {
                return;
            }
            this.copyCamera.transform.Mimic(this.renderCamera.transform);

            for (int i = 0; i < highlightObjects.Count; i++)
            {
                HighlightObject highlighObject = highlightObjects[i];
                highlighObject.go.layer = 1;
            }
            renderCamera.Render();
            for (int i = 0; i < highlightObjects.Count; i++)
            {
                highlightObjects[i].Reset();
            }
        }
        public Vector3? GetTargetPosition()
        {
            if (highlightObjects == null || highlightObjects.Count == 0)
            {
                return null;
            }
            Vector3 screenPoint = copyCamera.WorldToScreenPoint(highlightObjects[0].go.transform.position);
            Vector3 screenWorld = sceneUICamera.ScreenToWorldPoint(screenPoint);
            return screenWorld;
        }
        private RenderTexture CreateRenderTexture()
        {
            if (renderTexture != null)
            {
                return renderTexture;
            }
            Vector2 screenSize = new Vector2(Screen.width, Screen.height) * renderResolution;
            renderTexture = new RenderTexture(((int)screenSize.x), ((int)screenSize.y), 0, renderFormat);
            material.SetTexture(renderTextureMaskKey, renderTexture);
            return renderTexture;
        }
        private void ClearMaterialRT()
        {
            material.SetTexture(renderTextureMaskKey, null);
        }
        public class HighlightObject
        {
            public int originalLayer;
            public GameObject go;

            public HighlightObject(GameObject go)
            {
                this.originalLayer = go.layer;
                this.go = go;
            }
            public void Reset()
            {
                go.layer = originalLayer;
            }
        }

        public void AutoClose(bool state)
        {
            this.autoClose = state;
        }
    }
}
