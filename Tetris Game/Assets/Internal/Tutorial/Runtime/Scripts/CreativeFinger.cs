using UnityEngine;
using DG.Tweening;

public class CreativeFinger : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private RectTransform pivot;
    [SerializeField] private Transform fingerScalePivot;
    [SerializeField] private Transform fingerLocalPivot;
    [SerializeField] private Transform fingerRotationPivot;
    [SerializeField] private ParticleSystem fingerPS;
    [SerializeField] private Transform fingerPSLoc;
    [System.NonSerialized] private bool canEmit = false;

    public void SetUp(Camera cam)
    {
        canvas.worldCamera = cam;
    }
    
    void LateUpdate()
    {
        pivot.anchoredPosition = GetLocal();

        if (Input.GetMouseButtonDown(0))
        {
            canEmit = true;

            fingerScalePivot.DOKill();
            fingerScalePivot.DOScale(Vector3.one * 0.75f, 0.125f).SetEase(Ease.InOutSine).SetUpdate(true).onComplete += () => { Emit(); canEmit = false; };

            fingerLocalPivot.DOKill();
            fingerLocalPivot.DOLocalMove(new Vector3(30.0f, 0.0f, 0.0f), 0.125f).SetEase(Ease.InOutSine).SetUpdate(true);

            fingerRotationPivot.DOKill();
            fingerRotationPivot.DOLocalRotate(new Vector3(0.0f, 0.0f, 10.0f), 0.125f).SetEase(Ease.InOutSine).SetUpdate(true);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Emit();

            fingerScalePivot.DOKill();
            fingerScalePivot.DOScale(Vector3.one * 1.0f, 0.125f).SetEase(Ease.InOutSine).SetUpdate(true);

            fingerLocalPivot.DOKill();
            fingerLocalPivot.DOLocalMove(new Vector3(0.0f, -70.0f, 0.0f), 0.125f).SetEase(Ease.InOutSine).SetUpdate(true);

            fingerRotationPivot.DOKill();
            fingerRotationPivot.DOLocalRotate(new Vector3(0.0f, 0.0f, 0.0f), 0.125f).SetEase(Ease.InOutSine).SetUpdate(true);
        }
    }
    private void Emit()
    {
        if (canEmit)
        {
            fingerPS.transform.position = fingerPSLoc.position;
            fingerPS.Emit(1);
            canEmit = false;
        }
    }
    
    public Vector3 GetLocal()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect,Input.mousePosition, canvas.worldCamera, out Vector2 local);
        return local;
    }
}
