using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Internal.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace  Game
{
    public class Enemy : MonoBehaviour
    {
        [Header("Model")]
        [SerializeField] private Transform thisTransform;
        [SerializeField] public Transform modelPivot;
        [SerializeField] private Renderer skin;
        [SerializeField] private Animator animator;
        [SerializeField] private EnemyData so;
        [Header("Stats")]
        [System.NonSerialized] private int currentHealth = 1;
        // Self references
        [System.NonSerialized] public System.Action OnRemoved = null;
        [System.NonSerialized] private Tween _colorPunchTween;

        private static int WALK_HASH = Animator.StringToHash("Walk");
        private static int DEATH_HASH = Animator.StringToHash("Death");

    #region  Mono
        public void Walk()
        {
            thisTransform.position += thisTransform.forward * (Time.deltaTime * _Speed);
            if (Warzone.THIS.Zone.IsOutside(thisTransform))
            {
                Warzone.THIS.EnemyKamikaze(this);
            }
        }
    #endregion

    #region  Warzone

        public void Set(int healthValue, float speedValue)
        {
            _Health = healthValue;
            _Speed = speedValue;
            
            animator.SetTrigger(WALK_HASH);
        }
        public void TakeDamage(int value)
        {
            ColorPunch();
            Warzone.THIS.Emit(5, transform.position, currentHealth.Health2Color(), so.radius);
            _Health -= value;
            if (_Health <= 0)
            {
                Warzone.THIS.EnemyKilled(this);
            }
        }

        private void ColorPunch()
        {
            
        }
        
        public int _Health
        {
            set
            {
                currentHealth = value;
                // if (health > 0)
                // {
                    // modelPivot.localScale = Vector3.one * (1.0f + (health-1) * 0.75f);
                    // meshRenderer.SetColor(GameManager.MPB_ENEMY, GameManager.BaseColor, health.Health2Color());
                // }
            }
            get => currentHealth;
        }
        public float _Speed
        {
            set
            {
                so.speed = value;
            }
            get => so.speed;
        }
    
        public void OnSpawn(Vector3 position)
        {
            thisTransform.DOKill();
            
            thisTransform.position = position;
            thisTransform.forward = Vector3.back;

            thisTransform.localScale = Vector3.zero;
            thisTransform.DOScale(Vector3.one, 0.2f).SetEase(Ease.Linear);

            this.enabled = true;
        }
        public void Kamikaze()
        {
            Warzone.THIS.RemoveEnemy(this);
            OnRemoved?.Invoke();
            KamikazeDeconstruct();
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
            // Wallet.COIN.Transaction(1);
            // Particle.Coin.Emit(1, transform.position + new Vector3(0.0f, 0.25f, 0.0f));
            animator.SetTrigger(DEATH_HASH);

            DOVirtual.DelayedCall(1.25f, () =>
            {
                UIManagerExtensions.EmitEnemyCoin(thisTransform.position, 1, 1);

                Warzone.THIS.Emit(15, thisTransform.position, so.color, so.radius);
                this.Deconstruct();
            });
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
    }

}