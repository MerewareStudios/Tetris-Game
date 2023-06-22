using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DG.Tweening;
using Game.UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game
{
    public class Player : MonoBehaviour
    {
        [SerializeField] public Shield shield;
        [Header("Motion Settings")]
        [SerializeField] private Animator _animator;
        [SerializeField] private Transform holster;
        [SerializeField] public Transform shiledTarget;
        [System.NonSerialized] private Gun gun;

        [SerializeField] private Data _data;
        [System.NonSerialized] public System.Action OnDeath = null;

        [System.NonSerialized] private Vector2 _selfPosition;
        [System.NonSerialized] private float _currentAngle = 0.0f;
        
        [System.NonSerialized] private static readonly int SHOOT_HASH = Animator.StringToHash("Shoot");

#region  Mono

        void Start()
        {
            WeaponMenu.THIS.OnGunDataChanged += (newGunData) =>
            {
                newGunData.prevShoot = _GunData.prevShoot;
                _GunData = newGunData;
            };
        }


        private float prevShoot;
        void Update()
        {
            if (Warzone.THIS.Enemies.Count == 0)
            {
                return;
            }
            var targetPosition = Warzone.THIS.Enemies[0].transform.position;
            Vector2 direction = new Vector2(targetPosition.x, targetPosition.z) - _selfPosition;
            float targetAngle = -Vector2.SignedAngle(Vector2.up, direction);
            _currentAngle = Mathf.LerpAngle(_currentAngle, targetAngle, Time.deltaTime * _Data.turnRate);

            transform.eulerAngles = new Vector3(0.0f, _currentAngle, 0.0f);

            float angleDif = Mathf.Abs(_currentAngle - targetAngle);

            if ((_Data.time - gun._Data.prevShoot > gun._Data.fireRate) && angleDif <= 1.0f)
            {
                int bulletCount = Board.THIS.ConsumeBullet(_data.gunData.split);
                Shoot(bulletCount);
                gun._Data.prevShoot = _Data.time;


                float delay = Time.time - prevShoot;
                if (delay < gun._Data.fireRate)
                {
                    Debug.Log(delay);
                }
                prevShoot = Time.time;
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


                _GunData = _data.gunData;
                _ShieldData = _data.shieldData;
            }
            get => _data;
        }
        
        public Gun.Data _GunData
        {
            set
            {
                _data.gunData = value;
                if (gun)
                {
                    gun.Despawn();
                }

                gun = _data.gunData.gunType.GetPrefab().Spawn<Gun>(holster);
                gun._Data = _data.gunData;
            }
            get => _data.gunData;
        }
        
        public Shield.Data _ShieldData
        {
            set
            {
                _data.shieldData = value;
                shield._Data = _data.shieldData;
            }
            get => _data.shieldData;
        }
        
        public int _DamageTaken
        {
            set
            {
                _Data.currentHealth = Mathf.Clamp(_data.currentHealth - value, 0, _data.maxHealth);
                UIManager.THIS.healthCounter.Value(_data.currentHealth, _data.maxHealth);

                if (_Data.currentHealth <= 0)
                {
                    OnDeath?.Invoke();
                }
            }
        }
        public int _HealthGained
        {
            set
            {
                _Data.currentHealth = value;
                UIManager.THIS.healthCounter.ValueAnimated(_data.currentHealth, _data.maxHealth, 0.35f);
            }
            get => _Data.currentHealth;
        }

        public void Shoot(int bulletCount)
        {
            int shootCount = Mathf.Min(bulletCount, Warzone.THIS.Enemies.Count);

            if (shootCount > 0)
            {
                _animator.SetTrigger(SHOOT_HASH);
            }
            for (int i = 0; i < shootCount; i++)
            {
               gun.Shoot(Warzone.THIS.Enemies[i]);
            }
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
            [SerializeField] public float turnRate = 6.0f;
            [SerializeField] public Gun.Data gunData;
            [SerializeField] public Shield.Data shieldData;

            
            public Data()
            {
                this.time = 0.0f;
                this.currentHealth = 1;
                this.maxHealth = 1;
                this.turnRate = 6.0f;
                this.gunData = null;
                this.shieldData = null;
            }
            public Data(Data data)
            {
                this.time = data.time;
                this.currentHealth = data.currentHealth;
                this.maxHealth = data.maxHealth;
                this.turnRate = data.turnRate;
                this.gunData = data.gunData.Clone() as Gun.Data;
                this.shieldData = data.shieldData.Clone() as Shield.Data;
            }

            public object Clone()
            {
                return new Data(this);
            }
        } 
    }
}
