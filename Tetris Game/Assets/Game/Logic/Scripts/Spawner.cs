using DG.Tweening;
using Game;
using Internal.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Spawner : Singleton<Spawner>
{
    [Header("Layers")]
    [SerializeField] private LayerMask spawnerLayer;
    [SerializeField] private LayerMask gridcheckLayer;
    [Header("Locations")]
    [SerializeField] private Transform modelPivot;
    [SerializeField] private Transform spawnedBlockLocation;
    [Header("Input")]
    [SerializeField] private Vector3 distanceFromDraggingFinger;

    [SerializeField] private float horSense = 1.5f;

    [System.NonSerialized] private Block currentBlock;
    [System.NonSerialized] private bool GrabbedBlock = false;
    [System.NonSerialized] private Coroutine moveRoutine = null;
    [System.NonSerialized] private bool moving = false;
    [System.NonSerialized] private Vector3 finalPosition;

    public void Begin()
    {
        currentBlock = SpawnBlock();  
    }
    public void Deconstruct()
    {
        while (spawnedBlocks.Count > 0)
        {
            DespawnBlock(spawnedBlocks[^1]);   
        }
    }

    #region User Input
    public bool IsTouchingSpawner(Vector3 screenPosition)
    {
        Vector3 touchWorld = CameraManager.THIS.gameCamera.ScreenToWorldPoint(screenPosition);
        Vector3 direction = CameraManager.THIS.gameCamera.transform.forward;
        if (Physics.Raycast(touchWorld, direction, 100.0f, spawnerLayer))
        {
            return true;
        }
        return false;
    }
    public void ScreenDown(Vector3 screenPosition)
    {
        if (IsTouchingSpawner(screenPosition) && currentBlock != null)
        {
            GrabbedBlock = true;
            moveRoutine = StartCoroutine(MoveRoutine());
        }
    }
    public void ScreenTap(Vector3 screenPosition)
    {
        AnimateTap();
        if (GrabbedBlock)
        {
            // Map.THIS.grid.PuffLastLines(currentBlock.NextWidth);

            currentBlock.Rotate(() =>
            {
            });
        }
    }
    public void Move(Vector3 touchPosition)
    {
        if (!GrabbedBlock)
        {
            return;
        }

        finalPosition = spawnedBlockLocation.position;

        Vector2 viewPortTouch = CameraManager.THIS.gameCamera.ScreenToViewportPoint(touchPosition);
        float distanceMultiplier = (Mathf.Abs(viewPortTouch.x - 0.5f) + 1.0f) * horSense;

        Vector3 worldPosition = CameraManager.THIS.gameCamera.ScreenToWorldPoint(touchPosition);
        worldPosition.x *= distanceMultiplier;

        if (Physics.Raycast(worldPosition, CameraManager.THIS.gameCamera.transform.forward, out RaycastHit hit, 100.0f, gridcheckLayer))
        {
            finalPosition = hit.point + distanceFromDraggingFinger;
        }

        moving = true;


        Map.THIS.Dehighlight();
        Map.THIS.HighlightPawnOnGrid(currentBlock);
    }
    public void Release()
    {
        if (moveRoutine != null)
        {
            moving = false;
            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }
        if (!GrabbedBlock || currentBlock == null)
        {
            return;
        }
        GrabbedBlock = false;
        Map.THIS.Dehighlight();

        if (Map.THIS.CanPlaceBlockOnGrid(currentBlock))
        {
            Map.THIS.PlaceBlockOnGrid(currentBlock);

            currentBlock = SpawnBlock();
            // Map.THIS.grid.PuffLastLines(currentBlock.Width);
            return;
        }

        Mount();
    }
    #endregion

    private void Mount()
    {
        currentBlock.Move(spawnedBlockLocation.position + currentBlock.spawnerOffset, 25.0f, Ease.OutQuad, true);
    }

    private IEnumerator MoveRoutine()
    {
        while (true)
        {
            if (moving)
            {
                currentBlock.transform.position = Vector3.Lerp(currentBlock.transform.position, finalPosition, Time.deltaTime * 22.0f);
            }
            yield return null;
        }
    }

    private void AnimateTap()
    {
        modelPivot.DOKill();
        modelPivot.localPosition = Vector3.zero;
        modelPivot.DOPunchPosition(Vector3.down * 0.35f, 0.25f, 1, 1);
    }

    #region Spawn

    private List<Block> spawnedBlocks = new();
    public Block SpawnBlock()
    {
        Pool pool = GameManager.THIS.Constants.blocks.Random<Pool>();
        Block block = pool.Spawn<Block>(spawnedBlockLocation);
        block.transform.localPosition = block.spawnerOffset;
        block.transform.localScale = Vector3.one;
        block.transform.localRotation = Quaternion.identity;
        block.Construct();
        spawnedBlocks.Add(block);
        return block;
    } 
    public void DespawnBlock(Block block)
    {
        block.Deconstruct();
        spawnedBlocks.Remove(block);
    }
    public Pawn SpawnPawn(Transform parent, Vector3 position, int level)
    {
        Pawn pawn = Pool.Pawn.Spawn<Pawn>(parent);
        pawn.transform.position = position;
        pawn.transform.localRotation = Quaternion.identity;
        pawn.transform.localScale = Vector3.one;
        pawn.Construct(level);
        return pawn;
    }
    #endregion
}
