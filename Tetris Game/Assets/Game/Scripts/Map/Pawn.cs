using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Game
{
    public class Pawn : MonoBehaviour
    {
        [SerializeField] public MeshRenderer meshRenderer;
        [SerializeField] private MeshRenderer iconMR;
        [SerializeField] private TextMeshPro levelText;
        [SerializeField] public Transform modelPivot;
        [SerializeField] public Transform pivot;

        [System.NonSerialized] private Tween _moveTween = null;
        [System.NonSerialized] private Tween _delayedTween = null;
        [System.NonSerialized] private Transform _thisTransform;
        [System.NonSerialized] public Block ParentBlock;
        [System.NonSerialized] private int _amount = 1;
        [System.NonSerialized] public int Tick;
        
        [System.NonSerialized] public bool Mover = false;
        [System.NonSerialized] public bool Busy = false;
        [System.NonSerialized] public bool CanTakeAmmo = false;
        [System.NonSerialized] public bool Free2Place = false;

        private Pawn.Usage _usageType;
        public Pawn.Usage UsageType
        {
            set
            {
                _usageType = value;
                switch (_usageType)
                {
                    case Usage.Ammo:
                        MarkAmmoColor();
                        Free2Place = false;
                        break;
                    case Usage.UnpackedAmmo:
                        MarkUnpackedAmmoColor();
                        Free2Place = false;
                        break;
                    case Usage.MagnetLR:
                        MarkPowerupColor();
                        Free2Place = true;
                        break;
                    case Usage.MagnetUD:
                        MarkPowerupColor();
                        Free2Place = true;
                        break;
                    case Usage.Magnet:
                        MarkPowerupColor();
                        Free2Place = true;
                        break;
                    // case Usage.MagnetLRUD:
                    //     MarkPowerupColor();
                    //     break;
                    // case Usage.DamageDouble:
                    //     MarkPowerupColor();
                    //     break;
                    // case Usage.FirerateDouble:
                    //     MarkPowerupColor();
                    //     break;
                    // case Usage.SplitshotDouble:
                    //     MarkPowerupColor();
                    //     break;
                }
                
            }

            get => _usageType;
        }
        
        [System.NonSerialized] private static readonly Vector3 BulletPsUp = new Vector3(0.0f, 0.9f, 0.0f);
        
        public bool Connected => ParentBlock;

        public bool TextEnabled
        {
            set => levelText.enabled = value;
        }
        // public bool CanPlaceAnywhere => UsageType.Equals(Usage.HorMerge);
        public Vector3 TextPosition => levelText.transform.position;

        public enum Usage
        {
            Ammo,
            UnpackedAmmo,
            MagnetLR,
            MagnetUD,
            Magnet,
            // DamageDouble,
            // FirerateDouble,
            // SplitshotDouble,
            // ShooterIdle,
            // Heart,
            // Shield,
            // Vertical,
            // HorMerge,
            // Area,
            // Speed,
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
                        levelText.enabled = false;
                        iconMR.enabled = true;
                        break;
                    case Usage.UnpackedAmmo:
                        levelText.text = (_amount >= Board.THIS._Data.maxStack) ? "MAX" : _amount.ToString();
                        iconMR.enabled = false;
                        break;
                    case Usage.MagnetLR:
                        levelText.enabled = false;
                        iconMR.enabled = true;
                        this._amount = 0;
                        break;
                    case Usage.MagnetUD:
                        levelText.enabled = false;
                        iconMR.enabled = true;
                        this._amount = 0;
                        break;
                    case Usage.Magnet:
                        levelText.enabled = false;
                        iconMR.enabled = true;
                        this._amount = 0;
                        break;
                    // case Usage.MagnetLRUD:
                    //     levelText.enabled = false;
                    //     iconMR.enabled = true;
                    //     break;
                    // case Usage.DamageDouble:
                    //     levelText.enabled = false;
                    //     iconMR.enabled = true;
                    //     break;
                    // case Usage.FirerateDouble:
                    //     levelText.enabled = false;
                    //     iconMR.enabled = true;
                    //     break;
                    // case Usage.SplitshotDouble:
                    //     levelText.enabled = false;
                    //     iconMR.enabled = true;
                    //     break;
                }

                Sprite pawnIcon = Const.THIS.pawnIcons[(int)UsageType];
                if (pawnIcon)
                {
                    iconMR.material.SetTexture(GameManager.BaseMap, pawnIcon.texture);
                }
            }
        }

        // public void UnpackAmmo()
        // {
        //     UsageType = Pawn.Usage.UnpackedAmmo;
        // }

        public bool Unpack(float delay)
        {
            if (ParentBlock)
            {
                ParentBlock.DetachPawn(this);
            }
            switch (UsageType)
            {
                case Usage.Ammo:
                    
                    return true;
                case Usage.UnpackedAmmo:
                
                    return true;
                case Usage.MagnetLR:
                    UIManagerExtensions.Distort(_thisTransform.position + Vector3.up * 0.45f, 0.0f);
                    return true;
                case Usage.MagnetUD:
                    UIManagerExtensions.Distort(_thisTransform.position + Vector3.up * 0.45f, 0.0f);
                    return true;
                case Usage.Magnet:
                    UIManagerExtensions.Distort(_thisTransform.position + Vector3.up * 0.45f, 0.0f);
                    return true;
                // case Usage.Heart:
                //     TextEnabled = false;
                //     
                //     // Earn heart upgrade
                //
                //     // UIManager.THIS.ft_Icon.LerpHearth(levelText.transform.position, delay, 0.65f, endAction: () =>
                //     // {
                //     //     Warzone.THIS.GiveHeart(_amount);
                //     // });
                //     return false;
                // case Usage.Shield:
                //     TextEnabled = false;
                //     
                //     // Earn shield
                //
                //     // UIManager.THIS.ft_Icon.LerpShield(levelText.transform.position, delay, 0.65f, endAction: () =>
                //     // {
                //     //     Warzone.THIS.GiveShield(1);
                //     // });
                //     return false;
                // case Usage.Vertical:
                //     
                // case Usage.HorMerge:
                //     
                //     return false;
                // case Usage.Area:
                //     
                //     return false;
                // case Usage.Speed:
                //     
                //     return false;
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
        
        public void OnLevelEnd()
        {
            KillTweens();
            
            modelPivot.DOKill();
            modelPivot.localScale = Vector3.one;
        }

        private void KillTweens()
        {
            _delayedTween?.Kill();
            _moveTween?.Kill();

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

        public void MarkAmmoColor()
        {
            meshRenderer.material.SetColor(GameManager.BaseColor, Const.THIS.defaultColor);
        }
        public void MarkSteadyColor()
        {
            if (_usageType.Equals(Usage.Ammo))
            {
                meshRenderer.material.SetColor(GameManager.BaseColor, Const.THIS.steadyColor);
            }
        }
        public void MarkUnpackedAmmoColor()
        {
            meshRenderer.material.SetColor(GameManager.BaseColor, Const.THIS.mergerColor);
        }
        public void MarkPowerupColor()
        {
            meshRenderer.material.SetColor(GameManager.BaseColor, Const.THIS.powerColor);
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
        public void PunchScaleModelPivot(float magnitude, float duration = 0.3f)
        {
            modelPivot.DOKill();
            modelPivot.localScale = Vector3.one;
            modelPivot.DOPunchScale(Vector3.one * magnitude, duration, 1);
        }
        // public void PunchScale(float magnitude, float duration)
        // {
        //     modelPivot.DOKill();
        //     modelPivot.localScale = Vector3.one;
        //     modelPivot.DOScale(Vector3.one * magnitude, duration).SetRelative(true);
        // }
        public void PunchUp(float magnitude, float duration)
        {
            pivot.DOKill();
            pivot.localPosition = Vector3.zero;
            pivot.localScale = Vector3.one;
            pivot.DOPunchScale(Vector3.up * magnitude, duration, 1);
        }
        public void JumpUp(float magnitude, float duration, float delay)
        {
            pivot.DOKill();
            pivot.localScale = Vector3.one;
            pivot.localPosition = Vector3.zero;
            pivot.DOPunchPosition(Vector3.back * magnitude, duration, 1).SetDelay(delay);
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
            if (Busy)
            {
                return true;
            }
            if (!Mover)
            {
                return false;
            }
            
            Tick = tick;
            
            Place forwardPlace = Board.THIS.GetForwardPlace(checkerPlace);
            // if (!forwardPlace)
            // {
            //     Debug.Log(checkerPlace.gameObject.name, checkerPlace.gameObject);
            // }
            forwardPlace.Accept(this, moveDuration);
            
            checkerPlace.Current = null;
            return true;
        }

        public void Check(Place checkerPlace)
        {
            if (Busy)
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
                Mover = false;
                return;
            }

            if (forwardPlace.Occupied && !forwardPlace.Current.Mover) // if front place is occupied and not a mover
            {
                Mover = false;
                return;
            }
            
            if (UsageType.Equals(Usage.MagnetLR) || UsageType.Equals(Usage.MagnetUD) ||  UsageType.Equals(Usage.Magnet))
            {
                Mover = false;
                return;
            }
        }

        private void CheckDetach()
        {
            if (Mover)
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
