using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Game
{
    public class Pawn : MonoBehaviour
    {
        [SerializeField] private MeshRenderer iconMr;
        [SerializeField] private TextMeshPro levelText;
        [SerializeField] public Transform modelPivot;
        [SerializeField] public Transform pivot;
        
        [System.NonSerialized] private GameObject _currentModel = null;
        [System.NonSerialized] private MeshRenderer _meshRenderer;

        [System.NonSerialized] private Tween _moveTween = null;
        [System.NonSerialized] private Tween _delayedTween = null;
        [System.NonSerialized] private Transform _thisTransform;
        [System.NonSerialized] public Block ParentBlock;
        [System.NonSerialized] private int _amount = 1;
        [System.NonSerialized] public int Tick;
        
        [System.NonSerialized] public bool Mover = false;
        [System.NonSerialized] public bool Busy = false;
        [System.NonSerialized] public bool CanTakeContent = false;
        [System.NonSerialized] private static readonly Vector3 BulletPsUp = new Vector3(0.0f, 0.9f, 0.0f);
        
        public bool Free2Place => Const.THIS.pawnVisualData[(int)UsageType].free2Place;
        public bool Connected => ParentBlock;
        private Pawn.Usage _usageType = Usage.Empty;
        
        public Pawn.Usage UsageType
        {
            set
            {
                VisualData visualData = Const.THIS.pawnVisualData[(int)value];
                
                Debug.Log(this._usageType.Model() + " " + visualData.model);

                if (!_currentModel || !this._usageType.Model().Equals(visualData.model))
                {
                    DeSpawnModel();
                    _currentModel = visualData.model.Spawn();
                }
                
                
                this._usageType = value;
                
                
                
                Transform currenModelTransform = _currentModel.transform;
                
                currenModelTransform.parent = modelPivot;
                currenModelTransform.localPosition = visualData.defaultPosition;
                currenModelTransform.localEulerAngles = visualData.defaultRotation;
                currenModelTransform.localScale = visualData.defaultScale;

                _meshRenderer = _currentModel.GetComponent<MeshRenderer>();
                if (_meshRenderer)
                {
                    _meshRenderer.material.SetColor(GameManager.BaseColor, visualData.startColor);
                }

                CanTakeContent = false;
            }

            get => _usageType;
        }

        private void DeSpawnModel()
        {
            if (_currentModel)
            {
                _currentModel.Despawn();
                _currentModel = null;
                _meshRenderer = null;
            }
        }
        
        public int Amount
        {
            set
            {
                this._amount = value;
                
                VisualData visualData = Const.THIS.pawnVisualData[(int)UsageType];
                
                levelText.enabled = visualData.amountTextEnabled;
                if (levelText.enabled)
                {
                    bool max = value == Board.THIS.StackLimit;
                    _meshRenderer.material.SetColor(GameManager.BaseColor, max ? Const.THIS.mergerMaxColor : Const.THIS.mergerColor);
                    levelText.text = _amount.ToString();
                }
                
                iconMr.enabled = visualData.icon;
                if (iconMr.enabled)
                {
                    iconMr.material.SetTexture(GameManager.BaseMap, visualData.icon.texture);
                }
            }
            get => this._amount;
        }
        
        void Awake()
        {
            _thisTransform = transform;
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
            DeSpawnModel();
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

        // public void MarkAmmoColor()
        // {
        //     _meshRenderer.material.SetColor(GameManager.BaseColor, Const.THIS.mergerColor);
        // }
        public void MarkSteadyColor()
        {
            if (_usageType.Equals(Usage.UnpackedAmmo)) // not powerup
            {
                _meshRenderer.material.SetColor(GameManager.BaseColor, Const.THIS.steadyColor);
            }
        }
        // public void MarkUnpackedAmmoColor()
        // {
        //     _meshRenderer.material.SetColor(GameManager.BaseColor, Const.THIS.defaultColor);
        // }
        // public void MarkPowerupColor()
        // {
        //     _meshRenderer.material.SetColor(GameManager.BaseColor, Const.THIS.powerColor);
        // }
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
            Empty,
            Ammo,
            UnpackedAmmo,
            MagnetLR,
            Magnet,
            Nugget,
            Medic,
        }

        [Serializable]
        public class VisualData
        {
            [SerializeField] public Pawn.Usage usage;
            [SerializeField] public Pool model;
            [SerializeField] public Vector3 defaultPosition;
            [SerializeField] public Vector3 defaultRotation;
            [SerializeField] public Vector3 defaultScale;
            [SerializeField] public bool free2Place = false;
            [SerializeField] public Color startColor;
            [SerializeField] public bool amountTextEnabled = false;
            [SerializeField] public Sprite icon;
        }
    }
}
