using DG.Tweening;
using Internal.Core;
using TMPro;
using UnityEngine;

namespace Game
{
    public class Pawn : MonoBehaviour
    {
        [SerializeField] public MeshRenderer meshRenderer;
        [SerializeField] public TextMeshPro levelText;
        [SerializeField] public Transform modelPivot;

        [System.NonSerialized] private Transform _thisTransform;
        [System.NonSerialized] public Block ParentBlock;
        [System.NonSerialized] private int _level = 1;
        [System.NonSerialized] public int MovedAtTick = -1;
        [System.NonSerialized] private bool _mover = false;
        [System.NonSerialized] public bool MoveUntilForward = false;
        [System.NonSerialized] public bool UpcomingMover = false;
        [System.NonSerialized] public bool CanShoot = false;
        [System.NonSerialized] public bool Merger = false;
        
        [System.NonSerialized] private static readonly Vector3 BulletPsUp = new Vector3(0.0f, 0.9f, 0.0f);
        
        public bool Connected => ParentBlock;

        public int Level 
        { 
            get => this._level;
            set
            {
                this._level = value;
                levelText.text = value <= 1 ? "AMMO".ToTMProKey() : _level.ToString();
            }
        }
        
        public void Deconstruct()
        {
            _thisTransform.DOKill();
            modelPivot.DOKill();
            modelPivot.localScale = Vector3.one;

            ParentBlock = null;
            _mover = false;
            MoveUntilForward = false;
            CanShoot = false;
            Merger = false;
            UpcomingMover = false;
            MovedAtTick = -1;
            this.Despawn();
        }
        public void Move(Vector3 position, float duration, Ease ease, System.Action complete = null)
        {
            _thisTransform.DOKill();
            _thisTransform.DOMove(position, duration).SetEase(ease)
                .onComplete += () =>
                {
                    complete?.Invoke();
                };
        }
        public void Move(Transform parent, Vector3 position)
        {
            _thisTransform.parent = parent;
            _thisTransform.position = position;
        }

        #region Colors
        public void MarkSpawnColor()
        {
            meshRenderer.SetColor(GameManager.MPB_PAWN, "_BaseColor", Const.THIS.spawnColor);
        }
        public void MarkMoverColor()
        {
            meshRenderer.SetColor(GameManager.MPB_PAWN, "_BaseColor", Const.THIS.moverColor);
        }
        public void MarkSteadyColor()
        {
            meshRenderer.SetColor(GameManager.MPB_PAWN, "_BaseColor", Const.THIS.steadyColor);
        }
        public void MarkMergeColor()
        {
            meshRenderer.SetColor(GameManager.MPB_PAWN, "_BaseColor", Const.THIS.bigColor);
        }
        public void MarkEnemyColor()
        {
            meshRenderer.SetColor(GameManager.MPB_PAWN, "_BaseColor", Const.THIS.enemyColor);
        }
        public void MarkBiggestColor()
        {
            meshRenderer.SetColor(GameManager.MPB_PAWN, "_BaseColor", Const.THIS.bigColor);
        }
        #endregion
        public void AnimatedShow(float delay, System.Action complete)
        {
            modelPivot.DOKill();
            modelPivot.localScale = Vector3.zero;
            modelPivot.DOScale(Vector3.one, 0.25f).SetDelay(delay).SetEase(Ease.OutBack, 2.0f)
                .onComplete += () => 
                {
                    complete?.Invoke();    
                };
        }
        public void PunchScale(float magnitude)
        {
            modelPivot.DOKill();
            modelPivot.localScale = Vector3.one;
            modelPivot.DOPunchScale(Vector3.one * magnitude, 0.25f);

            Particle.Bullet.Emit(1, _thisTransform.position + BulletPsUp);
            Particle.Ring.Emit(1, _thisTransform.position + BulletPsUp);
        }
        public void Hide(System.Action complete = null)
        {
            modelPivot.DOKill();
            modelPivot.localScale = Vector3.one;
            modelPivot.DOScale(Vector3.zero, 0.1f).SetEase(Ease.Linear)
                .onComplete += () => 
                { 
                    complete?.Invoke();    
                };
            
            Vector3 emitPosition = _thisTransform.position + BulletPsUp;
            Particle.Bullet.Emit(1, emitPosition);
            Particle.Ring.Emit(1, emitPosition);
        }  
        public void Show()
        {
            modelPivot.DOKill();
            modelPivot.localScale = Vector3.zero;
            modelPivot.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack);
        }

        public void MoveForward(Place checkerPlace, int tick, float moveDuration)
        {
            if (UpcomingMover)
            {
                MoveUntilForward = true;
                UpcomingMover = false;
            }
            (Place forwardPlace, bool shouldStay) = ShouldStay(checkerPlace);
            if (shouldStay)
            {
                _mover = false;
                return;
            }
            this.MovedAtTick = tick;
            _mover = true;
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
                    MoveUntilForward = false;
                }
                if (markColor)
                {
                    if (Merger)
                    {
                        MarkMergeColor();
                    }
                    else
                    {
                        MarkSteadyColor();
                    }
                }
                if (Connected)
                {
                    ParentBlock.Detach();
                }
            }
        }
        public (Place, bool) ShouldStay(Place checkerPlace)
        {
            Place forwardPlace = Map.THIS.GetForwardPlace(checkerPlace);

            if (!forwardPlace)
            {
                return (null, true);
            }
            if (forwardPlace.Occupied && !forwardPlace.Current.Connected && !forwardPlace.Current._mover)
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
