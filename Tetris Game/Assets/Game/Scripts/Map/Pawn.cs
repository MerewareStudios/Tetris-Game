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
        [System.NonSerialized] public bool CanTakeContent = false;
        
        public bool Free2Place => _usageType.Equals(Usage.MagnetLR) || _usageType.Equals(Usage.Magnet);

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
                        // Free2Place = false;
                        break;
                    case Usage.UnpackedAmmo:
                        MarkUnpackedAmmoColor();
                        // Free2Place = false;
                        break;
                    case Usage.MagnetLR:
                        MarkPowerupColor();
                        // Free2Place = true;
                        break;
                    case Usage.Magnet:
                        MarkPowerupColor();
                        // Free2Place = true;
                        break;
                }
                CanTakeContent = false;
                
            }

            get => _usageType;
        }
        
        [System.NonSerialized] private static readonly Vector3 BulletPsUp = new Vector3(0.0f, 0.9f, 0.0f);
        
        public bool Connected => ParentBlock;

        public bool TextEnabled
        {
            set => levelText.enabled = value;
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
                        bool max = value == Board.THIS.StackLimit;
                        meshRenderer.material.SetColor(GameManager.BaseColor, max ? Const.THIS.mergerMaxColor : Const.THIS.mergerColor);
                        levelText.text = _amount.ToString();
                        iconMR.enabled = false;
                        break;
                    case Usage.UnpackedAmmo:
                        levelText.enabled = false;
                        iconMR.enabled = true;
                        break;
                    case Usage.MagnetLR:
                        levelText.enabled = false;
                        iconMR.enabled = true;
                        _amount = 0;
                        break;
                    case Usage.Magnet:
                        levelText.enabled = false;
                        iconMR.enabled = true;
                        _amount = 0;
                        break;
                }

                Sprite pawnIcon = Const.THIS.pawnIcons[(int)UsageType];
                if (pawnIcon)
                {
                    iconMR.material.SetTexture(GameManager.BaseMap, pawnIcon.texture);
                }
            }

        }

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
                case Usage.Magnet:
                    UIManagerExtensions.Distort(_thisTransform.position + Vector3.up * 0.45f, 0.0f);
                    return true;
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
            meshRenderer.material.SetColor(GameManager.BaseColor, Const.THIS.mergerColor);
        }
        public void MarkSteadyColor()
        {
            if (_usageType.Equals(Usage.UnpackedAmmo)) // not powerup
            {
                meshRenderer.material.SetColor(GameManager.BaseColor, Const.THIS.steadyColor);
            }
        }
        public void MarkUnpackedAmmoColor()
        {
            meshRenderer.material.SetColor(GameManager.BaseColor, Const.THIS.defaultColor);
        }
        public void MarkPowerupColor()
        {
            meshRenderer.material.SetColor(GameManager.BaseColor, Const.THIS.powerColor);
        }
        #endregion
        public void UnpackAmmo(float delay, float scale, float duration, System.Action start = null)
        {
            modelPivot.DOKill();
            modelPivot.localScale = Vector3.zero;
            
            _delayedTween?.Kill();
            _delayedTween = DOVirtual.DelayedCall(delay, () =>
            {
                start?.Invoke();    

                modelPivot.DOKill();
                modelPivot.localScale = Vector3.one;
                modelPivot.DOPunchScale(Vector3.one * scale, duration, 1).onComplete = () =>
                {
                    CanTakeContent = true;
                };
            }, false);
        }
        public void PunchScaleModelPivot(float magnitude, float duration = 0.3f)
        {
            modelPivot.DOKill();
            modelPivot.localScale = Vector3.one;
            modelPivot.DOPunchScale(Vector3.one * magnitude, duration, 1);
        }
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
            
            if (UsageType.Equals(Usage.MagnetLR) ||  UsageType.Equals(Usage.Magnet))
            {
                Mover = false;
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
        
        public enum Usage
        {
            Ammo,
            UnpackedAmmo,
            MagnetLR,
            Magnet,
            Nugget,
        }

        [Serializable]
        public class VisualData
        {
            [SerializeField] public Pawn.Usage usage;
            [SerializeField] public Pool model;
            [SerializeField] public Vector3 defaultPosition;
            [SerializeField] public Vector3 defaultScale;
        }
    }
}
