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
    [SerializeField] private LayerMask spawnerLayer;
    [Header("Locations")]
    [SerializeField] private Transform modelPivot;
    [SerializeField] private Transform spawnedBlockLocation;
    [Header("Input")]
    [SerializeField] private Vector3 distanceFromDraggingFinger;
    [SerializeField] public Vector3 distanceOfBlockCast;
    [SerializeField] public Vector3 tutorialLift;
    [SerializeField] private GameObject[] nextBlockPawns;
    [SerializeField] private GameObject nextBlockVisual;
    [SerializeField] private RectTransform nextBlockPivot;
   
    [System.NonSerialized] public Block CurrentBlock;
    [System.NonSerialized] private Pool _nextBlock;
    [System.NonSerialized] private Plane _plane;
    [System.NonSerialized] public bool GrabbedBlock = false;
    [System.NonSerialized] private Coroutine _moveRoutine = null;
    [System.NonSerialized] private Vector3 _dragOffset;
    [System.NonSerialized] private Vector3 _finalPosition;
    [System.NonSerialized] private Tween _delayedTween;
    [System.NonSerialized] private Tween _assertionTween;
    [System.NonSerialized] private int _spawnIndex = 0;
    [System.NonSerialized] private readonly List<Block> _spawnedBlocks = new();
    [System.NonSerialized] private float _smoothFactorLerp = 10.0f;

    public bool NextBlockEnabled
    {
        set
        {
            if (nextBlockVisual.activeSelf == value)
            {
                return;
            }
            nextBlockVisual.SetActive(value);
            if (value)
            {
                nextBlockPivot.DOKill();
                nextBlockPivot.position = FakeAdBanner.THIS.ButtonPosition;
                nextBlockPivot.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutSine);
                DisplayNextBlock();
            }
        }
        get => nextBlockVisual.activeSelf;
    }
    
    private void Awake()
    {
        _plane = new Plane(Vector3.up, Vector3.zero);
    }

    public Vector3 MountPosition => spawnedBlockLocation.position + CurrentBlock.blockData.spawnerOffset;
    public Vector3 HitPoint(Ray ray) => _plane.Raycast(ray, out float enter) ? ray.GetPoint(enter) : Vector3.zero;
    
    public void UpdatePosition(Vector3 pivot)
    {
        transform.position = HitPoint(new Ray(pivot, CameraManager.THIS.gameCamera.transform.forward));
    }
    public void UpdateFingerDelta(Vector3 pivot)
    {
        distanceFromDraggingFinger.z = (pivot.z - transform.position.z);
    }

    public void Shake()
    {
        if (CurrentBlock)
        {
            CurrentBlock.ShakeRotation();
        }
    }
    public void Lift()
    {
        if (CurrentBlock)
        {
            CurrentBlock.Lift(tutorialLift);
        }
    }
    public void DelayedSpawn(float delay)
    {
        _delayedTween?.Kill();
        _delayedTween = DOVirtual.DelayedCall(delay, () =>
        {
            CurrentBlock = SpawnSuggestedBlock();  
        }, false);
    }
    public void UndelayedSpawn()
    {
        _delayedTween?.Kill();
        CurrentBlock = SpawnSuggestedBlock();  
    }
    public void Deconstruct()
    {
        while (_spawnedBlocks.Count > 0)
        {
            Block block = _spawnedBlocks[^1];
            block.Deconstruct();
            RemoveBlock(block);   
        }
        _spawnIndex = 0;
        GrabbedBlock = false;
        StopAllRunningTasksOnBlock();
    }
    public void OnLevelEnd()
    {
        _delayedTween?.Kill();
        _assertionTween?.Kill();
        StopMovement();
        Mount();
        GrabbedBlock = false;
        Board.THIS.HighlightPlaces();
    }
    public void OnLevelLoad()
    {
        _spawnIndex = 0;
        _nextBlock = this.RandomBlock();
    }
    private void StopAllRunningTasksOnBlock()
    {
        _delayedTween?.Kill();

        _assertionTween?.Kill();
        StopMovement();

        CurrentBlock = null;
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
        if (!IsTouchingSpawner(Input.mousePosition) || !CurrentBlock)
        {
            return;
        }

        _assertionTween = DOVirtual.DelayedCall(0.2f, null, true);
        _assertionTween.onComplete = () =>
        {
            GrabbedBlock = true;


            RecordFingerStart();
            UpdateTargetPosition();
            
            _moveRoutine = StartCoroutine(MoveRoutine());
            
            IEnumerator MoveRoutine()
            {
                if (CurrentBlock)
                {
                    CurrentBlock.CancelLift();
            
                    if (ONBOARDING.TEACH_PICK.IsNotComplete())
                    {
                        Onboarding.HideFinger();
                    }
                }

                // Vector3 prev = CurrentBlock.transform.position;
                float smoothFactor = 0.0f;
                while (true)
                {
                    CurrentBlock.transform.position = Vector3.Lerp(CurrentBlock.transform.position, _finalPosition, Time.deltaTime * 28.0f * smoothFactor);
                    smoothFactor = Mathf.Lerp(smoothFactor, 1.0f, Time.deltaTime * _smoothFactorLerp);

                    // if ((CurrentBlock.transform.position - prev).sqrMagnitude > 0.15f)
                    // {
                        Board.THIS.HighlightPlaces();
                        // prev = CurrentBlock.transform.position;
                    // }

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
        if (!IsTouchingSpawner(Input.mousePosition) || !CurrentBlock || GrabbedBlock)
        {
            return;
        }

        if (CurrentBlock.Busy)
        {
            return;
        }
        
        AnimateTap();
        CurrentBlock.Rotate();
            
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
        if (!CurrentBlock)
        {
            return;
        }
        if (CurrentBlock.Busy)
        {
            return;
        }
        if (_assertionTween != null)
        {
            _assertionTween.Kill(true);
            _assertionTween = null;
        }
        if (!GrabbedBlock)
        {
            return;
        }

        UpdateTargetPosition();
        // Board.THIS.HighlightPlaces();
    }

    private void RecordFingerStart()
    {
        Vector3 worldPosition = CameraManager.THIS.gameCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3 hitPoint = HitPoint(new Ray(worldPosition, CameraManager.THIS.gameCamera.transform.forward));
        _dragOffset = hitPoint - MountPosition;
    }
    private void UpdateTargetPosition()
    {
        Vector3 worldPosition = CameraManager.THIS.gameCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3 hitPoint = HitPoint(new Ray(worldPosition, CameraManager.THIS.gameCamera.transform.forward));
        Vector3 targetPosition = hitPoint + distanceFromDraggingFinger;
        _finalPosition = targetPosition - _dragOffset;
    }
    public void Input_OnUp()
    {
        InputUpWrap();
        Board.THIS.HighlightPlaces();
    }

    private void InputUpWrap()
    {
        if (Input.touchCount > 1)
        {
            return;
        }
        if (!GameManager.PLAYING)
        {
            return;
        }
        _assertionTween?.Kill();
        StopMovement();
        if (!GrabbedBlock || CurrentBlock == null)
        {
            return;
        }
        GrabbedBlock = false;
        

        if (Board.THIS.CanPlace(CurrentBlock))
        {
            Board.THIS.Place(CurrentBlock);

            CurrentBlock = null;
            
            Board.THIS.HideSuggestedPlaces();


            if (ONBOARDING.ALL_BLOCK_STEPS.IsComplete())
            {
                _delayedTween?.Kill();
                // UndelayedSpawn();
                DelayedSpawn(0.25f);
                
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
        if (!CurrentBlock)
        {
            return;
        }
        CurrentBlock.Move(MountPosition, 25.0f, Ease.OutQuad, true);
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
        if (suggestedBlocks != null && suggestedBlocks.Length > _spawnIndex)
        {
            _smoothFactorLerp = 3.0f;
            suggestedBlockData = suggestedBlocks[_spawnIndex];
            pool = suggestedBlockData.type;
        }
        else
        {
            _smoothFactorLerp = 10.0f;
            
            pool = _nextBlock;
            
            _nextBlock = this.RandomBlock();

            if (NextBlockEnabled)
            {
                DisplayNextBlock();
            }
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

        _spawnIndex++;
        return block;
    }

    public void InterchangeBlock(Pool pool, Pawn.Usage usage)
    {
        DespawnCurrentBlock();
        StopAllRunningTasksOnBlock();  
        Board.THIS.HideSuggestedPlaces();
        CurrentBlock = SpawnBlock(pool, usage, null);
    }
    private void DespawnCurrentBlock()
    {
        if (CurrentBlock)
        {
            CurrentBlock.Deconstruct();
            RemoveBlock(CurrentBlock);   
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


    private void DisplayNextBlock()
    {
        List<Transform> segmentTransforms = _nextBlock.Prefab<Block>().segmentTransforms;

        for (int i = 0; i < segmentTransforms.Count; i++)
        {
            nextBlockPawns[i].SetActive(segmentTransforms[i]);
        }
    }
}
