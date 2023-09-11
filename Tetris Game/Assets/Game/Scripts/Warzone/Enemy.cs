using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game.UI;
using Internal.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace  Game
{
    public class Enemy : MonoBehaviour
    {
        [Header("Model")]
        [SerializeField] public Transform thisTransform;
        [SerializeField] public Transform modelPivot;
        [SerializeField] private Renderer skin;
        [SerializeField] private Animator animator;
        [SerializeField] private EnemyData so;
        [Header("Stats")]
        [System.NonSerialized] public int Health = 1;
        [System.NonSerialized] public System.Action OnRemoved = null;
        [System.NonSerialized] private Tween _colorPunchTween;

        private static int WALK_HASH = Animator.StringToHash("Walk");
        private static int DEATH_HASH = Animator.StringToHash("Death");
        private static int HIT_HASH = Animator.StringToHash("Hit");

        public int Damage => so.damage;
        public Vector3 CrossSize => new Vector3(so.crossSize, so.crossSize, so.crossSize);

    #region  Mono
        public void Walk()
        {
            thisTransform.Translate(new Vector3(0.0f, 0.0f, Time.deltaTime * so.speed));
            if (Warzone.THIS.Zone.IsOutside(thisTransform))
            {
                Warzone.THIS.EnemyKamikaze(this);
            }
        }
    #endregion

    #region  Warzone

        private void Replenish()
        {
            Health = so.maxHealth;
            
            animator.SetTrigger(WALK_HASH);
            skin.SetColor(GameManager.MPB_ENEMY, GameManager.EnemyEmisColor, Color.black);
        }
        public void TakeDamage(int value)
        {
            ColorPunch();
            Warzone.THIS.Emit(so.emitCount, transform.position, so.color, so.radius);
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

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.O))
            {
                animator.SetTrigger(HIT_HASH);
            }
        }

        private void ColorPunch()
        {
            _colorPunchTween?.Kill();
            float timeStep = 0.0f;
            _colorPunchTween = DOTween.To((x) => timeStep = x, 0.0f, 1.0f, 0.35f).SetEase(Ease.Linear);
            _colorPunchTween.onUpdate = () =>
            {
                skin.SetColor(GameManager.MPB_ENEMY, GameManager.EnemyEmisColor, so.hitGradient.Evaluate(timeStep));
            };
        }
        
        public void OnSpawn(Vector3 position)
        {
            Replenish();

            thisTransform.DOKill();
            
            thisTransform.position = position;
            thisTransform.forward = Vector3.back;

            thisTransform.localScale = Vector3.zero;
            thisTransform.DOScale(Vector3.one, 0.2f).SetEase(Ease.Linear);

            this.enabled = true;
        }
        public void Kamikaze(bool giveRewards)
        {
            if (giveRewards)
            {
                GiveRewards();
            }
            Warzone.THIS.RemoveEnemy(this);
            OnRemoved?.Invoke();
            KamikazeDeconstruct();
            LevelManager.THIS.CheckVictory();
        }

        public void KamikazeDeconstruct()
        {
            thisTransform.DOKill();
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
                Warzone.THIS.Emit(so.deathEmitCount, thisTransform.position, so.color, so.radius);
                this.Deconstruct();
                LevelManager.THIS.CheckVictory();
            });
        }

        private void GiveRewards()
        {
            foreach (var reward in so.enemyRewards)
            {
                Helper.IsPossible(reward.rewardProbability, () =>
                {
                    switch (reward.type)
                    {
                        case UpgradeMenu.PurchaseType.Coin:
                            UIManagerExtensions.EmitEnemyCoinBurst(thisTransform.position, Mathf.Clamp(reward.rewardAmount, 0, 15), reward.rewardAmount);
                            break;
                        case UpgradeMenu.PurchaseType.Heart:
                            UIManagerExtensions.HeartToPlayer(thisTransform.position,  Mathf.Clamp(reward.rewardAmount, 0, 15), reward.rewardAmount);
                            break;
                        case UpgradeMenu.PurchaseType.Shield:
                            UIManagerExtensions.ShieldToPlayer(thisTransform.position,  Mathf.Clamp(reward.rewardAmount, 0, 15), reward.rewardAmount);
                            break;
                    }
                });
            }
        }
    #endregion

        public void Deconstruct()
        {
            OnRemoved = null;
            this.Despawn();
        }
        
        public enum Type
        {
            Slime,
            Mushroom,
            Turtle,
            Cactus,
            Chest,
            Eye,
        }
        
        [System.Serializable]
        public class SpawnData
        {
            [SerializeField] public int spawnDelay = 3;
            [SerializeField] public float spawnInterval = 0.2f;
            [SerializeField] public List<CountData> countDatas;
            [SerializeField] public List<BossData > bossDatas;
        } 
        [System.Serializable]
        public class CountData
        {
            [SerializeField] public Pool enemyType;
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