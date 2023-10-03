using DG.Tweening;
using Game;
using Internal.Core;
using System.Collections;
using System.Collections.Generic;
using IWI.Tutorial;
using UnityEngine;

public class Spawner : Singleton<Spawner>
{
    [Header("Layers")]
    // [SerializeField] private MeshCollider meshColliderSpawner;
    [SerializeField] private MeshCollider meshCollider;
    [SerializeField] private LayerMask spawnerLayer;
    [Header("Locations")]
    [SerializeField] private Transform modelPivot;
    [SerializeField] private Transform spawnedBlockLocation;
    [Header("Input")]
    [SerializeField] private Vector3 distanceFromDraggingFinger;
    [SerializeField] public Vector3 distanceOfBlockCast;
    [SerializeField] public Vector3 tutorialLift;
    [SerializeField] public RectTransform spawnerPin;

    public void UpdatePosition()
    {
        transform.position = HitPoint(new Ray(spawnerPin.position, CameraManager.THIS.gameCamera.transform.forward));
    }
    
    public Vector3 HitPoint(Ray ray)
    {
        if (meshCollider.Raycast(ray, out var hit, 100.0f))
        {
            return hit.point;
        }
        return Vector3.zero;
    }

    public void UpdateFingerDelta(Vector3 pivot)
    {
        distanceFromDraggingFinger.z = (pivot.z - transform.position.z);
    }

    [System.NonSerialized] public bool FitColorPass = true;
    [System.NonSerialized] public Block _currentBlock;
    [System.NonSerialized] private bool _grabbedBlock = false;
    [System.NonSerialized] private Coroutine _moveRoutine = null;
    [System.NonSerialized] private Vector3 _dragOffset;
    [System.NonSerialized] private Vector3 _finalPosition;
    [System.NonSerialized] private Tween delayedTween;
    [System.NonSerialized] private Tween assertionTween;
    [System.NonSerialized] public int SpawnIndex = 0;
    [System.NonSerialized] public int SpawnTime = 0;
    [System.NonSerialized] private readonly List<Block> _spawnedBlocks = new();

    public void Shake()
    {
        if (_currentBlock)
        {
            _currentBlock.ShakeRotation();
        }
    }
    public void Lift()
    {
        if (_currentBlock)
        {
            _currentBlock.Lift(tutorialLift);
        }
    }
    public void DelayedSpawn(float delay)
    {
        delayedTween?.Kill();
        delayedTween = DOVirtual.DelayedCall(delay, () =>
        {
            _currentBlock = SpawnSuggestedBlock();  
        }, false);
    }
    public void Deconstruct()
    {
        while (_spawnedBlocks.Count > 0)
        {
            Block block = _spawnedBlocks[^1];
            block.Deconstruct();
            RemoveBlock(block);   
        }

        SpawnIndex = 0;
        StopAllRunningTasksOnBlock();
    }
    public void OnLevelEnd()
    {
        delayedTween?.Kill();
        assertionTween?.Kill();
        StopMovement();
        Mount();
        Board.THIS.HighlightBlock();
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
        SpawnIndex = 0;
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
        if (!GameManager.PLAYING)
        {
            return;
        }
        if (Input.touchCount > 1)
        {
            return;
        }
        if (!IsTouchingSpawner(Input.mousePosition) || !_currentBlock)
        {
            return;
        }

        assertionTween = DOVirtual.DelayedCall(0.2f, null, false);
        assertionTween.onComplete = () =>
        {
            _grabbedBlock = true;


            RecordFingerStart();
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
                        Onboarding.HideFinger();
                    }
                }

