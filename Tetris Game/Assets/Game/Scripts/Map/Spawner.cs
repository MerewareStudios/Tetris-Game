using DG.Tweening;
using Game;
using Internal.Core;
using System.Collections;
using System.Collections.Generic;
using IWI.Tutorial;
using Lofelt.NiceVibrations;
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
    [SerializeField] private RectTransform seeLeftover;
    [SerializeField] private int leftOverCount = 0;
    private const int MaxLeftOverCount = 25;
    [SerializeField] private float spawnDelay = 0.45f;
    [SerializeField] private float liftPitch = 1.0f;
    [SerializeField] private float mountPitch = 1.0f;
    [SerializeField] private float placePitch = 1.0f;
   
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
    

    public void SetNextBlockVisibility(bool visible)
    {
        if(visible)
        {
            leftOverCount = MaxLeftOverCount;
            DisplayNextBlock();
        }
        nextBlockVisual.SetActive(true);
        nextBlockVisual.transform.DOKill();
        nextBlockVisual.transform.DOScale(visible ? Vector3.one : Vector3.zero, 0.25f).SetEase(visible ? Ease.OutBack : Ease.InBack).onComplete =
            () =>
            {
                nextBlockVisual.SetActive(visible);
            };
    }

    public bool NextBlockVisible => nextBlockVisual.activeSelf;
    
    private void Awake()
    {
        _plane = new Plane(Vector3.up, Vector3.zero);

    }

    public Vector3 MountPosition => spawnedBlockLocation.position + CurrentBlock.blockData.spawnerOffset;
    public Vector3 BlockSpawnerOffset => CurrentBlock.blockData.spawnerOffset;
    public Vector3 HitPoint(Ray ray) => _plane.Raycast(ray, out float enter) ? ray.GetPoint(enter) : Vector3.zero;
    
    public void UpdatePosition(Vector3 pivot)
    {
        transform.position = HitPoint(new Ray(pivot, CameraManager.THIS.gameCamera.transform.forward));
#if CREATIVE
        transform.position += Vector3.forward * Const.THIS.creativeSettings.bottomOffset;
#endif
    }
    public void UpdateFingerDelta(Vector3 pivot)
    {
#if CREATIVE
        distanceFromDraggingFinger.z = (pivot.z - transform.position.z) * 0.5f;
        return;
#endif
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

    public void Spawn()
    {
        CurrentBlock = SpawnSuggestedBlock();
    }
    public void DelayedSpawn(float delay)
    {
#if CREATIVE
        if (_spawnIndex == 0)
        {
            delay = Const.THIS.creativeSettings.firstBlockSpawnDelay;
        }
        else
        {
            delay = Const.THIS.creativeSettings.genericBlockSpawnDelay;
        }
#endif
        if (_delayedTween != null && _delayedTween.IsPlaying())
        {
            return;
        }
        StopDelayedSpawn();
        _delayedTween = DOVirtual.DelayedCall(delay, () =>
        {
            CurrentBlock = SpawnSuggestedBlock();
            _delayedTween = null;
        }, false);
    }

    private void StopDelayedSpawn()
    {
        _delayedTween?.Kill();
        _delayedTween = null;
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
        StopDelayedSpawn();
        _assertionTween?.Kill();
        StopMovement();
        Mount();
        GrabbedBlock = false;
        Board.THIS.HighlightPlaces();
    }
    public void OnLevelLoad()
    {
        _spawnIndex = 0;
        _nextBlock = GetNexRandomBlock();
    }
    private void StopAllRunningTasksOnBlock()
    {
        StopDelayedSpawn();
        _assertionTween?.Kill();
        StopMovement();

        CurrentBlock = null;
    }

    private Pool GetNexRandomBlock()
    {
#if CREATIVE
        if (!Const.THIS.creativeSettings.randomBlock)
        {
            return Const.THIS.creativeSettings.blocks[_spawnIndex];
        }
#endif
        return this.RandomBlock();
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
            
                    if (ONBOARDING.DRAG_AND_DROP.IsNotComplete())
                    {
                        Onboarding.HideFinger();
                    }
                    else if (ONBOARDING.BLOCK_ROTATION.IsNotComplete())
                    {
                        Onboarding.HideFinger();
                    }
                }

                Audio.Spawner_User_Interaction.PlayOneShotPitch(0.5f, liftPitch);

                
                float smoothFactor = 0.0f;
                while (true)
                {
                    CurrentBlock.transform.position = Vector3.Lerp(CurrentBlock.transform.position, _finalPosition, Time.deltaTime * 34.0f * smoothFactor);
                    smoothFactor = Mathf.Lerp(smoothFactor, 1.25f, Time.deltaTime * _smoothFactorLerp);
                    Board.THIS.HighlightPlaces();
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
        
        HapticManager.Vibrate(HapticPatterns.PresetType.Selection);

        
        AnimateTap();
        if (CurrentBlock.CanRotate)
        {
            Audio.Rotate_Click.PlayOneShot();
            CurrentBlock.Rotate();
        }
        
            
        if (ONBOARDING.DRAG_AND_DROP.IsComplete() && ONBOARDING.BLOCK_ROTATION.IsNotComplete())
        {
            ONBOARDING.BLOCK_ROTATION.SetComplete();
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
// #if CREATIVE
        // Vector3 targetPosition = hitPoint;
// #else
        Vector3 targetPosition = hitPoint + distanceFromDraggingFinger;
// #endif

        _finalPosition = targetPosition - _dragOffset;

        Vector3 drag = _dragOffset + MountPosition - hitPoint;
        _finalPosition -= drag * 0.25f;
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
            HapticManager.Vibrate(HapticPatterns.PresetType.LightImpact);
            Board.THIS.Place(CurrentBlock);
            Audio.Spawner_User_Interaction.PlayOneShotPitch(0.6f, placePitch);

            CurrentBlock = null;
            
            Board.THIS.HideSuggestedPlaces();


            if (ONBOARDING.ALL_BLOCK_STEPS.IsComplete())
            {
                StopDelayedSpawn();
                DelayedSpawn(spawnDelay);
                return;
            }
            
            if (ONBOARDING.DRAG_AND_DROP.IsNotComplete())
            {
                ONBOARDING.DRAG_AND_DROP.SetComplete();
                Onboarding.SpawnSecondBlockAndTeachRotation();
                Audio.Hint_2.Play();

                return;
            }
            
            if (ONBOARDING.SPEECH_MERGE.IsNotComplete())
            {
                Onboarding.TalkAboutMerge();
                ONBOARDING.SPEECH_MERGE.SetComplete();

                return;
            }
            
            return;
        }
        
        if (ONBOARDING.DRAG_AND_DROP.IsNotComplete())
        {
            Onboarding.DragOn(transform.position, Finger.Cam.Game, Lift, timeIndependent:false);
        }
        
        else if (ONBOARDING.BLOCK_ROTATION.IsNotComplete())
        {
            Onboarding.ClickOn(Spawner.THIS.transform.position, Finger.Cam.Game, Shake, infoEnabled:true, timeIndependent:false);
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
        Audio.Spawner_User_Interaction.PlayOneShotPitch(1.0f, mountPitch);

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
        bool learnedRotation = ONBOARDING.BLOCK_ROTATION.IsComplete();
        Board.SuggestedBlock[] suggestedBlocks = learnedRotation ? null : LevelManager.GetSuggestedBlocks();
        Board.SuggestedBlock suggestedBlockData = null;
        Pool pool;
        if (suggestedBlocks != null && suggestedBlocks.Length > _spawnIndex)
        {
            _smoothFactorLerp = 5.0f;
            suggestedBlockData = suggestedBlocks[_spawnIndex];
            pool = suggestedBlockData.type;
        }
        else
        {
            _smoothFactorLerp = 10.0f;
            
            pool = _nextBlock;
            
            _nextBlock = GetNexRandomBlock();
            if ((_nextBlock.Equals(Pool.Single_Block) || _nextBlock.Equals(Pool.Two_I_Block)) && Helper.IsPossible(0.5f))
            {
                _nextBlock = GetNexRandomBlock();
            }

            if (NextBlockVisible)
            {
                DisplayNextBlock();
            }
        }
        return SpawnBlock(pool, Pawn.Usage.UnpackedAmmo, suggestedBlockData);
    } 
    private Block SpawnBlock(Pool pool, Pawn.Usage usage, Board.SuggestedBlock suggestedBlockData)
    {
        Block block = Pool.Block.Spawn<Block>(spawnedBlockLocation);
        
        block.RequiredPlaces = suggestedBlockData == null ? null : Board.THIS.Index2Place(suggestedBlockData.requiredPlaces);
        block.CanRotate = suggestedBlockData?.canRotate ?? true;
        
        block.Construct(pool, usage);
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
        pawn.SetUsageType(usageType, amount);
        return pawn;
    }
    #endregion


    private void DisplayNextBlock()
    {
        leftOverCount--;
        if (leftOverCount == 0)
        {
            SetNextBlockVisibility(false);
            return;
        }
        
        List<Transform> segmentTransforms = _nextBlock.Prefab<Block>().segmentTransforms;
        for (int i = 0; i < segmentTransforms.Count; i++)
        {
            nextBlockPawns[i].SetActive(segmentTransforms[i]);
        }
        seeLeftover.DOSizeDelta(new Vector2(100.0f * (leftOverCount / (float)MaxLeftOverCount), 7.24f), 0.2f);
    }
}
