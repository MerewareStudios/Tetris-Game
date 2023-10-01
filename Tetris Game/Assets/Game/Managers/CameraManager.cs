
using DG.Tweening;
using Internal.Core;
using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    [SerializeField] public Camera gameCamera;
    [SerializeField] public Camera uiCamera;
    [SerializeField] private Transform shakePivot;

    public void Shake(float amplitude = 1.0f, float duration = 0.35f)
    {
        shakePivot.DOKill();
        shakePivot.localRotation = Quaternion.identity;
        shakePivot.DOPunchRotation(Random.insideUnitSphere * 1.0f, 0.35f).SetEase(Ease.InOutSine);
    }
}
