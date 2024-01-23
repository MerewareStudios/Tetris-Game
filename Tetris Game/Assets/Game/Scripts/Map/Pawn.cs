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
        [SerializeField] public Transform thisTransform;
        
        [System.NonSerialized] private static readonly Vector3 BulletPsUp = new Vector3(0.0f, 0.9f, 0.0f);
        
        [System.NonSerialized] private Tween _moveTween = null;
        [System.NonSerialized] public SubModel SubModel = null;
        [System.NonSerialized] private Block _parentBlock;
        [System.NonSerialized] public VisualData VData = null;
        [SerializeField] private int _tick;
        // [System.NonSerialized] public bool Mover = false;
        [System.NonSerialized] public bool Busy = false;
        // [System.NonSerialized] public BlockData RecentBlockData;
        
        
        public bool Connected => ParentBlock;
        [System.NonSerialized] public Pawn.Usage UsageType = Usage.Empty;

        public Block ParentBlock
        {
            get => _parentBlock;
            set
            {
                _parentBlock = value;
                // RecentBlockData = _parentBlock.blockData;
                if (!value)
                {
                    return;
                }

                if (SubModel && VData.useParentColor)
                {
                    SubModel.BaseColor = ParentBlock.blockData.Color;
                }
            }
        }

        public void EmitExplodeEffect()
        {
            if (!SubModel)
            {
                return;
            }
            SubModel.EmitExplodeEffect();
        }
        
        public bool SkipMerge
        {
            get
            {
                if (VData.usage.Equals(Usage.Rocket) || VData.usage.Equals(Usage.Bomb) || VData.usage.Equals(Usage.Punch))
                {
                    if (!Warzone.THIS.EnemyExits())
                    {
                        return true;
                    }
                }
                return false;
            }    
        }
        
        public int Tick
        {
            set
            {
                this._tick = value;
            }
            get => this._tick;
        }
        
        public bool Available
        {
            get => this.SubModel && SubModel.IsAvailable();
            set
            {
                if (this.SubModel)
                {
                    this.SubModel.MarkAvailable(value);
                }
            }
        }

        public void SetUsageType(Usage value, int extra)
        {
            VData = Const.THIS.pawnVisualData[(int)value];
            
            if (SubModel)
            {
                DeSpawnModel();
            }
            SubModel = VData.model.Spawn<SubModel>();
            SubModel.gameObject.SetActive(true);

            this.UsageType = value;
            
            SubModel.ResetEmission();

            // SubModel.BaseColor = (VData.useParentColor && !ParentBlock) ? ParentBlock.blockData.Color : VData.startColor;
            SubModel.OnConstruct(VData.model, modelPivot, extra);
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
            set => SubModel.OnExtraValueChanged(value);
            get => this.SubModel ? this.SubModel.GetExtra() : 0;
        }

        public void MakeAvailable()
        {
            if (SubModel)
            {
                SubModel.MakeAvailable();
            }
        }
        public void UseSubModel()
        {
            if (SubModel)
            {
                SubModel.OnUse();
            }
        }
        public void OnMerge()
        {
            if (SubModel)
            {
                SubModel.OnMerge();
            }
        }
        public void OnPlace(Place place)
        {
            if (SubModel)
            {
                SubModel.OnPlace(place);
            }
        }


        public bool Unpack(Place place)
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
                    SubModel.OnUnpack();
                    SubModel = null;
                    break;
                case Usage.Magnet:
                    SubModel.Lose();
                    SubModel.OnUnpack();
                    SubModel = null;
                    break;
                case Usage.Nugget:
                    SubModel.Lose();
                    SubModel.OnUnpack();
                    SubModel = null;
                    break;
                case Usage.Medic:
                    SubModel.Lose();
                    SubModel.OnUnpack();
                    SubModel = null;
                    break;
                case Usage.Rocket:
                    Enemy rocketEnemy = Warzone.THIS.GetRandomTarget();
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
                    SubModel.Lose();
                    SubModel.OnProjectile(bombEnemy);
                    SubModel = null;
                    break;
                case Usage.Screw:
                    if (SubModel.OnCustomUnpack())
                    {
                        SubModel.Lose();
                        SubModel.OnUnpack();
                        SubModel = null;
                        return true;
                    }
                    return false;
                case Usage.Gift:
                    SubModel.Lose();
                    SubModel.OnAnimate((position) =>
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            Pawn.Usage usg = Const.THIS.gifts.Random();
                            Board.THIS.SpawnPawnAndJumpRandom(place, position, usg, usg.ExtraValue());
                        }
                    });
                    SubModel = null;
                    return true;
                case Usage.Punch:
                    Enemy punchEnemy = Warzone.THIS.GetProjectileTarget(SubModel.Position);
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
            SubModel.OnExplode(center);

            // switch (UsageType)
            // {
            //     case Usage.Empty:
            //         break;
            //     case Usage.Ammo:
            //         break;
            //     case Usage.UnpackedAmmo:
            //         break;
            //     case Usage.Energy:
            //         break;
            //     case Usage.Magnet:
            //         break;
            //     case Usage.Nugget:
            //         break;
            //     case Usage.Medic:
            //         break;
            //     case Usage.Rocket:
            //         break;
            //     case Usage.Landmine:
            //         break;
            //     case Usage.Bomb:
            //         // SubModel = null;
            //         break;
            //     case Usage.Screw:
            //         
            //         break;
            //     case Usage.Gift:
            //         
            //         break;
            //     case Usage.Punch:
            //         
            //         break;
            //     case Usage.Lock:
            //         
            //         break;
            // }
        }
        
        public void RewardForSubModel()
        {
            if (!SubModel)
            {
                return;
            }

            int rewardAmount = 0;
            switch (UsageType)
            {
                case Usage.Empty:
                    break;
                case Usage.Ammo:
                    break;
                case Usage.UnpackedAmmo:
                    break;
                case Usage.Energy:
                    rewardAmount = 5;
                    break;
                case Usage.Magnet:
                    rewardAmount = 5;
                    break;
                case Usage.Nugget:
                    rewardAmount = 5;
                    break;
                case Usage.Medic:
                    rewardAmount = 5;
                    break;
                case Usage.Rocket:
                    rewardAmount = 5;
                    break;
                case Usage.Landmine:
                    rewardAmount = 5;
                    break;
                case Usage.Bomb:
                    rewardAmount = 5;
                    break;
                case Usage.Screw:
                    
                    break;
                case Usage.Gift:
                    rewardAmount = 5;
                    break;
                case Usage.Punch:
                    rewardAmount = 5;
                    break;
                case Usage.Lock:
                    
                    break;
            }

            if (rewardAmount > 0)
            {
                SubModel.gameObject.SetActive(false);
                UIManagerExtensions.BoardCoinToPlayer(SubModel.Position,  Math.Min(rewardAmount, 5), rewardAmount);
            }
        }
        
        public void DetachSubModelAndDeconstruct()
        {
            SubModel.Lose();
            SubModel = null;
            Deconstruct();
        }
        
        
        public void Deconstruct()
        {
            KillTweens();

            transform.DOKill();
            
            modelPivot.DOKill();
            modelPivot.localScale = Vector3.one;

            ParentBlock = null;
            
            DeSpawnModel();
            
            Busy = false;
            // Mover = false;

            this.Despawn(Pool.Pawn);
        }
        
        public void OnLevelEnd()
        {
            KillTweens();
            
            modelPivot.DOKill();
            modelPivot.localScale = Vector3.one;
        }

        private void KillTweens()
        {
            _moveTween?.Kill();
        }
        public void Move(Vector3 position, float duration, Ease ease, System.Action complete = null)
        {
            _moveTween?.Kill();
            _moveTween = thisTransform.DOMove(position, duration).SetEase(ease);
            _moveTween.onComplete = () =>
                {
                    complete?.Invoke();
                };
        }
        public void Jump(Vector3 position, float power, float duration, Ease ease, System.Action complete = null)
        {
            _moveTween?.Kill();
            _moveTween = thisTransform.DOJump(position, power, 1, duration).SetEase(ease);
            _moveTween.onComplete = () =>
            {
                complete?.Invoke();
            };
        }

        #region Colors
        // public void MarkSteadyColor()
        // {
        //     if (this.UsageType.Equals(Usage.UnpackedAmmo))
        //     {
        //         SubModel.BaseColor = Const.THIS.steadyColor;
        //     }
        // }
        #endregion
        
        
        public void PunchScaleModelPivot(float magnitude, float duration = 0.3f)
        {
            modelPivot.DOKill();
            modelPivot.localScale = Vector3.one;
            modelPivot.DOPunchScale(Vector3.one * magnitude, duration, 1);
        }
        public void Show()
        {
            modelPivot.DOKill();
            modelPivot.localScale = Vector3.zero;
            modelPivot.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack);
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
            pivot.localPosition = Vector3.zero;
            pivot.localScale = Vector3.one;
            pivot.DOPunchPosition(Vector3.back * magnitude, duration, 1).SetDelay(delay);
        }
        


        // public bool MoveForward(Place checkerPlace, int tick, float moveDuration)
        // {
        //     if (Busy)
        //     {
        //         return false;
        //     }
        //     // if (!Mover)
        //     // {
        //     //     return false;
        //     // }
        //
        //     Tick = tick;
        //     
        //     checkerPlace.Current = null;
        //     
        //     Place forwardPlace = Board.THIS.GetForwardPlace(checkerPlace);
        //     if (!forwardPlace)
        //     {
        //         return false;
        //     }
        //     
        //     forwardPlace.Accept(this, moveDuration);
        //
        //     return true;
        // }

        // public void Check(Place checkerPlace)
        // {
        //     if (Busy)
        //     {
        //         return;
        //     }
        //     // CheckSteady(checkerPlace);
        //     CheckDetach();
        // }
        // private void CheckSteady(Place checkerPlace)
        // {
        //     Place forwardPlace = Board.THIS.GetForwardPlace(checkerPlace);
        //
        //     if (!forwardPlace)
        //     {
        //         Mover = false;
        //         return;
        //     }
        //
        //     if (forwardPlace.Occupied && !forwardPlace.Current.Mover)
        //     {
        //         Mover = false;
        //         return;
        //     }
        //     
        //     if (VData.neverMoves)
        //     {
        //         Mover = false;
        //     }
        // }

        // private void CheckDetach()
        // {
        //     // if (Mover)
        //     // {
        //     //     return;
        //     // }
        //     if (Connected)
        //     {
        //         ParentBlock.Detach();
        //     }
        // }
        
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
            [SerializeField] public bool useParentColor = false;
            // [SerializeField] public Color startColor;
            // [SerializeField] public Gradient explodeGradient;
            [SerializeField] public Sprite powerUpIcon;
            [SerializeField] public bool neverMoves = false;
            [SerializeField] public bool moverOnPlacement = true;
        }
    }
}
