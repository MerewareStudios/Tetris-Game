using DG.Tweening;
using Game;
using Internal.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class Spawner : Singleton<Spawner>
{
    [Header("Layers")]
    [SerializeField] private LayerMask spawnerLayer;
    [SerializeField] private LayerMask gridCheckLayer;
    [Header("Locations")]
    [SerializeField] private Transform modelPivot;
    [SerializeField] private Transform spawnedBlockLocation;
    [Header("Input")]
    [SerializeField] private Vector3 distanceFromDraggingFinger;

    [SerializeField] private float horSense = 1.5f;

    [System.NonSerialized] private Block _currentBlock;
    [System.NonSerialized] private bool _grabbedBlock = false;
    [System.NonSerialized] private Coroutine _moveRoutine = null;
    [System.NonSerialized] private bool _moving = false;
    [System.NonSerialized] private Vector3 _finalPosition;

    public void Begin()
    {
        _currentBlock = SpawnBlock();  
    }
    public void Deconstruct()
    {
        while (_spawnedBlocks.Count > 0)
        {
            Block block = _spawnedBlocks[^1];
            if (!block.PlacedOnGrid)
            {
                block.DeconstructChildren();
            }
            block.Deconstruct();
            RemoveBlock(block);   
        }
    }

    #region User Input
    private bool IsTouchingSpawner(Vector3 screenPosition)
    {
        Vector3 touchWorld = CameraManager.THIS.gameCamera.ScreenToWorldPoint(screenPosition);
        Vector3 direction = CameraManager.THIS.gameCamera.transform.forward;
        return Physics.Raycast(touchWorld, direction, 100.0f, spawnerLayer);
    }
    public void ScreenDown(Vector3 screenPosition)
    {
        if (!IsTouchingSpawner(screenPosition) || !_currentBlock)
        {
            return;
        }
        _grabbedBlock = true;
        _moveRoutine = StartCoroutine(MoveRoutine());
    }
    public void ScreenTap(Vector3 screenPosition)
    {
        AnimateTap();
        if (_grabbedBlock)
        {
            _currentBlock.Rotate();
        }
    }
    public void Move(Vector3 touchPosition)
    {
        if (!_grabbedBlock)
        {
            return;
        }

        _finalPosition = spawnedBlockLocation.position;

        Vector2 viewPortTouch = CameraManager.THIS.gameCamera.ScreenToViewportPoint(touchPosition);
        float distanceMultiplier = (Mathf.Abs(viewPortTouch.x - 0.5f) + 1.0f) * horSense;

        Vector3 worldPosition = CameraManager.THIS.gameCamera.ScreenToWorldPoint(touchPosition);
        worldPosition.x *= distanceMultiplier;

        if (Physics.Raycast(worldPosition, CameraManager.THIS.gameCamera.transform.forward, out RaycastHit hit, 100.0f, gridCheckLayer))
        {
            _finalPosition = hit.point + distanceFromDraggingFinger;
        }

        _moving = true;


        Map.THIS.Dehighlight();
        Map.THIS.HighlightPawnOnGrid(_currentBlock);
    }
    public void Release()
    {
        if (_moveRoutine != null)
        {
            _moving = false;
            StopCoroutine(_moveRoutine);
            _moveRoutine = null;
        }
        if (!_grabbedBlock || _currentBlock == null)
        {
            return;
        }
        _grabbedBlock = false;
        Map.THIS.Dehighlight();

        if (Map.THIS.CanPlaceBlockOnGrid(_currentBlock))
        {
            Map.THIS.PlaceBlockOnGrid(_currentBlock);

            _currentBlock = SpawnBlock();
            return;
        }

        Mount();
    }
    #endregion

    private void Mount()
    {
        _currentBlock.Move(spawnedBlockLocation.position + _currentBlock.spawnerOffset, 25.0f, Ease.OutQuad, true);
    }

    private IEnumerator MoveRoutine()
    {
        while (true)
        {
            if (_moving)
            {
                _currentBlock.transform.position = Vector3.Lerp(_currentBlock.transform.position, _finalPosition, Time.deltaTime * 22.0f);
            }
            yield return null;
        }
    }

    private void AnimateTap()
    {
        modelPivot.DOKill();
        modelPivot.localPosition = Vector3.zero;
        modelPivot.DOPunchPosition(Const.THIS.jumpPower, Const.THIS.jumpDuration, 1, 1);
    }

    #region Spawn

    private List<Block> _spawnedBlocks = new();
    private Block SpawnBlock()
    {
        Pool pool = Const.THIS.blocks.Random<Pool>();
        Block block = pool.Spawn<Block>(spawnedBlockLocation);
        Transform blockTransform = block.transform;
        blockTransform.localScale = Vector3.one;
        blockTransform.localPosition = block.spawnerOffset;
        blockTransform.localRotation = Quaternion.identity;
        block.Construct();
        _spawnedBlocks.Add(block);
        return block;
    } 
    public void RemoveBlock(Block block)
    {
        _spawnedBlocks.Remove(block);
    }
    public Pawn SpawnPawn(Transform parent, Vector3 position, int level)
    {
        Pawn pawn = Pool.Pawn.Spawn<Pawn>(parent);
        Transform pawnTransform = pawn.transform;
        pawnTransform.position = position;
        pawnTransform.localRotation = Quaternion.identity;
        pawnTransform.localScale = Vector3.one;
        pawn.Level = level;
        return pawn;
    }
    #endregion
}
