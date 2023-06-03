using DG.Tweening;
using Internal.Core;
using RootMotion.Demos;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace Game
{
    public class Pawn : MonoBehaviour
    {
        [SerializeField] public MeshRenderer meshRenderer;
        [SerializeField] public TextMeshPro levelText;
        [SerializeField] public Transform modelPivot;

        [System.NonSerialized] public Block parentBlock;
        [System.NonSerialized] private int level = 1;
        [System.NonSerialized] public int movedAtTick = -1;
        [System.NonSerialized] public bool Mover = false;
        [System.NonSerialized] public bool MoveUntilForward = false;
        [System.NonSerialized] public bool CanShoot = false;
        public bool Connected { get { return parentBlock != null; } }
        public int Level 
        { 
            get 
            { 
                return this.level;
            }
            set
            {
                this.level = value;
                levelText.text = value == 1 ? "AMMO".ToTMProKey() : level.ToString();
            }
        }

        public void Construct(int level)
        {
            Level = level;
            movedAtTick = -1;
            modelPivot.DOKill();
            modelPivot.localScale = Vector3.one;
        }
        public void Deconstruct()
        {
            parentBlock = null;
            Mover = false;
            MoveUntilForward = false;
            CanShoot = false;
            this.Despawn();
        }
        public void Move(Vector3 position, float duration, Ease ease, System.Action OnComplete = null)
        {
            transform.DOKill();
            transform.DOMove(position, duration).SetEase(ease)
                .onComplete += () =>
                {
                    OnComplete?.Invoke();
                };
        }
        public void Move(Transform parent, Vector3 position)
        {
            transform.parent = parent;
            transform.position = position;
        }

        #region Colors
        public void MarkSpawnColor()
        {
            meshRenderer.SetColor(GameManager.MPB_PAWN, "_BaseColor", GameManager.THIS.Constants.spawnColor);
        }
        public void MarkMoverColor()
        {
            meshRenderer.SetColor(GameManager.MPB_PAWN, "_BaseColor", GameManager.THIS.Constants.moverColor);
        }
        public void MarkSteadyColor()
        {
            meshRenderer.SetColor(GameManager.MPB_PAWN, "_BaseColor", GameManager.THIS.Constants.steadyColor);
        }
        public void MarkEnemyColor()
        {
            meshRenderer.SetColor(GameManager.MPB_PAWN, "_BaseColor", GameManager.THIS.Constants.enemyColor);
        }
        public void MarkBiggestColor()
        {
            meshRenderer.SetColor(GameManager.MPB_PAWN, "_BaseColor", GameManager.THIS.Constants.bigColor);
        }
        #endregion
        public void AnimatedShow(float delay, System.Action OnComplete)
        {
            modelPivot.DOKill();
            modelPivot.localScale = Vector3.zero;
            modelPivot.DOScale(Vector3.one, 0.25f).SetDelay(delay).SetEase(Ease.OutBack, 2.0f)
                .onComplete += () => 
                {
                    OnComplete?.Invoke();    
                };
        }
        public void PunchScale(float magnitude)
        {
            modelPivot.DOKill();
            modelPivot.localScale = Vector3.one;
            modelPivot.DOPunchScale(Vector3.one * magnitude, 0.25f);
        }
        public void Hide(System.Action OnComplete = null)
        {
            modelPivot.DOKill();
            modelPivot.localScale = Vector3.one;
            modelPivot.DOScale(Vector3.zero, 0.25f).SetEase(Ease.Linear)
                .onComplete += () => 
                { 
                    OnComplete?.Invoke();    
                };
        }  
        public void Show()
        {
            modelPivot.DOKill();
            modelPivot.localScale = Vector3.zero;
            modelPivot.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack);
        }

        public void MoveForward(Place checkerPlace, int tick, float moveDuration)
        {
            (Place forwardPlace, bool shouldStay) = ShouldStay(checkerPlace);
            if (shouldStay)
            {
                Mover = false;
                return;
            }
            this.movedAtTick = tick;
            Mover = true;
            CanShoot = false;
            forwardPlace.Accept(this, moveDuration, () =>
            {
                checkerPlace.Current = null;
                CanShoot = true;
            });
        }
        public void CheckSteady(Place checkerPlace, bool markColor)
        {
            (Place forwardPlace, bool shouldStay) = ShouldStay(checkerPlace);

            if (shouldStay)
            {
                if (MoveUntilForward)
                {
                    Map.THIS.grid.SetFrontFree(checkerPlace.index.x, false);
                }
                MoveUntilForward = false;
                if (markColor)
                {
                    MarkSteadyColor();
                }
                if (Connected)
                {
                    parentBlock.Detach();
                }
            }
        }
        public (Place, bool) ShouldStay(Place checkerPlace)
        {
            Place forwardPlace = Map.THIS.GetForwardPlace(checkerPlace);

            if (forwardPlace == null)
            {
                return (null, true);
            }
            if (forwardPlace.Occupied && !forwardPlace.Current.Connected && !forwardPlace.Current.Mover)
            {
                return (forwardPlace, true);
            }
            if (MoveUntilForward && Map.THIS.grid.IsFrontFree(checkerPlace.index.x))
            {
                return (forwardPlace, false);
            }
            if (!Connected)
            {
                return (forwardPlace, true);
            }

            return (forwardPlace, false);
        }
    }
}
