using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DG.Tweening;
using UnityEngine;

namespace Game
{
    public class Player : MonoBehaviour
    {
        [Header("Motion Settings")]
        [SerializeField] private Animator _animator;
        [SerializeField] private Transform muzzle;
        [SerializeField] private float turnRate = 6.0f;

        [System.NonSerialized] private Data _data;
        [System.NonSerialized] public System.Action OnDeath = null;

        [System.NonSerialized] private Vector2 _selfPosition;
        [System.NonSerialized] private float _currentAngle = 0.0f;
        
        [System.NonSerialized] private static readonly int SHOOT_HASH = Animator.StringToHash("Shoot");

#region  Mono

        void Update()
        {
            if (Warzone.THIS.Enemies.Count == 0)
            {
                return;
            }
            var targetPosition = Warzone.THIS.Enemies[0].transform.position;
            Vector2 direction = new Vector2(targetPosition.x, targetPosition.z) - _selfPosition;
            float angle = -Vector2.SignedAngle(Vector2.up, direction);
            _currentAngle = Mathf.MoveTowardsAngle(_currentAngle, angle, Time.deltaTime * turnRate);

            transform.eulerAngles = new Vector3(0.0f, _currentAngle, 0.0f);
        }

#endregion
        
        public Data _Data
        {
            set
            {
                _data = value;
                _selfPosition = new Vector2(transform.position.x, transform.position.z);
                UIManager.THIS.healthCounter.Value(_data.CurrentHealth, _data.MaxHealth);
            }
            get => _data;
        }
        
        public int _DamageTaken
        {
            set
            {
                _Data.CurrentHealth = Mathf.Clamp(_data.CurrentHealth - value, 0, _data.MaxHealth);
                UIManager.THIS.healthCounter.Value(_data.CurrentHealth, _data.MaxHealth);

                if (_Data.CurrentHealth == 0)
                {
                    OnDeath?.Invoke();
                }
            }
        }

        public void Shoot(int bulletCount)
        {
            int shootCount = Mathf.Min(bulletCount, Warzone.THIS.Enemies.Count);
            
            for (int i = 0; i < shootCount; i++)
            {
                Shoot(Warzone.THIS.Enemies[i]);
            }
        }
        
        public void Shoot(Enemy enemy)
        {
            _animator.SetTrigger(SHOOT_HASH);

            Transform enemyTransform = enemy.transform;
            Vector3 targetPosition = enemyTransform.position;

            enemy.OnRemoved += () =>
            {
                enemyTransform = null;
            };
            
            Transform bullet = Pool.Bullet.Spawn().transform;
            bullet.DOKill();
            bullet.transform.position = muzzle.position;
            Tween bulletTween = bullet.DOJump(enemyTransform.position, 2.0f, 1, 0.5f).SetEase(Ease.Linear);
            bulletTween.onUpdate += () =>
            {
                if (enemyTransform)
                {
                    targetPosition = enemyTransform.position;
                }
                bulletTween.SetTarget(targetPosition);
            };
            bulletTween.onComplete += () =>
            {
                if (enemyTransform)
                {
                    enemy._DamageTaken = 1;
                }
                bullet.Despawn();
            };
        }

        public void Deconstruct()
        {
            this.enabled = false;

            _currentAngle = 0.0f;
            transform.eulerAngles = new Vector3(0.0f, _currentAngle, 0.0f);
        }
        
        [System.Serializable]
        public class Data : ICloneable
        {
            [SerializeField] public int CurrentHealth = 1;
            [SerializeField] public int MaxHealth = 1;

            
            public Data()
            {
                this.CurrentHealth = 1;
                this.MaxHealth = 1;
            }
            public Data(Data data)
            {
                this.CurrentHealth = data.CurrentHealth;
                this.MaxHealth = data.MaxHealth;
            }

            public object Clone()
            {
                return new Data(this);
            }
        } 
    }
}
