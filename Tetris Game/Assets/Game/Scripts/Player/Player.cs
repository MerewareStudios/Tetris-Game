using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game
{
    public class Player : MonoBehaviour
    {
        [Header("Motion Settings")]
        [SerializeField] private Animator _animator;
        [SerializeField] private float turnRate = 6.0f;
        [SerializeField] private Transform holster;
        [System.NonSerialized] private Gun gun;

        [SerializeField] private Data _data;
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
            float targetAngle = -Vector2.SignedAngle(Vector2.up, direction);
            _currentAngle = Mathf.MoveTowardsAngle(_currentAngle, targetAngle, Time.deltaTime * turnRate);

            transform.eulerAngles = new Vector3(0.0f, _currentAngle, 0.0f);

            float angleDif = Mathf.Abs(_currentAngle - targetAngle);

            if ((_Data.time - gun._Data.prevShoot > gun._Data.shootInterval) && angleDif <= 1.0f)
            {
                int bulletCount = Board.THIS.ConsumeBullet();
                Shoot(bulletCount);
                gun._Data.prevShoot = _Data.time;
            }

            _Data.time += Time.deltaTime;
        }

#endregion
        
        public Data _Data
        {
            set
            {
                _data = value;
                var pos = transform.position;
                _selfPosition = new Vector2(pos.x, pos.z);
                UIManager.THIS.healthCounter.Value(_data.currentHealth, _data.maxHealth);

                if (gun)
                {
                    gun.Despawn();
                }

                gun = _data.gunData.type.GetPrefab().Spawn<Gun>(holster);
                gun._Data = _data.gunData;
            }
            get => _data;
        }
        
        public int _DamageTaken
        {
            set
            {
                _Data.currentHealth = Mathf.Clamp(_data.currentHealth - value, 0, _data.maxHealth);
                UIManager.THIS.healthCounter.Value(_data.currentHealth, _data.maxHealth);

                if (_Data.currentHealth == 0)
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
            bullet.transform.position = gun.muzzle.position;
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

            if (gun)
            {
                gun.Despawn();
                gun = null;
            }
        }
        
        public void Begin()
        {
            this.enabled = true;

            _currentAngle = 0.0f;
            transform.eulerAngles = new Vector3(0.0f, _currentAngle, 0.0f);
        }
        
        public void Reset()
        {
            _Data.currentHealth = _Data.maxHealth;
            _Data = _data;
        }
        
        [System.Serializable]
        public class Data : ICloneable
        {
            [SerializeField] public float time;
            [SerializeField] public int currentHealth = 1;
            [SerializeField] public int maxHealth = 1;
            [SerializeField] public Gun.Data gunData;

            
            public Data()
            {
                this.time = 0.0f;
                this.currentHealth = 1;
                this.maxHealth = 1;
                this.gunData = null;
            }
            public Data(Data data)
            {
                this.time = data.time;
                this.currentHealth = data.currentHealth;
                this.maxHealth = data.maxHealth;
                this.gunData = data.gunData.Clone() as Gun.Data;
            }

            public object Clone()
            {
                return new Data(this);
            }
        } 
    }
}
