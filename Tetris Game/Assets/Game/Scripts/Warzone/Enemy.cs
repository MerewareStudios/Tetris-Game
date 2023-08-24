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
        [SerializeField] public Transform modelPivot;
        [SerializeField] private MeshRenderer meshRenderer;
        [Header("Stats")]
        [System.NonSerialized] private int health = 1;
        [System.NonSerialized] private float speed = 1.0f;
        // Self references
        [System.NonSerialized] private Transform _thisTransform;
        [System.NonSerialized] public System.Action OnRemoved = null;

    #region  Mono
        private void Awake()
        {
            _thisTransform = this.transform;
        }
        void Update()
        {
            _thisTransform.position += _thisTransform.forward * (Time.deltaTime * _Speed);
            if (Warzone.THIS.Zone.IsOutside(_thisTransform))
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
        }
        public int _DamageTaken
        {
            set
            {
                float radius = health * 0.1f;
                Warzone.THIS.Emit(health * 5, transform.position, health.Health2Color(), radius);
                _Health -= value;
                if (_Health <= 0)
                {
                    Warzone.THIS.EnemyKilled(this);
                }
            }
        }
        
        public int _Health
        {
            set
            {
                health = value;
                if (health > 0)
                {
                    modelPivot.localScale = Vector3.one * (1.0f + (health-1) * 0.75f);
                    meshRenderer.SetColor(GameManager.MPB_ENEMY, GameManager.BaseColor, health.Health2Color());
                }
            }
            get => health;
        }
        public float _Speed
        {
            set
            {
                speed = value;
            }
            get => speed;
        }
    
        public void OnSpawn(Vector3 position)
        {
            _thisTransform.DOKill();
            
            _thisTransform.position = position;
            _thisTransform.forward = Vector3.back;

            _thisTransform.localScale = Vector3.zero;
            _thisTransform.DOScale(Vector3.one, 0.2f).SetEase(Ease.Linear);

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
            _thisTransform.DOKill();
            Particle.Kamikaze.Play(_thisTransform.position);
            this.Deconstruct();
        }
        public void Kill()
        {
            _thisTransform.DOKill();
            Warzone.THIS.RemoveEnemy(this);
            // Wallet.COIN.Transaction(1);
            UIManagerExtensions.EmitEnemyCoin(transform.position, 1, 1);
            // Particle.Coin.Emit(1, transform.position + new Vector3(0.0f, 0.25f, 0.0f));
            this.Deconstruct();
        }
    #endregion

        public void Deconstruct()
        {
            OnRemoved = null;
            this.Despawn();
        }
        
    }
}