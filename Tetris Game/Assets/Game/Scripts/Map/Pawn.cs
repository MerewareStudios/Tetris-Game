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
        
        [System.NonSerialized] public bool MOVER = false;
        [System.NonSerialized] public bool BUSY = false;
        
        [System.NonSerialized] public Pawn.Usage UsageType;
        
        [System.NonSerialized] private static readonly Vector3 BulletPsUp = new Vector3(0.0f, 0.9f, 0.0f);
        
        public bool Connected => ParentBlock;

        public bool TextEnabled
        {
            set => levelText.enabled = value;
        }

        public enum Usage
        {
            Ammo,
            Shooter,
            ShooterIdle,
            Heart,
            Shield,
            Vertical,
            Horizontal,
            Area,
            Speed,
        }
        

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
                TextEnabled = true;
                switch (UsageType)
                {
                    case Usage.Ammo:
                        levelText.text = UsageType.ToString().ToTMProKey(_amount);
                        break;
                    case Usage.Shooter:
                        levelText.text = _amount.ToString();
                        break;
                    case Usage.ShooterIdle:
                        levelText.text = _amount.ToString();
                        break;
                    default:
                        levelText.text = UsageType.ToString().ToTMProKey();
                        break;
                }
            }
        }

        public void OnMerge()
        {
            UsageType = Pawn.Usage.Shooter;
        }

        public bool Unbox(float delay)
        {
            switch (UsageType)
            {
                case Usage.Ammo:
                    
                    return true;
                case Usage.Shooter:
                
                    return true;
                case Usage.ShooterIdle:
                    
                    return true;
                case Usage.Heart:
                    TextEnabled = false;
                    UIManager.THIS.ft_Icon.LerpHearth(levelText.transform.position, delay, 0.65f, endAction: () =>
                    {
                        Warzone.THIS.GiveHeart(_amount);
                    });
                    return false;
                case Usage.Shield:
                    TextEnabled = false;
                    UIManager.THIS.ft_Icon.LerpShield(levelText.transform.position, delay, 0.65f, endAction: () =>
                    {
                        Warzone.THIS.GiveShield(5.0f);
                    });
                    return false;
                case Usage.Vertical:
                    
                case Usage.Horizontal:
                    
                    return false;
                case Usage.Area:
                    
                    return false;
                case Usage.Speed:
                    
                    return false;
            }

            return true;
        }
        
        public void Deconstruct()
        {
            _moveTween?.Kill();
            modelPivot.DOKill();
            modelPivot.localScale = Vector3.one;

            ParentBlock = null;

            // SHOOTER = false;
            
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
        public void AnimatedShow(float delay, System.Action complete = null)
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
            modelPivot.DOPunchScale(Vector3.one * magnitude, 0.25f, 1);

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
