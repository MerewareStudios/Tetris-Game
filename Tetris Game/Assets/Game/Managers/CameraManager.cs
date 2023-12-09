using DG.Tweening;
using Internal.Core;
using UnityEngine;
using Random = UnityEngine.Random;

public class CameraManager : Singleton<CameraManager>
{
    [SerializeField] public Camera gameCamera;
    [SerializeField] public Camera uiCamera;
    [SerializeField] private Transform shakePivot;

    public float OrtoSize
    {
        set
        {
            gameCamera.orthographicSize = value;
#if CREATIVE
            gameCamera.orthographicSize += Const.THIS.creativeSettings.addedFov;
            gameCamera.transform.localEulerAngles = Const.THIS.creativeSettings.addedCamAngle;
#endif
        }
        get => gameCamera.orthographicSize;
    }

    public void Shake(float amplitude = 1.0f, float duration = 0.35f)
    {
        shakePivot.DOKill();
        shakePivot.localRotation = Quaternion.identity;
        shakePivot.DOPunchRotation(Random.insideUnitSphere.normalized * amplitude, duration).SetEase(Ease.InOutSine);
    }
}
