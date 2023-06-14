using System;
using DG.Tweening;
using Internal.Core;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;

namespace Game
{
    public class Pawn : MonoBehaviour
    {
        [SerializeField] public MeshRenderer meshRenderer;
        [SerializeField] public TextMeshPro levelText;
        [SerializeField] public Transform modelPivot;

        [System.NonSerialized] private Tween _moveTween = null;
        [System.NonSerialized] private Transform _thisTransform;
        [System.NonSerialized] public Block ParentBlock;
        [System.NonSerialized] private int _amount = 1;
        [System.NonSerialized] public int Tick;
        
        [System.NonSerialized] private static readonly Vector3 BulletPsUp = new Vector3(0.0f, 0.9f, 0.0f);
        
        public bool Connected => ParentBlock;

        void Awake()
        {
            _thisTransform = transform;
        }

        public int Amount 
        { 
            get => this._amount;
            set
            {
                this._amount = value;
                levelText.text = value <= 1 ? (SHOOTER ? _amount.ToString() : "AMMO".ToTMProKey()) : _amount.ToString();
            }
        }
        
        public void Deconstruct()
        {
            _moveTween?.Kill();
            modelPivot.DOKill();
            modelPivot.localScale = Vector3.one;

            ParentBlock = null;

            SHOOTER = false;
            
            this.Despawn();
        }
        public void Move(Vector3 position, float duration, Ease ease, System.Action complete = null)
        {
            _moveTween?.Kill();
            _moveTween = _thisTransform.DOMove(position, duration).SetEase(ease);
            _moveTween.onComplete += () =>
                {
                    complete?.Invoke();
                };
        }
        public void Set(Transform parent, Vector3 position)
        {
            _thisTransform.parent = parent;
            _thisTransform.position = position;
        }

        #region Colors
        public void MarkDefaultColor()
        {
            meshRenderer.SetColor(GameManager.MPB_PAWN, "_BaseColor", Const.THIS.defaultColor);
        }
        public void MarkSteadyColor()
        {
            meshRenderer.SetColor(GameManager.MPB_PAWN, "_BaseColor", Const.THIS.steadyColor);
        }
        public void MarkMergerColor()
        {
            meshRenderer.SetColor(GameManager.MPB_PAWN, "_BaseColor", Const.THIS.mergerColor);
        }
        // public void MarkMoverColor()
        // {
        //     meshRenderer.SetColor(GameManager.MPB_PAWN, "_BaseColor", Const.THIS.moverColor);
        // }
        // public void MarkEnemyColor()
        // {
        //     meshRenderer.SetColor(GameManager.MPB_PAWN, "_BaseColor", Const.THIS.enemyColor);
        // }
        // public void MarkBiggestColor()
        // {
        //     meshRenderer.SetColor(GameManager.MPB_PAWN, "_BaseColor", Const.THIS.bigColor);
        // }
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


        [System.NonSerialized] public bool MOVER = false;
        [System.NonSerialized] public bool BUSY = false;
        [System.NonSerialized] public bool SHOOTER = false;
        
        public void MoveForward(Place checkerPlace, int tick, float moveDuration)
        {
            if (BUSY)
            {
                return;
            }
            if (!MOVER)
            {
                return;
            }
            
            Tick = tick;
            
            Place forwardPlace = Board.THIS.GetForwardPlace(checkerPlace);
            forwardPlace.Accept(this, moveDuration);
            
            checkerPlace.Current = null;
        }

        public void Check(Place checkerPlace)
        {
            if (BUSY)
            {
                return;
            }
            CheckSteady(checkerPlace);
            CheckDetach();
        }
        private void CheckSteady(Place checkerPlace)
        {
            Place forwardPlace = Board.THIS.GetForwardPlace(checkerPlace);

            if (!forwardPlace) // if at the edge of the map
            {
                MOVER = false;
                return;
            }

            if (forwardPlace.Occupied && !forwardPlace.Current.MOVER) // if front is occupied and no intent of moving
            {
                MOVER = false;
                return;
            }
        }

        private void CheckDetach()
        {
            if (MOVER)
            {
                return;
            }
            if (Connected)
            {
                ParentBlock.Detach();
            }
        }
    }
}
