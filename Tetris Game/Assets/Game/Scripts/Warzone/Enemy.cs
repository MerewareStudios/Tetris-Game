using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace  Game
{
    public class Enemy : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] public int Damage = 1;
        [SerializeField] public float Speed = 1.0f;
        // Self references
        [System.NonSerialized] private Transform _thisTransform;

    #region  Mono
        private void Awake()
        {
            _thisTransform = this.transform;
        }
        void Update()
        {
            _thisTransform.position += _thisTransform.forward * (Time.deltaTime * Speed);
            if (Warzone.THIS.Zone.IsOutside(_thisTransform))
            {
                Warzone.THIS.EnemyKamikaze(this);
            }
        }
    #endregion

    #region  Warzone
    
        public int _DamageTaken
        {
            set
            {
                Warzone.THIS.EnemyKilled(this);
            }
        }
    
        public void OnSpawn(Vector3 position)
        {
            _thisTransform.DOKill();
            
            _thisTransform.position = position;
            _thisTransform.forward = Vector3.back;

            _thisTransform.localScale = Vector3.zero;
            _thisTransform.DOScale(Vector3.one, 0.2f).SetEase(Ease.Linear);
        }
        public void Kamikaze()
        {
            _thisTransform.DOKill();
            Particle.Kamikaze.Play(_thisTransform.position);
            CameraManager.THIS.Shake();
            this.Despawn();
        }
        public void Kill()
        {
            _thisTransform.DOKill();
            Particle.BloodExplosion.Play(transform.position);
            this.Despawn();
        }
    #endregion
        
    }
}