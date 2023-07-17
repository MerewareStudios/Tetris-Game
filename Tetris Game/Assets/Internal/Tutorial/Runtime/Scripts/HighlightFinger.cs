using DG.Tweening;
using Internal.Core;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HighlightFinger : MonoBehaviour
{
    private static HighlightFinger currentReference;

    public static void Create(Camera cam, Image image, float delay)
    {
        if (HighlightFinger.currentReference == null)
        {
            HighlightFinger.currentReference = MonoBehaviour.Instantiate(Resources.Load<HighlightFinger>("Menu/Highlight Finger"));
        }
        HighlightFinger.currentReference.Init(cam, image, delay);
    }
    public static void Create(Camera cam, Camera copyCam, GameObject targetObject, float delay)
    {
        if (HighlightFinger.currentReference == null)
        {
            HighlightFinger.currentReference = MonoBehaviour.Instantiate(Resources.Load<HighlightFinger>("Menu/Highlight Finger"));
        }
        HighlightFinger.currentReference.Init(cam, copyCam, targetObject, delay);
    }
    public static void AddHighlightObject(GameObject targetObject)
    {
        if (HighlightFinger.currentReference != null)
        {
            HighlightFinger.currentReference.AddHighlightObj(targetObject);
        }
    }
    public static void CloseStatic()
    {
        if (HighlightFinger.currentReference != null)
        {
            HighlightFinger.currentReference.Close();
            HighlightFinger.currentReference = null;
        }
    }

    [SerializeField] private Canvas canvas;
    [SerializeField] private Canvas blackoutCanvas;
    [SerializeField] private Camera uiCamera;
    [SerializeField] private Camera tdCamera;
    [SerializeField] private Transform positionPivot;
    [SerializeField] private Transform fingerScalePivot;
    [SerializeField] private Transform fingerLocalPivot;
    [SerializeField] private Transform fingerRotationPivot;
    [SerializeField] private ParticleSystem fingerPS;
    [SerializeField] private Transform fingerPSLoc;
    [SerializeField] private Image fingerImage;
    [SerializeField] private RawImage backgroundImage;
    [SerializeField] private Material material;
    [SerializeField] private Gradient gradient;
    [SerializeField] private Image mimicImage;
    
    // 3D Highlight
    [System.NonSerialized] private Camera tdCopyCamera;
    // 2D Highlight
    [System.NonSerialized] private Image copyImage;
    // Common Highlight
    [System.NonSerialized] private Transform followTransform;
    [System.NonSerialized] private List<GameObject> highlightObjects = new();
    [System.NonSerialized] private Coroutine sequence;
    [System.NonSerialized] private Vector3 targetPositionWorld;
    [System.NonSerialized] private Vector3 targetPositionScreen;
    [System.NonSerialized] private Vector3 targetPositionView;
    [System.NonSerialized] private float currentStep;
    //[System.NonSerialized] private bool canCheck = false;
    [System.NonSerialized] private RenderTexture renderTextureUI;
    [System.NonSerialized] private RenderTexture renderTexture3D;
    [System.NonSerialized] private Tweener fadeTween;

    public void Init(Camera cam, Camera copyCam, GameObject targetObject, float delay)
    {
        canvas.worldCamera = cam;
        canvas.sortingLayerName = "Overlay";
        canvas.sortingOrder = 10;

        this.tdCopyCamera = copyCam;
        this.tdCamera.enabled = false;
        copyCam.Mimic(this.tdCamera);

        fadeTween?.Kill();

        if (renderTexture3D == null)
        {
            Vector2 screenSize = new Vector2(Screen.width, Screen.height) * 0.25f;
            renderTexture3D = new RenderTexture(((int)screenSize.x), ((int)screenSize.y), 0, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB);
            material.SetTexture("_Mask3D", renderTexture3D);
            tdCamera.targetTexture = renderTexture3D;
        }
        uiCamera.enabled = uiCamera.targetTexture != null;


        currentStep = 0.0f;
        material.SetColor("_BackgroundColor", gradient.Evaluate(currentStep));
        this.fingerImage.enabled = false;
        if (sequence != null)
        {
            StopCoroutine(sequence);
        }
        sequence = StartCoroutine(PressAndRelease(delay));
        //canCheck = false;
        backgroundImage.raycastTarget = true;

        highlightObjects.Add(targetObject);


        this.transform.SetParent(null);
    }
    private void AddHighlightObj(GameObject go)
    {
        highlightObjects.Add(go);
    }
    public void Init(Camera cam, Image copyImage, float delay)
    {
        canvas.worldCamera = cam;
        canvas.sortingLayerName = "Overlay";
        canvas.sortingOrder = 10;

        fadeTween?.Kill();

        if (renderTextureUI == null)
        {
            Vector2 screenSize = new Vector2(Screen.width, Screen.height) * 0.1f;
            renderTextureUI = new RenderTexture(((int)screenSize.x), ((int)screenSize.y), 0, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB);
            material.SetTexture("_Mask2D", renderTextureUI);
            uiCamera.targetTexture = renderTextureUI;
        }

        tdCamera.enabled = tdCamera.targetTexture != null;

        this.copyImage = copyImage;
        currentStep = 0.0f;
        material.SetColor("_BackgroundColor", gradient.Evaluate(currentStep));
        this.fingerImage.enabled = false;
        if (sequence != null)
        {
            StopCoroutine(sequence);
        }
        sequence = StartCoroutine(PressAndRelease(delay));
        //canCheck = false;
        backgroundImage.raycastTarget = true;

        followTransform = copyImage.rectTransform;


        this.transform.SetParent(null);
    }
    private void UpdateMimicImage()
    {
        if (copyImage == null)
        {
            return;
        }
        mimicImage.sprite = copyImage.sprite;
        mimicImage.rectTransform.position = copyImage.rectTransform.position;
        mimicImage.rectTransform.localScale = Vector3.one;
        mimicImage.rectTransform.rotation = copyImage.rectTransform.rotation;


        Vector3[] corners = new Vector3[4];
        copyImage.rectTransform.GetWorldCorners(corners);

        float width = (corners[0] - corners[3]).magnitude;
        float height = (corners[0] - corners[1]).magnitude;
        mimicImage.rectTransform.sizeDelta = new Vector2(width, height) / blackoutCanvas.transform.localScale.x;


        mimicImage.type = copyImage.type;
        mimicImage.pixelsPerUnitMultiplier = copyImage.pixelsPerUnitMultiplier;
    }
    public void UpdateFingerPosition()
    {
        targetPositionScreen = canvas.worldCamera.WorldToScreenPoint(followTransform.position);
        targetPositionView = canvas.worldCamera.WorldToViewportPoint(followTransform.position);
        targetPositionWorld = followTransform.position;

        material.SetVector("_Position", targetPositionView);
        //material.SetFloat("_Radius", 0.06f);

        positionPivot.position = targetPositionWorld;
    }
    public void UpdateCameraPosition()
    {
        if (this.tdCamera != null && this.tdCopyCamera != null)
        {
            this.tdCopyCamera.transform.Mimic(this.tdCamera.transform);

            List<int> layers = new();
            for (int i = 0; i < highlightObjects.Count; i++)
            {
                GameObject highlighObject = highlightObjects[i];

                int prevLayer = highlighObject.layer;
                layers.Add(prevLayer);

                highlighObject.layer = 1;
            }
            tdCamera.Render();
            for (int i = 0; i < highlightObjects.Count; i++)
            {
                GameObject highlighObject = highlightObjects[i];
                highlighObject.layer = layers[i];
            }
        }
    }
    public void Close()
    {
        if (sequence != null)
        {
            StopCoroutine(sequence);
        }
        fingerScalePivot.DOKill();
        fingerLocalPivot.DOKill();
        fingerRotationPivot.DOKill();
        Destroy(this.gameObject);
    }

    private IEnumerator PressAndRelease(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        //Fade(1.0f, 0.25f, 0.0f, () => { canCheck = true; });

        
        fingerImage.enabled = true;

        while (true)
        {
            Press();

            yield return new WaitForSecondsRealtime(0.25f);

            Release();

            yield return new WaitForSecondsRealtime(1.5f);
        }

    }
    private void Press()
    {
        fingerScalePivot.DOKill();
        fingerScalePivot.DOScale(Vector3.one * 0.75f, 0.125f).SetEase(Ease.InOutSine).SetUpdate(true).onComplete += () => { Emit();};

        fingerLocalPivot.DOKill();
        fingerLocalPivot.DOLocalMove(new Vector3(30.0f, 0.0f, 0.0f), 0.125f).SetEase(Ease.InOutSine).SetUpdate(true);

        fingerRotationPivot.DOKill();
        fingerRotationPivot.DOLocalRotate(new Vector3(0.0f, 0.0f, 10.0f), 0.125f).SetEase(Ease.InOutSine).SetUpdate(true);
    }

    public void Release()
    {
        fingerScalePivot.DOKill();
        fingerScalePivot.DOScale(Vector3.one * 1.0f, 0.125f).SetEase(Ease.InOutSine).SetUpdate(true);

        fingerLocalPivot.DOKill();
        fingerLocalPivot.DOLocalMove(new Vector3(0.0f, -70.0f, 0.0f), 0.125f).SetEase(Ease.InOutSine).SetUpdate(true);

        fingerRotationPivot.DOKill();
        fingerRotationPivot.DOLocalRotate(new Vector3(0.0f, 0.0f, 0.0f), 0.125f).SetEase(Ease.InOutSine).SetUpdate(true);
    }
    public void Fade(float end, float duration, float delay, System.Action endAction = null)
    {
        fadeTween = DOVirtual.Float(currentStep, end, duration, (time) => { currentStep = time; material.SetColor("_BackgroundColor", gradient.Evaluate(currentStep)); }).SetEase(Ease.InOutSine).SetDelay(delay).SetUpdate(true);
            fadeTween.onComplete = () => { endAction?.Invoke(); };
    }
    void LateUpdate()
    {
        UpdateCameraPosition();
    }
    private void Emit()
    {
        fingerPS.transform.position = fingerPSLoc.position;
        fingerPS.Emit(1);
    }
}
