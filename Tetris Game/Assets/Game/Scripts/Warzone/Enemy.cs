using System.Collections.Generic;
using DG.Tweening;
using Internal.Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace  Game
{
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private Transform model;
        [SerializeField] private HealthCanvas healthCanvas;
        [SerializeField] public Transform thisTransform;
        [SerializeField] public Transform hitTarget;
        [SerializeField] private Renderer skin;
        [SerializeField] private Animator animator;
        [SerializeField] public EnemyData so;
        [SerializeField] public ParticleSystem castPs;
        [SerializeField] public Transform castParent;
        [System.NonSerialized] private int _health;
        [System.NonSerialized] private Tween _colorPunchTween;
        [System.NonSerialized] public int ID;
        [System.NonSerialized] private GameObject _dragTrail = null;
        [System.NonSerialized] public bool DragTarget = false;
        [System.NonSerialized] private Tween _castTweenLoop;
        [System.NonSerialized] private Tween _wipeTween;
        [System.NonSerialized] public int CoinAmount;

        private static int WALK_HASH = Animator.StringToHash("Walk");
        private static int DEATH_HASH = Animator.StringToHash("Death");
        private static int HIT_CROSS = Animator.StringToHash("Base Layer.Hit");
        private static int CAST_HASH = Animator.StringToHash("Cast");
        private static int CASTING_BOOL_HASH = Animator.StringToHash("Casting");
        // private static int CAN_WALK_HASH = Animator.StringToHash("CanWalk");

        public int Damage => Health;
        public Vector3 Position => thisTransform.position;
        public float PositionZ => thisTransform.position.z;
        public Vector2 PositionXZ => new Vector2(thisTransform.position.x, thisTransform.position.z);

        public int Health
        {
            set
            {
                this._health = value;
                healthCanvas.Health = value;
            }
            get => this._health;
        }

        public Vector3 CrossSize => new Vector3(so.crossSize, so.crossSize, so.crossSize);

        void Awake()
        {
            healthCanvas.canvas.worldCamera = CameraManager.THIS.gameCamera;
        }

        #region  Mono
        public void Walk()
        {
            thisTransform.Translate(new Vector3(0.0f, 0.0f, Time.deltaTime * so.speed * LevelManager.DeltaMult));
            
            Warzone.THIS.CheckLandmine(this);
            
            if (Warzone.THIS.IsOutside(thisTransform))
            {
                Warzone.THIS.EnemyKilled(this, true);
            }
        }

        public void Cast()
        {
            _castTweenLoop?.Kill();

            switch (so.castType)
            {
                case CastTypes.None:
                    break;
                case CastTypes.SpawnBomb:
                    if (castPs)
                    {
                        castPs.Play();
                    }
                    animator.SetBool(CASTING_BOOL_HASH, true);
                    animator.SetTrigger(CAST_HASH);
                    break;
                case CastTypes.DestoryPawn:
                    if (castPs)
                    {
                        castPs.Stop();
                        castPs.transform.DOKill();
                    }
                    animator.SetBool(CASTING_BOOL_HASH, true);
                    animator.SetTrigger(CAST_HASH);
                    break;
                case CastTypes.SpawnEnemy:
                    if (castPs)
                    {
                        castPs.Play();
                    }
                    animator.SetBool(CASTING_BOOL_HASH, true);
                    animator.SetTrigger(CAST_HASH);
                    break;
            }

            if (so.spawnerDuration > 0.1f)
            {
                _castTweenLoop?.Kill();
                _castTweenLoop = DOVirtual.DelayedCall(so.spawnerDuration, Cast, false).SetLoops(-1);
            }
        }
        public void OnCast()
        {
            if (!GameManager.PLAYING)
            {
                return;
            }
            animator.SetBool(CASTING_BOOL_HASH, false);
            switch (so.castType)
            {
                case CastTypes.None:
                    break;
                case CastTypes.SpawnBomb:
                    for (int i = 0; i < so.spawnerCount; i++)
                    {
                        Board.THIS.SpawnTrapBomb(so.spawnerExtra);
                    }
                    break;
                case CastTypes.DestoryPawn:
                    for (int i = 0; i < so.spawnerCount; i++)
                    {
                        castPs.Play();
                        Board.THIS.DestroyWithProjectile(castPs, castParent.position);
                    }
                    break;
                case CastTypes.SpawnEnemy:
                    for (int i = 0; i < so.spawnerCount; i++)
                    {
                        Vector3 pos = Warzone.THIS.RandomPos(so.extraData.RandomForwardRange());
                        Particle.Lightning.Play(pos - CameraManager.THIS.gameCamera.transform.forward);
                        Enemy enemy = Warzone.THIS.CustomSpawnEnemy(so.extraData, pos, 1);
                    }
                    Warzone.THIS.AssignClosestEnemy();
                    break;
            }
        }
    #endregion

    #region  Warzone

        public void Replenish()
        {
            Health = so.maxHealth;
            animator.SetTrigger(WALK_HASH);
            skin.material.SetColor(GameManager.EmissionKey, Color.black);
        }
        public void TakeDamage(int value, float scale = 1.0f)
        {
            if (value <= 0)
            {
                return;
            }
            ColorPunch();
            Warzone.THIS.Emit(so.emitCount, hitTarget.position, so.colorGrad, so.radius);
            healthCanvas.DisplayDamage(-value, scale);
            Warzone.THIS.Player.PunchCrossHair();
            
            if (Health <= 0)
            {
                return;
            }
            
            Health -= value;
            if (Health <= 0)
            {
                OnDeathAction();
                Warzone.THIS.EnemyKilled(this);
            }
            else
            {
                PlayHitAnimation();
            }
        }

        private void PlayHitAnimation()
        {
            if (!so.castType.Equals(CastTypes.None) && animator.GetBool(CASTING_BOOL_HASH))
            {
                return;
            }
            animator.CrossFade(HIT_CROSS, 0.1f, -1, 0.0f);
        }

        private void OnDeathAction()
        {
            switch (so.deathAction)
            {
                case DeathAction.None:
                    break;
                case DeathAction.Swarm:
                    for (int i = 0; i < so.spawnerCount; i++)
                    {
                        Enemy enemy = Warzone.THIS.CustomSpawnEnemy(so.extraData, transform.position, 1);
                        Vector3 target = thisTransform.position + new Vector3(Random.Range(-2.0f, 2.0f), 0.0f, Random.Range(-0.5f, 2.0f));
                        target.z = Mathf.Min(Warzone.THIS.StartLine, target.z);
                        enemy.Jump(target);
                    }
                    break;
            }
        }

        public void Drag(float distance, System.Action onComplete)
        {
            if (!_dragTrail)
            {
                _dragTrail = Pool.Drag_Trail.Spawn();
                _dragTrail.transform.SetParent(thisTransform);
                _dragTrail.transform.localPosition = new Vector3(0.0f, 0.1f, 0.166f);
                _dragTrail.transform.localEulerAngles = new Vector3(90.0f, 0.0f, 0.0f);
            }
            
            float finalDrag = Mathf.Min(Warzone.THIS.StartLine - thisTransform.position.z, distance);
            
            thisTransform.DOKill();
            // thisTransform.localScale = Vector3.one;
            thisTransform.DOMoveZ(finalDrag, 0.5f).SetRelative(true).SetEase(Ease.OutSine).onComplete = () =>
            {
                _dragTrail.Despawn(Pool.Drag_Trail);
                _dragTrail = null;
                DragTarget = false;
                onComplete?.Invoke();
            };
        }

        public void Jump(Vector3 target)
        {
            thisTransform.DOKill();
            thisTransform.DOJump(target, 1.0f, 1, 0.5f);
        }

        private void ColorPunch()
        {
            _colorPunchTween?.Kill();
            float timeStep = 0.0f;
            _colorPunchTween = DOTween.To((x) => timeStep = x, 0.0f, 1.0f, 0.35f).SetEase(Ease.Linear);
            _colorPunchTween.onUpdate = () =>
            {
                skin.material.SetColor(GameManager.EmissionKey, so.hitGradient.Evaluate(timeStep));
            };
        }
        
        public void OnSpawn(Vector3 position, int id)
        {
            this.ID = id;
            
            thisTransform.DOKill();
            model.DOKill();
            
            thisTransform.position = position;
            thisTransform.forward = Vector3.back;

            model.localScale = Vector3.zero;
            model.DOScale(so.scale, 0.2f).SetEase(Ease.Linear);

            this.enabled = true;

            DragTarget = false;

            Cast();
        }
        
        public void Kamikaze()
        {
            _castTweenLoop?.Kill();
            
            model.DOKill();
            Warzone.THIS.RemoveEnemy(this);
            Particle.Kamikaze.Play(thisTransform.position);
            this.Deconstruct();
        }

        public void Kill()
        {
            _castTweenLoop?.Kill();
            
            model.DOKill();
            Warzone.THIS.RemoveEnemy(this);
            
            animator.SetTrigger(DEATH_HASH);

            _wipeTween?.Kill();

            // float t = Time.realtimeSinceStartup;
            // Debug.Log("checking end level after delay : " + so.wipeDelay + " " + t);
            _wipeTween = DOVirtual.DelayedCall(so.wipeDelay, () =>
            {
                GiveRewards();
                Warzone.THIS.Emit(so.deathEmitCount, thisTransform.position, so.colorGrad, so.radius);
                // Debug.Log("checked" + " " + (Time.realtimeSinceStartup - t));
                this.Deconstruct();
                LevelManager.THIS.CheckEndLevel();
            }, false);
        }

        private void GiveRewards()
        {
            UIManagerExtensions.EmitEnemyCoinBurst(hitTarget.position, Mathf.Clamp(CoinAmount, 0, 15), CoinAmount);

            foreach (var reward in so.enemyRewards)
            {
                if(Helper.IsPossible(reward.probability))
                {
                    switch (reward.type)
                    {
                        case EnemyReward.Coin:
                            UIManagerExtensions.EmitEnemyCoinBurst(hitTarget.position, Mathf.Clamp(reward.amount, 0, 15), reward.amount);
                            break;
                        case EnemyReward.Health:
                            UIManagerExtensions.BoardHeartToPlayer(hitTarget.position,  Mathf.Clamp(reward.amount, 0, 15), reward.amount);
                            break;
                        case EnemyReward.Gem:
                            UIManagerExtensions.EmitEnemyGemBurst(hitTarget.position,  Mathf.Clamp(reward.amount, 0, 15), reward.amount);
                            break;
                        case EnemyReward.Ticket:
                            UIManagerExtensions.EmitEnemyTicketBurst(hitTarget.position,  Mathf.Clamp(reward.amount, 0, 15), reward.amount);
                            break;
                    }
                }
            }
        }
    #endregion

        // public void OnVictory()
        // {
        //     
        // }
        //
        // public void OnFail()
        // {
        //     _castTweenLoop?.Kill();
        //
        // }

        public void Deconstruct()
        {
            _castTweenLoop?.Kill();
            _wipeTween?.Kill();
            thisTransform.DOKill();
            model.DOKill();

            if (_dragTrail)
            {
                _dragTrail.Despawn(Pool.Drag_Trail);
                _dragTrail = null;
            }
            this.Despawn(so.type);
        }

        public enum CastTypes
        {
            None,
            SpawnBomb,
            DestoryPawn,
            SpawnEnemy,
        }
        public enum DeathAction
        {
            None,
            Swarm,
        }
        public enum EnemyReward
        {
            Coin,
            Health,
            Gem,
            Ticket,
        }
        [System.Serializable]
        public class BossData
        {
            [SerializeField] public Pool enemyType;
            [SerializeField] public int count;
        } 
    }

}