using System.Collections.Generic;
using DG.Tweening;
using Game.UI;
using Internal.Core;
using TMPro;
using UnityEngine;

namespace  Game
{
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private HealthCanvas healthCanvas;
        [Header("Model")]
        [SerializeField] public Transform thisTransform;
        [SerializeField] public Transform hitTarget;
        [SerializeField] private Renderer skin;
        [SerializeField] private Animator animator;
        [SerializeField] public EnemyData so;
        [Header("Stats")]
        [System.NonSerialized] private int _health;
        // [System.NonSerialized] public System.Action OnRemoved = null;
        [System.NonSerialized] private Tween _colorPunchTween;
        [System.NonSerialized] public int ID;

        private static int WALK_HASH = Animator.StringToHash("Walk");
        private static int DEATH_HASH = Animator.StringToHash("Death");
        private static int HIT_HASH = Animator.StringToHash("Hit");

        public int Damage => Health;

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
            
            Warzone.THIS.CheckLandmine(thisTransform);
            
            if (Warzone.THIS.IsOutside(thisTransform))
            {
                Warzone.THIS.EnemyKilled(this, true);
            }
        }
    #endregion

    #region  Warzone

        public void Replenish()
        {
            Health = so.maxHealth;
            
            animator.SetTrigger(WALK_HASH);
            // skin.SetColor(GameManager.MPB_ENEMY, GameManager.EnemyEmisColor, Color.black);
            skin.material.SetColor(GameManager.EmissionKey, Color.black);
        }
        public void TakeDamage(int value, float scale = 1.0f)
        {
            ColorPunch();
            Warzone.THIS.Emit(so.emitCount, transform.position, so.colorGrad, so.radius);
            healthCanvas.DisplayDamage(-value, scale);
            Warzone.THIS.Player.PunchCrossHair();
            
            if (Health <= 0)
            {
                return;
            }
            
            Health -= value;
            if (Health <= 0)
            {
                Warzone.THIS.EnemyKilled(this);
            }
            else
            {
                animator.SetTrigger(HIT_HASH);
            }
        }

        public void Drag(float distance)
        {
            float finalDistance = Mathf.Min(Warzone.THIS.StartLine - thisTransform.position.z, distance); 
            thisTransform.DOKill();
            thisTransform.DOMoveZ(finalDistance, 0.5f).SetRelative(true).SetEase(Ease.OutSine);
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
            
            thisTransform.position = position;
            thisTransform.forward = Vector3.back;

            thisTransform.localScale = Vector3.zero;
            thisTransform.DOScale(Vector3.one, 0.2f).SetEase(Ease.Linear);

            this.enabled = true;
        }
        public void Kamikaze()
        {
            thisTransform.DOKill();
            Warzone.THIS.RemoveEnemy(this);
            Particle.Kamikaze.Play(thisTransform.position);
            this.Deconstruct();
        }

        public void Kill()
        {
            thisTransform.DOKill();
            Warzone.THIS.RemoveEnemy(this);
            
            animator.SetTrigger(DEATH_HASH);

            DOVirtual.DelayedCall(so.wipeDelay, () =>
            {
                GiveRewards();
                Warzone.THIS.Emit(so.deathEmitCount, thisTransform.position, so.colorGrad, so.radius);
                this.Deconstruct();
                LevelManager.THIS.CheckEndLevel();
            }, false);
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
                            UIManagerExtensions.EmitEnemyCoinBurst(thisTransform.position, Mathf.Clamp(reward.amount, 0, 15), reward.amount);
                            break;
                        case UpgradeMenu.PurchaseType.Reserved1:
                            UIManagerExtensions.HeartToPlayer(thisTransform.position,  Mathf.Clamp(reward.amount, 0, 15), reward.amount);
                            break;
                    }
                }
            }
        }
    #endregion

        public void Deconstruct()
        {
            // OnRemoved = null;
            this.Despawn();
        }
        
        // public enum Type
        // {
        //     Slime,
        //     Mushroom,
        //     Turtle,
        //     Cactus,
        //     Chest,
        //     Eye,
        // }
        
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
        } 
        [System.Serializable]
        public class BossData
        {
            [SerializeField] public Pool enemyType;
            [SerializeField] public int count;
        } 
    }

}