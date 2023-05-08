
using Internal.Core;
using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    [SerializeField] public Camera mainCamera;
    [SerializeField] public Camera gameCamera;
    [SerializeField] public Camera uiCamera;
}
