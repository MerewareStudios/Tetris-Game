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
    [SerializeField] private MeshCollider meshCollider;
    [SerializeField] private LayerMask spawnerLayer;
    [Header("Locations")]
    [SerializeField] private Transform modelPivot;
    [SerializeField] private Transform spawnedBlockLocation;
    [Header("Input")]
    [SerializeField] private Vector3 distanceFromDraggingFinger;
    [SerializeField] public Vector3 distanceOfBlockCast;

    [System.NonSerialized] private Block _currentBlock;
    [System.NonSerialized] private bool _grabbedBlock = false;
    [System.NonSerialized] private Coroutine _moveRoutine = null;
    [System.NonSerialized] private Vector3 _fingerOffset;
    [System.NonSerialized] private Vector3 _finalPosition;
    [System.NonSerialized] private Tween delayedTween;
    [System.NonSerialized] private Tween assertionTween;
    [System.NonSerialized] private int _blockSpawnCount = 0;
    [System.NonSerialized] private readonly List<Block> _spawnedBlocks = new();

    public void Shake()
    {
        if (_currentBlock)
        {
            _currentBlock.Shake();
        }
    }
    public void Lift()
    {
        if (_currentBlock)
        {
            _currentBlock.Lift();
        }
    }
    public void DelayedSpawn(float delay)
    {
        DOVirtual.DelayedCall(delay, () =>
        {
            _currentBlock = SpawnNextBlock();  
        });
    }
    public void Deconstruct()
    {
        while (_spawnedBlocks.Count > 0)
        {
            Block block = _spawnedBlocks[^1];
            block.DeconstructAnimated();
            RemoveBlock(block);   
        }
        
        StopAllRunningTasksOnBlock();
    }

    private void StopAllRunningTasksOnBlock()
    {
        delayedTween?.Kill();

        assertionTween?.Kill();
        StopMovement();

        _currentBlock = null;
    }
    public void OnLevelLoad()
    {
        _blockSpawnCount = 0;
    }

    #region User Input
    private bool IsTouchingSpawner(Vector3 screenPosition)
    {
        Vector3 touchWorld = CameraManager.THIS.gameCamera.ScreenToWorldPoint(screenPosition);
        Vector3 direction = CameraManager.THIS.gameCamera.transform.forward;
        return Physics.Raycast(touchWorld, direction, 100.0f, spawnerLayer);
    }
    public void Input_OnDown()
    {
        if (!IsTouchingSpawner(Input.mousePosition) || !_currentBlock)
        {
            return;
        }

        assertionTween = DOVirtual.DelayedCall(0.2f, null);
        assertionTween.onComplete = () =>
        {
            _grabbedBlock = true;



            CalculateFingerOffset();
            UpdateTargetPosition();
            
            _moveRoutine = StartCoroutine(MoveRoutine());
            
            IEnumerator MoveRoutine()
            {
                if (_currentBlock)
                {
                    _currentBlock.OnPickUp();
                    _currentBlock.CancelLift();
            
                    if (ONBOARDING.TEACH_PICK.IsNotComplete())
                    {
                        UIManager.THIS.finger.Hide();
                    }
                }

                float smoothFactor = 0.0f;
                while (true)
                {
                    _currentBlock.transform.position = Vector3.Lerp(_currentBlock.transform.position, _finalPosition, Time.deltaTime * 24.0f * smoothFactor);
                    smoothFactor = Mathf.Lerp(smoothFactor, 1.0f, Time.deltaTime * 10.0f);
                    yield return null;
                }
            }
        };
    }
    public void Input_OnClick()
    {
        if (!IsTouchingSpawner(Input.mousePosition) || !_currentBlock || _grabbedBlock)
        {
            return;
        }

        if (!_currentBlock.Busy)
        {
            AnimateTap();
            _currentBlock.Rotate();
            
            if (ONBOARDING.TEACH_PICK.IsComplete() && ONBOARDING.LEARN_ROTATION.IsNotComplete())
            {
                ONBOARDING.LEARN_ROTATION.SetComplete();
                UIManager.THIS.finger.Hide();
            }
        }
    }
    public void Input_OnDrag()
    {
        if (!_currentBlock)
        {
            return;
        }
        if (assertionTween != null)
        {
            assertionTween.Kill(true);
            assertionTween = null;
        }
        if (!_grabbedBlock)
        {
            return;
        }

        UpdateTargetPosition();


        Board.THIS.Dehighlight();
        Board.THIS.HighlightPawnOnGrid(_currentBlock);
    }

    private void CalculateFingerOffset()
    {
        // Recalculate of the mesh collider position changes
        Vector3 startPosition = new Vector3(0.00f, 0.26f, -3.30f);
        // Vector3 startPosition = Vector3.zero;
        // Transform cameraTransform = CameraManager.THIS.gameCamera.transform;
        // Ray ray = new Ray(_currentBlock.transform.position + cameraTransform.forward * -10.0f, cameraTransform.forward);
        // if (meshCollider.Raycast(ray, out RaycastHit hit, 100.0f))
        // {
        //     startPosition = hit.point;
        //     Debug.Log(startPosition);
        // }
            
        Vector3 worldPosition = CameraManager.THIS.gameCamera.ScreenToWorldPoint(Input.mousePosition);
        if (meshCollider.Raycast(new Ray(worldPosition, CameraManager.THIS.gameCamera.transform.forward), out RaycastHit hit, 100.0f))
        {
            _fingerOffset = hit.point - startPosition;
            // _fingerOffset.y = 0.0f;
            // _fingerOffset.z = 0.0f;
        }
    }
    private void UpdateTargetPosition()
    {
        Vector3 worldPosition = CameraManager.THIS.gameCamera.ScreenToWorldPoint(Input.mousePosition);
        if (meshCollider.Raycast(new Ray(worldPosition, CameraManager.THIS.gameCamera.transform.forward), out RaycastHit hit, 100.0f))
        {
            Vector3 targetPosition = hit.point + distanceFromDraggingFinger;
            _finalPosition = targetPosition - _fingerOffset;
        }
    }
    public void Input_OnUp()
    {
        if (!GameManager.PLAYING)
        {
            return;
        }
        assertionTween?.Kill();
        StopMovement();
        if (!_grabbedBlock || _currentBlock == null)
        {
            return;
        }
        _grabbedBlock = false;
        Board.THIS.Dehighlight();

        if (Board.THIS.CanPlace(_currentBlock))
        {
            Board.THIS.Place(_currentBlock);

            _currentBlock = null;
            
            Board.THIS.HideSuggestedPlaces();

            if (ONBOARDING.ALL_BLOCK_STEPS.IsComplete())
            {
                delayedTween?.Kill();
                delayedTween = DOVirtual.DelayedCall(0.08f, () =>
                {
                    _currentBlock = SpawnNextBlock(); // spawn the next block with delay
                });
                
                return;
            }
            
            if (ONBOARDING.TEACH_PICK.IsNotComplete())
            {
                ONBOARDING.TEACH_PICK.SetComplete();
            }

            if (ONBOARDING.TEACH_PLACEMENT.IsNotComplete())
            {
                
                ONBOARDING.TEACH_PLACEMENT.SetComplete();

                Onboarding.SpawnSecondBlockAndTeachRotation();

                return;
            }
            
            if (ONBOARDING.TALK_ABOUT_MERGE.IsNotComplete())
            {
                Onboarding.TalkAboutMerge();
                ONBOARDING.TALK_ABOUT_MERGE.AutoComplete();
                return;
            }
            
            return;
        }
        
        if (ONBOARDING.TEACH_PICK.IsNotComplete())
        {
            Onboarding.ClickOn(this.transform.position, true, null);
        }

        Mount();
    }
    #endregion

    private void Mount()
    {
        _currentBlock.Move(spawnedBlockLocation.position + _currentBlock.spawnerOffset, 25.0f, Ease.OutQuad, true);
    }

    private void StopMovement()
    {
        if (_moveRoutine == null) return;
        StopCoroutine(_moveRoutine);
        _moveRoutine = null;
    }

    private void AnimateTap()
    {
        modelPivot.DOKill();
        modelPivot.localPosition = Vector3.zero;
        modelPivot.DOPunchPosition(Const.THIS.jumpPower, Const.THIS.jumpDuration, 1, 1);
    }

    #region Spawn

    private Block SpawnNextBlock()
    {
        Board.SuggestedBlock[] suggestedBlocks = null;

        if (ONBOARDING.TALK_ABOUT_MERGE.IsNotComplete())
        {
            suggestedBlocks = LevelManager.THIS.GetSuggestedBlocks();
        }

        Board.SuggestedBlock suggestedBlockData = null;
        Pool pool;
        Board.BlockRot blockRot = Board.BlockRot.UP;
        
        if (suggestedBlocks != null && suggestedBlocks.Length > _blockSpawnCount)
        {
            suggestedBlockData = suggestedBlocks[_blockSpawnCount];
            pool = suggestedBlockData.type;
            blockRot = suggestedBlockData.blockRot;
        }
        else
        {
            pool = this.RandomBlock();
        }
        
        Block block = pool.Spawn<Block>(spawnedBlockLocation);

        block.RequiredIndexes = suggestedBlockData?.requiredPlaces;
        block.canRotate = suggestedBlockData?.canRotate ?? true;
        
        Transform blockTransform = block.transform;
        blockTransform.localScale = Vector3.one;
        blockTransform.localPosition = block.spawnerOffset;
        block.Construct(pool);
        block.Rotation = blockRot;
        _spawnedBlocks.Add(block);


        _blockSpawnCount++;
        
        Board.THIS.ShowSuggestedPlaces(block);
        
        return block;
    } 
    private Block SpawnBlock(Pool pool, Pawn.Usage usage)
    {
        Block block = pool.Spawn<Block>(spawnedBlockLocation);
        Transform blockTransform = block.transform;
        blockTransform.localScale = Vector3.one;
        blockTransform.localPosition = block.spawnerOffset;
        blockTransform.localRotation = Quaternion.identity;
        block.Construct(pool, usage);
        _spawnedBlocks.Add(block);
        return block;
    }

    public void InterchangeBlock(Pool pool, Pawn.Usage usage)
    {
        StopAllRunningTasksOnBlock();  
        _currentBlock = SpawnBlock(pool, usage);
    }
    public void RemoveBlock(Block block)
    {
        _spawnedBlocks.Remove(block);
    }
    public Pawn SpawnPawn(Transform parent, Vector3 position, int level, Pawn.Usage usageType)
    {
        Pawn pawn = Pool.Pawn.Spawn<Pawn>(parent);
        Transform pawnTransform = pawn.transform;
        pawnTransform.position = position;
        pawnTransform.localRotation = Quaternion.identity;
        pawnTransform.localScale = Vector3.one;
        pawn.UsageType = usageType;
        pawn.Amount = level;
        return pawn;
    }
    #endregion
}
