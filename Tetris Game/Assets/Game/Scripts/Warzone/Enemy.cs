using System.Collections.Generic;
using DG.Tweening;
using FSG.MeshAnimator.ShaderAnimated;
using Game.UI;
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
        [SerializeField] private ShaderMeshAnimator animator;
        [SerializeField] public EnemyData so;
        [SerializeField] public ParticleSystem castPs;
        [SerializeField] public Transform castParent;
        [System.NonSerialized] private int _health;
        [System.NonSerialized] private Tween _colorPunchTween;
        [System.NonSerialized] public int ID;
        [System.NonSerialized] private GameObject _dragTrail = null;
        [System.NonSerialized] public bool DragTarget = false;
        [System.NonSerialized] private Tween _castTweenLoop;
        // [System.NonSerialized] private Tween _wipeTween;

        // private static int WALK_HASH = Animator.StringToHash("Walk");
        // private static int DEATH_HASH = Animator.StringToHash("Death");
        // private static int HIT_HASH = Animator.StringToHash("Hit");
        // private static int CAST_HASH = Animator.StringToHash("Cast");
        // private static int CASTING_BOOL_HASH = Animator.StringToHash("Casting");
        
        private static int HIT_HASH = 0;
        private static int DEATH_HASH = 1;
        private static int WALK_HASH = 2;

        public void GetHitEnd()
        {
            CrossWalkAnimation();
        }
        public void DeathEnd()
        {
            GiveRewards();
            Warzone.THIS.Emit(so.deathEmitCount, thisTransform.position, so.colorGrad, so.radius);
            this.Deconstruct();
            LevelManager.THIS.CheckEndLevel();
        }

        public void PlayWalkAnimation()
        {
            animator.Play(WALK_HASH);
        }
        public void CrossWalkAnimation()
        {
            animator.Crossfade(WALK_HASH, 0.1f);
        }
        public void PlayDeathAnimation()
        {
            animator.Crossfade(DEATH_HASH, 0.1f);
        }
        public void PlayGetHitAnimation()
        {
            animator.Crossfade(HIT_HASH, 0.1f);
        }
        // private static int CAST_HASH = Animator.StringToHash("Cast");
        // private static int CASTING_BOOL_HASH = Animator.StringToHash("Casting");

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
                    // animator.SetBool(CASTING_BOOL_HASH, true);
                    // animator.SetTrigger(CAST_HASH);
                    break;
                case CastTypes.DestoryPawn:
                    if (castPs)
                    {
                        castPs.Stop();
                        castPs.transform.DOKill();
                    }
                    // animator.SetBool(CASTING_BOOL_HASH, true);
                    // animator.SetTrigger(CAST_HASH);
                    break;
                case CastTypes.SpawnEnemy:
                    if (castPs)
                    {
                        castPs.Play();
                    }
                    // animator.SetBool(CASTING_BOOL_HASH, true);
                    // animator.SetTrigger(CAST_HASH);
                    break;
            }

            if (so.spawnerDuration >= 0.0f)
            {
                _castTweenLoop?.Kill();
                _castTweenLoop = DOVirtual.DelayedCall(so.spawnerDuration, Cast, false).SetLoops(-1);
            }
        }
        public void OnCast()
        {
            // animator.SetBool(CASTING_BOOL_HASH, false);
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
                        Vector3 pos = Warzone.THIS.NextSpawnPosition(so.extraData.RandomForwardRange());
                        Particle.Lightning.Play(pos - CameraManager.THIS.gameCamera.transform.forward);
                        Enemy enemy = Warzone.THIS.CustomSpawnEnemy(so.extraData, pos);
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
            PlayWalkAnimation();
            // animator.Play(WALK_HASH);
            // animator.SetTrigger(WALK_HASH);
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
                return;
            }
            
            // animator.SetTrigger(HIT_HASH);
            PlayGetHitAnimation();
            // animator.Play(HIT_HASH);
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
                        Enemy enemy = Warzone.THIS.CustomSpawnEnemy(so.extraData, transform.position);
                        Vector3 target = thisTransform.position + new Vector3(Random.Range(-1.0f, 1.0f), 0.0f, Random.Range(0.35f, 2.0f));
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
            
            PlayDeathAnimation();
            // animator.Crossfade(DEATH_HASH, 0.1f);
            // animator.SetTrigger(DEATH_HASH);

            // _wipeTween?.Kill();
            // _wipeTween = DOVirtual.DelayedCall(so.wipeDelay, () =>
            // {
                
            // }, false);
        }

        private void GiveRewards()
        {
            foreach (var reward in so.enemyRewards)
            {
                if(Helper.IsPossible(reward.probability))
                {
                    switch (reward.type)
                    {
                        case UpgradeMenu.PurchaseType.CoinPack:
                            UIManagerExtensions.EmitEnemyCoinBurst(hitTarget.position, Mathf.Clamp(reward.amount, 0, 15), reward.amount);
                            break;
                        case UpgradeMenu.PurchaseType.Reserved1:
                            UIManagerExtensions.HeartToPlayer(hitTarget.position,  Mathf.Clamp(reward.amount, 0, 15), reward.amount);
                            break;
                    }
                }
            }
        }
    #endregion

        public void Deconstruct()
        {
            _castTweenLoop?.Kill();
            // _wipeTween?.Kill();

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
        
        [System.Serializable]
        public class SpawnData
        {
            [SerializeField] public int spawnDelay = 3;
            [SerializeField] public float spawnInterval = 6.0f;
            [SerializeField] public List<CountData> countDatas;
        } 
        [System.Serializable]
        public class CountData
        {
            [SerializeField] public EnemyData enemyData;
            [SerializeField] public int count;
            public CountData(EnemyData enemyData, int count)
            {
                this.enemyData = enemyData;
                this.count = count;
            }
        } 
        [System.Serializable]
        public class BossData
        {
            [SerializeField] public Pool enemyType;
            [SerializeField] public int count;
        } 
    }

}