                float smoothFactor = 0.0f;
                while (true)
                {
                    _currentBlock.transform.position = Vector3.Lerp(_currentBlock.transform.position, _finalPosition, Time.deltaTime * 28.0f * smoothFactor);
                    smoothFactor = Mathf.Lerp(smoothFactor, 1.0f, Time.deltaTime * 10.0f);
                    yield return null;
                }
            }
        };
    }
    public void Input_OnClick()
    {
        if (!GameManager.PLAYING)
        {
            return;
        }
        if (Input.touchCount > 1)
        {
            return;
        }
        if (!IsTouchingSpawner(Input.mousePosition) || !_currentBlock || _grabbedBlock)
        {
            return;
        }

        if (_currentBlock.Busy)
        {
            return;
        }
        
        AnimateTap();
        _currentBlock.Rotate();
            
        if (ONBOARDING.TEACH_PICK.IsComplete() && ONBOARDING.TEACH_ROTATION.IsNotComplete())
        {
            ONBOARDING.TEACH_ROTATION.SetComplete();
            Onboarding.HideFinger();
        }
    }
    public void Input_OnDrag()
    {
        if (Input.touchCount > 1)
        {
            return;
        }
        if (!_currentBlock)
        {
            return;
        }
        if (_currentBlock.Busy)
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
        HighlightCurrentBlock();
    }

    public void HighlightCurrentBlock()
    {
        if (!_currentBlock)
        {
            return;
        }
        if (!_grabbedBlock)
        {
            return;
        }
        Board.THIS.HighlightBlock(_currentBlock);
    }

    private void RecordFingerStart()
    {
        Vector3 worldPosition = CameraManager.THIS.gameCamera.ScreenToWorldPoint(Input.mousePosition);
        if (meshCollider.Raycast(new Ray(worldPosition, CameraManager.THIS.gameCamera.transform.forward), out RaycastHit hit, 100.0f))
        {
            _dragOffset = hit.point - _currentBlock.transform.position;
        }
    }
    private void UpdateTargetPosition()
    {
        Vector3 worldPosition = CameraManager.THIS.gameCamera.ScreenToWorldPoint(Input.mousePosition);
        
        if (meshCollider.Raycast(new Ray(worldPosition, CameraManager.THIS.gameCamera.transform.forward), out RaycastHit hit, 100.0f))
        {
            Vector3 targetPosition = hit.point + distanceFromDraggingFinger;
            _finalPosition = targetPosition - _dragOffset;
        }
    }
    public void Input_OnUp()
    {
        if (Input.touchCount > 1)
        {
            return;
        }
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
        Board.THIS.HighlightBlock();

        if (Board.THIS.CanPlace(_currentBlock))
        {
            Board.THIS.Place(_currentBlock);
            Board.THIS.HideSuggestedPlaces();

            _currentBlock = null;
            

            if (ONBOARDING.ALL_BLOCK_STEPS.IsComplete())
            {
                delayedTween?.Kill();
                DelayedSpawn(0.2f);
                
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
            Onboarding.DragOn(transform.position, Finger.Cam.Game, Lift);
        }

        Mount();
    }
    #endregion

    private void Mount()
    {
        if (!_currentBlock)
        {
            return;
        }
        _currentBlock.Move(spawnedBlockLocation.position + _currentBlock.blockData.spawnerOffset, 25.0f, Ease.OutQuad, true);
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

    private Block SpawnSuggestedBlock()
    {
        bool learnedRotation = ONBOARDING.TEACH_ROTATION.IsComplete();
        Board.SuggestedBlock[] suggestedBlocks = learnedRotation ? null : LevelManager.THIS.GetSuggestedBlocks();
        Board.SuggestedBlock suggestedBlockData = null;
        Pool pool;
        if (suggestedBlocks != null && suggestedBlocks.Length > SpawnIndex)
        {
            suggestedBlockData = suggestedBlocks[SpawnIndex];
            pool = suggestedBlockData.type;
            FitColorPass = false;
        }
        else
        {
            FitColorPass = true;
            pool = this.RandomBlock();
        }
        
        return SpawnBlock(pool, Pawn.Usage.UnpackedAmmo, suggestedBlockData);
    } 
    private Block SpawnBlock(Pool pool, Pawn.Usage usage, Board.SuggestedBlock suggestedBlockData)
    {
        Block block = pool.Spawn<Block>(spawnedBlockLocation);
        
        block.RequiredPlaces = suggestedBlockData == null ? null : Board.THIS.Index2Place(suggestedBlockData.requiredPlaces);
        block.CanRotate = suggestedBlockData?.canRotate ?? true;
        
        Transform blockTransform = block.transform;
        blockTransform.localScale = Vector3.one;
        blockTransform.localPosition = block.blockData.spawnerOffset;

        block.Construct(usage);
        block.Rotation = suggestedBlockData?.blockRot ?? Board.BlockRot.UP;
        _spawnedBlocks.Add(block);

        SpawnTime = (int)Time.time;
        SpawnIndex++;

        return block;
    }

    public void InterchangeBlock(Pool pool, Pawn.Usage usage)
    {
        DespawnCurrentBlock();
        StopAllRunningTasksOnBlock();  
        Board.THIS.HideSuggestedPlaces();
        _currentBlock = SpawnBlock(pool, usage, null);
    }
    private void DespawnCurrentBlock()
    {
        if (_currentBlock)
        {
            _currentBlock.Deconstruct();
            RemoveBlock(_currentBlock);   
        }    
    }
    public void RemoveBlock(Block block)
    {
        _spawnedBlocks.Remove(block);
    }
    public Pawn SpawnPawn(Transform parent, Vector3 position, int amount, Pawn.Usage usageType)
    {
        Pawn pawn = Pool.Pawn.Spawn<Pawn>(parent);
        Transform pawnTransform = pawn.transform;
        pawnTransform.position = position;
        pawnTransform.rotation = Quaternion.identity;
        pawnTransform.localScale = Vector3.one;
        pawn.UsageType = usageType;
        pawn.Amount = amount;
        return pawn;
    }
    #endregion
}
