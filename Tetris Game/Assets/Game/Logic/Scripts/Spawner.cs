using DG.Tweening;
using Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Spawner : MonoBehaviour
{
    [SerializeField] private LayerMask spawnerLayer;
    [SerializeField] private LayerMask gridcheckLayer;
    [SerializeField] private Transform modelPivot;
    [SerializeField] private Vector3 downDistance;
    [SerializeField] private Vector3 touchDistance;
    [SerializeField] private Transform blockParent;
    [SerializeField] private Block currentBlock;
    [SerializeField] private float forwardDistance;

    void Start()
    {
        Spawn();    
    }

    public void Check(Vector3 touchPosition)
    {
        Vector3 touchWorld = CameraManager.THIS.gameCamera.ScreenToWorldPoint(touchPosition);
        Vector3 direction = CameraManager.THIS.gameCamera.transform.forward;
        if (Physics.Raycast(touchWorld, direction, 100.0f, spawnerLayer))
        {
            Tap();
        }
    }
    public void Move(Vector3 touchPosition)
    {
        Vector3 touchWorld = CameraManager.THIS.gameCamera.ScreenToWorldPoint(touchPosition);
        Vector3 direction = CameraManager.THIS.gameCamera.transform.forward * forwardDistance;
        Vector3 finalPosition = touchWorld + direction;

        if (Physics.Raycast(touchWorld, direction, out RaycastHit hit, 100.0f, gridcheckLayer))
        {
            finalPosition = hit.point;
        }

        currentBlock.transform.position = finalPosition + touchDistance;
        currentBlock.Check();
    }
    public void Release(Vector3 touchPosition)
    {
        Spawn();

    }

    private void Tap()
    {
        modelPivot.DOKill();
        modelPivot.localPosition = Vector3.zero;
        modelPivot.DOPunchPosition(downDistance, 0.25f, 1, 1);
    }
   

    private void Spawn()
    {
        currentBlock = Pool.T_Block.Spawn<Block>(blockParent);
        currentBlock.Construct();
    }
}
