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
        [SerializeField] private TextMeshPro levelText;
        [SerializeField] public Transform modelPivot;
        [SerializeField] public Transform pivot;

        [System.NonSerialized] private Tween _moveTween = null;
        [System.NonSerialized] private Tween _delayedTween = null;
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
        public bool CanPlaceAnywhere => UsageType.Equals(Usage.HorMerge);
        public Vector3 TextPosition => levelText.transform.position;

        public enum Usage
        {
            Ammo,
            Shooter,
            ShooterIdle,
            Heart,
            Shield,
            Vertical,
            HorMerge,
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
                        levelText.text = (_amount >= Board.THIS._Data.maxStack) ? "MAX" : _amount.ToString();
                        break;
                    case Usage.ShooterIdle:
                        levelText.text = (_amount >= Board.THIS._Data.maxStack) ? "MAX" : _amount.ToString();
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
                        Warzone.THIS.GiveShield(1);
                    });
                    return false;
                case Usage.Vertical:
                    
                case Usage.HorMerge:
                    
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
            KillTweens();
            
            modelPivot.DOKill();
            modelPivot.localScale = Vector3.one;

            ParentBlock = null;

            this.Despawn();
        }

        private void KillTweens()
        {
            _delayedTween?.Kill();
            _moveTween?.Kill();

        }
        public void OnVictory()
        {
            KillTweens();

            modelPivot.DOKill();
            modelPivot.localScale = Vector3.one;
            modelPivot.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack)
                .onComplete += Deconstruct;

            
            UIManagerExtensions.EarnCurrencyWorld(Const.CurrencyType.Coin, levelText.transform.position, 1.25f, () =>
            {
                Wallet.Transaction(new Const.Currency(Const.CurrencyType.Coin, 1));
            });
        }
        public void OnFail()
        {
            KillTweens();

            modelPivot.DOKill();
            modelPivot.localScale = Vector3.one;
            modelPivot.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack)
                .onComplete += Deconstruct;

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
            meshRenderer.SetColor(GameManager.MPB_PAWN, GameManager.BaseColor, Const.THIS.defaultColor);
        }
        public void MarkSteadyColor()
        {
            meshRenderer.SetColor(GameManager.MPB_PAWN, GameManager.BaseColor, Const.THIS.steadyColor);
        }
        public void MarkMergerColor()
        {
            meshRenderer.SetColor(GameManager.MPB_PAWN, GameManager.BaseColor, Const.THIS.mergerColor);
        }
        #endregion
        public void AnimatedShow(float delay, float scale, float duration, System.Action start = null)
        {
            modelPivot.DOKill();
            modelPivot.localScale = Vector3.zero;
            
            _delayedTween?.Kill();
            _delayedTween = DOVirtual.DelayedCall(delay, () =>
            {
                start?.Invoke();    

                modelPivot.DOKill();
                modelPivot.localScale = Vector3.one;
                modelPivot.DOPunchScale(Vector3.one * scale, duration, 1);
            });
        }
        public void PunchScaleBullet(float magnitude)
        {
            modelPivot.DOKill();
            modelPivot.localScale = Vector3.one;
            modelPivot.DOPunchScale(Vector3.one * magnitude, 0.3f, 1);
        }
        public void PunchScale(float magnitude, float duration)
        {
            modelPivot.DOKill();
            modelPivot.localScale = Vector3.one;
            modelPivot.DOScale(Vector3.one * magnitude, duration).SetRelative(true);
        }
        public void PunchUp(float magnitude, float duration)
        {
            pivot.DOKill();
            pivot.localScale = Vector3.one;
            pivot.DOPunchScale(Vector3.up * magnitude, duration, 1);
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
        }  
        public void Show()
        {
            modelPivot.DOKill();
            modelPivot.localScale = Vector3.zero;
            modelPivot.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack);
        }



        
        public bool MoveForward(Place checkerPlace, int tick, float moveDuration)
        {
            if (BUSY)
            {
                return true;
            }
            if (!MOVER)
            {
                return false;
            }
            
            Tick = tick;
            
            Place forwardPlace = Board.THIS.GetForwardPlace(checkerPlace);
            forwardPlace.Accept(this, moveDuration);
            
            checkerPlace.Current = null;
            return true;
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

            if (forwardPlace.Occupied && !forwardPlace.Current.MOVER) // if front place is occupied and not a mover
            {
                MOVER = false;
                return;
            }
            
            if (UsageType.Equals(Usage.HorMerge))
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
