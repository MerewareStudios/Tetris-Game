using System;
using DG.Tweening;
using Internal.Core;
using UnityEngine;

namespace Game
{
    public class Pawn : MonoBehaviour
    {
        [SerializeField] public Transform modelPivot;
        [SerializeField] public Transform pivot;
        
        [System.NonSerialized] private static readonly Vector3 BulletPsUp = new Vector3(0.0f, 0.9f, 0.0f);
        
        [System.NonSerialized] private Tween _moveTween = null;
        [System.NonSerialized] private Tween _delayedTween = null;
        [System.NonSerialized] private Transform _thisTransform;
        [System.NonSerialized] private int _amount = 1;
        
        [System.NonSerialized] public SubModel SubModel = null;
        [System.NonSerialized] public Block ParentBlock;
        [System.NonSerialized] public VisualData VData = null;
        [System.NonSerialized] public int Tick;
        [System.NonSerialized] public bool Mover = false;
        [System.NonSerialized] public bool Busy = false;
        [System.NonSerialized] public bool CanTakeContent = false;
        
        
        public bool Connected => ParentBlock;
        private Pawn.Usage _usageType = Usage.Empty;
        
        public Pawn.Usage UsageType
        {
            set
            {
                VData = Const.THIS.pawnVisualData[(int)value];
                
                if (!SubModel || !this._usageType.Model().Equals(VData.model))
                {
                    DeSpawnModel();
                    SubModel = VData.model.Spawn<SubModel>();
                }
                
                
                this._usageType = value;
                
                SubModel.OnConstruct(modelPivot);
                
                SubModel.BaseColor = VData.startColor;

                CanTakeContent = false;
            }

            get => _usageType;
        }

        private void DeSpawnModel()
        {
            if (SubModel)
            {
                SubModel.OnDeconstruct();
                SubModel = null;
            }
        }
        
        public int Amount
        {
            set
            {
                this._amount = value;
                SubModel.OnExternalValueChanged(_amount);
            }
            get => this._amount;
        }
        
        void Awake()
        {
            _thisTransform = transform;
        }

        public bool Unpack()
        {
            if (ParentBlock)
            {
                ParentBlock.DetachPawn(this);
            }

            if (!SubModel)
            {
                return true;
            }
            
            switch (UsageType)
            {
                case Usage.Empty:
                    break;
                case Usage.Ammo:
                    break;
                case Usage.UnpackedAmmo:
                    break;
                case Usage.Energy:
                    SubModel.Lose();
                    SubModel.OnUse();
                    SubModel = null;
                    break;
                case Usage.Magnet:
                    SubModel.Lose();
                    SubModel.OnUse();
                    SubModel = null;
                    break;
                case Usage.Nugget:
                    SubModel.Lose();
                    SubModel.OnUse();
                    SubModel = null;
                    break;
                case Usage.Medic:
                    SubModel.Lose();
                    SubModel.OnUse();
                    SubModel = null;
                    break;
                case Usage.Rocket:
                    Enemy rocketEnemy = Warzone.THIS.GetRandomTarget();
                    if (!rocketEnemy)
                    {
                        return false;
                    }
                    SubModel.Lose();
                    SubModel.OnProjectile(rocketEnemy);
                    SubModel = null;
                    break;
                case Usage.Landmine:
                    SubModel.UnParent();
                    SubModel.OnDeploy(Warzone.THIS.GetLandMineTarget(), Warzone.THIS.AddLandMine);
                    SubModel = null;
                    break;
                case Usage.Bomb:
                    Enemy bombEnemy = Warzone.THIS.GetAoeTarget();
                    if (!bombEnemy)
                    {
                        return false;
                    }
                    SubModel.Lose();
                    SubModel.OnProjectile(bombEnemy);
                    SubModel = null;
                    break;
                case Usage.Screw:
                    if (SubModel.OnCustomUnpack())
                    {
                        SubModel.Lose();
                        SubModel.OnUse();
                        SubModel = null;
                        return true;
                    }
                    return false;
                case Usage.Gift:
                    SubModel.Lose();
                    SubModel.OnAnimate(() =>
                    {
                        UsageType = Const.THIS.gifts.Random();
                    });
                    SubModel = null;
                    return false;
                case Usage.Punch:
                    Enemy punchEnemy = Warzone.THIS.GetProjectileTarget(SubModel.Position);
                    if (!punchEnemy)
                    {
                        return false;
                    }
                    SubModel.Lose();
                    SubModel.OnProjectile(punchEnemy);
                    SubModel = null;
                    break;
                case Usage.Lock:
                    return false;
            }
            
            return true;
        }
        
        public void Explode(Vector2Int center)
        {
            if (!SubModel)
            {
                return;
            }
            switch (UsageType)
            {
                case Usage.Empty:
                    break;
                case Usage.Ammo:
                    break;
                case Usage.UnpackedAmmo:
                    break;
                case Usage.Energy:
                    break;
                case Usage.Magnet:
                    break;
                case Usage.Nugget:
                    break;
                case Usage.Medic:
                    break;
                case Usage.Rocket:
                    break;
                case Usage.Landmine:
                    break;
                case Usage.Bomb:
                    SubModel.OnExplode();
                    SubModel = null;
                    Board.THIS.ExplodePawnsCircular(center, Board.BombRadius);
                    break;
                case Usage.Screw:
                    
                    break;
                case Usage.Gift:
                    
                    break;
                case Usage.Punch:
                    
                    break;
                case Usage.Lock:
                    
                    break;
            }
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
        public void MarkSteadyColor()
        {
            if (_usageType.Equals(Usage.UnpackedAmmo)) // not powerup
            {
                SubModel.BaseColor = Const.THIS.steadyColor;
            }
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
            
            if (VData.neverMoves)
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
            Energy,
            Magnet,
            Nugget,
            Medic,
            Rocket,
            Landmine,
            Bomb,
            Screw,
            Gift,
            Punch,
            Lock,
        }

        [Serializable]
        public class VisualData
        {
            [SerializeField] public Pawn.Usage usage;
            [SerializeField] public int externValue = 0;
            [SerializeField] public Pool model;
            [SerializeField] public bool free2Place = false;
            [SerializeField] public Color startColor;
            [SerializeField] public Sprite powerUpIcon;
            [SerializeField] public bool neverMoves = false;
            [SerializeField] public bool moverOnPlacement = true;
        }
    }
}
