using DG.Tweening;
using Internal.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Visuals.Tutorial
{
    public class Highlighter2D : MonoBehaviour
    {
        public static Highlighter2D instance;
        public static Highlighter2D THIS
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
            THIS = Instantiate(Resources.Load<Highlighter2D>("Menu/Highlighter2D"));
        }
        public static Highlighter2D Highlight(Camera sceneUICamera, Image image, float blackoutDelay, System.Action onSuccess, float releaseDelay = 0.2f, string infoText = "", Vector3 offset = default)
        {
            Blackout.Show(sceneUICamera, blackoutDelay, () => { THIS.canCheck = true; Finger.Show(sceneUICamera, THIS.GetTargetPosition, releaseDelay).SetText(infoText, offset); });
            
            THIS.CopyImage = image;
            THIS.sceneUICamera = sceneUICamera;
            THIS.canCheck = false;
            THIS.OnSuccess = onSuccess;
            THIS.closeTween?.Kill();
            THIS.autoClose = true;
            return THIS;
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
        [SerializeField] private Camera uiCamera;
        [System.NonSerialized] private Camera sceneUICamera;
        [Header("Image Settings")]
        [SerializeField] private Canvas thisCanvas;
        [SerializeField] private Image mimicImage;
        [System.NonSerialized] private Image copyImage;
        internal Image CopyImage
        {
            set
            {
                this.copyImage = value;
                SetupImage();
                uiCamera.targetTexture = CreateRenderTexture();
                uiCamera.enabled = false;
            }
        }
        [Header("Material")]
        [SerializeField] private Material material;
        [SerializeField] private string renderTextureMaskKey = "_Mask2D";
        [Header("Render Settings")]
        [SerializeField] private UnityEngine.Experimental.Rendering.GraphicsFormat renderFormat;
        [Range(0.05f, 2.0f)][SerializeField] private float renderResolution = 1.0f;
        [System.NonSerialized] private RenderTexture renderTexture;
        [System.NonSerialized] private bool canCheck = false;
        [System.NonSerialized] private System.Action OnSuccess;
        [System.NonSerialized] private Tween closeTween;
        [System.NonSerialized] private bool autoClose;


        void Update()
        {
            if (autoClose && canCheck && Input.GetMouseButtonDown(0))
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(copyImage.rectTransform, Input.mousePosition, sceneUICamera, out Vector2 localPoint);
                if (copyImage.rectTransform.rect.Contains(localPoint))
                {
                    Blackout.Hide(0.35f);
                    closeTween = DOVirtual.DelayedCall(0.35f, () => { Highlighter2D.Close(); });
                    canCheck = false;
                    Finger.Hide();
                    OnSuccess?.Invoke();
                }
            }
        }
        void LateUpdate()
        {
            UpdateCopyImage();
            uiCamera.Render();
        }

        public Vector3? GetTargetPosition()
        {
            if (mimicImage == null)
            {
                return null;
            }
            return mimicImage.transform.position;
        }
        private void SetupImage()
        {
            if (copyImage == null)
            {
                return;
            }
            mimicImage.sprite = copyImage.sprite;
            mimicImage.type = copyImage.type;
            mimicImage.pixelsPerUnitMultiplier = copyImage.pixelsPerUnitMultiplier;
        }
        private void UpdateCopyImage()
        {
            if (copyImage == null)
            {
                return;
            }
            mimicImage.rectTransform.position = copyImage.rectTransform.position;
            mimicImage.rectTransform.localScale = Vector3.one;
            mimicImage.rectTransform.rotation = copyImage.rectTransform.rotation;


            Vector3[] corners = new Vector3[4];
            copyImage.rectTransform.GetWorldCorners(corners);

            float width = (corners[0] - corners[3]).magnitude;
            float height = (corners[0] - corners[1]).magnitude;
            mimicImage.rectTransform.sizeDelta = new Vector2(width, height) / thisCanvas.transform.localScale.x;
        }
        private RenderTexture CreateRenderTexture()
        {
            if (renderTexture != null)
            {
                return renderTexture;
            }
            Vector2 screenSize = new Vector2(Screen.width, Screen.height) * renderResolution;
            renderTexture = new RenderTexture(((int)screenSize.x), ((int)screenSize.y), 0, renderFormat, 0);
            material.SetTexture(renderTextureMaskKey, renderTexture);
            return renderTexture;
        }
        private void ClearMaterialRT()
        {
            material.SetTexture(renderTextureMaskKey, null);
        }

        public void AutoClose(bool state)
        {
            this.autoClose = state;
        }
    }
}