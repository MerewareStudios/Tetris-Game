using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DG.Tweening;
using Game.UI;
using Internal.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game
{
    public class Player : MonoBehaviour
    {
        [SerializeField] public Shield shield;
        [FormerlySerializedAs("_animator")]
        [Header("Motion Settings")]
        [SerializeField] public Animator animator;
        [SerializeField] private Transform holster;
        [SerializeField] public Transform shiledTarget;
        [System.NonSerialized] private Gun gun;

        [System.NonSerialized] private Data _data;
        [System.NonSerialized] public System.Action OnDeath = null;

        [System.NonSerialized] private Vector2 _selfPosition;
        [System.NonSerialized] private float _currentAngle = 0.0f;
        [System.NonSerialized] private bool _shouldGetUp = false;
        [System.NonSerialized] private Coroutine _searchRoutine = null;
        
        [System.NonSerialized] public static readonly int IDLE_HASH = Animator.StringToHash("Idle");
        [System.NonSerialized] public static readonly int SHOOT_HASH = Animator.StringToHash("Shoot");
        [System.NonSerialized] public static readonly int VICTORY_HASH = Animator.StringToHash("Victory");
        [System.NonSerialized] public static readonly int VICTORY_INF_HASH = Animator.StringToHash("VictoryInf");
        [System.NonSerialized] public static readonly int DEATH_HASH = Animator.StringToHash("Death");
        [System.NonSerialized] public static readonly int GETUP_HASH = Animator.StringToHash("GetUp");
        [System.NonSerialized] public static readonly int WAVE_HASH = Animator.StringToHash("Wave");
        [System.NonSerialized] public static readonly int SHOW_HASH = Animator.StringToHash("Show");
        [System.NonSerialized] public static readonly int POINT_HASH = Animator.StringToHash("Point");

        // public static int RANDOM_DEATH_HASH => DEATH_HASHES.Random(); 
#region  Mono

        void Start()
        {
            WeaponMenu.THIS.OnGunDataChanged += (newGunData) =>
            {
                newGunData.prevShoot = _GunData.prevShoot;
                _GunData = newGunData;
            };

            
        }

#endregion
        
        public Data _Data
        {
            set
            {
                _data = value;
                var pos = transform.position;
                _selfPosition = new Vector2(pos.x, pos.z);

                _CurrentHealth = _data.currentHealth;

                _GunData = _data.gunData;
                _ShieldData = _data.shieldData;
            }
            get => _data;
        }
        
        public int _CurrentHealth
        {
            set
            {
                _data.currentHealth = value;

                if (_data.currentHealth > 0)
                {
                    StatDisplayArranger.THIS.Show(StatDisplay.Type.Health, _data.currentHealth);
                }
                else
                {
                    StatDisplayArranger.THIS.Hide(StatDisplay.Type.Health);
                }
                
                if (_Data.currentHealth < 0)
                {
                    OnDeath?.Invoke();
                }
            }
            get => _data.currentHealth;
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
        
        public void ReplenishHealth()
        {
            if (_CurrentHealth > 0) return;
            
            _CurrentHealth = 0;
        }

        public void Shoot(int bulletCount)
        {
            int shootCount = Mathf.Min(bulletCount, Warzone.THIS.Enemies.Count);

            if (shootCount == 0)
            {
                animator.SetTrigger(SHOOT_HASH);
                gun.Bubble();

                if (ONBOARDING.NEED_MORE_AMMO_SPEECH.IsNotComplete())
                {
                    Onboarding.TalkAboutNeedMoreAmmo();
                    ONBOARDING.NEED_MORE_AMMO_SPEECH.SetComplete();
                    ONBOARDING.ALL_BLOCK_STEPS.SetComplete();
                    return;
                }
                return;
            }
            
            animator.SetTrigger(SHOOT_HASH);
            for (int i = 0; i < shootCount; i++)
            {
               gun.Shoot(Warzone.THIS.Enemies[i]);
            }
        }

        public void Deconstruct()
        {
            StopSearching();

            _currentAngle = 0.0f;
            transform.eulerAngles = new Vector3(0.0f, _currentAngle, 0.0f);

            if (gun)
            {
                gun.Despawn();
                gun = null;
            }
        }
        
        public void OnVictory()
        {
            StopSearching();
            shield.PauseProtection();
            animator.SetTrigger(VICTORY_HASH);
        }
        public void OnFail()
        {
            StopSearching();
            shield.PauseProtection();
            animator.SetTrigger(DEATH_HASH);
            _shouldGetUp = true;
        }
        public void StartSearching()
        {
            Warzone.THIS.Player.animator.SetTrigger(Player.IDLE_HASH);

            _searchRoutine = StartCoroutine(SearchEnemyRoutine());
            
            IEnumerator SearchEnemyRoutine()
            {
                while (true)
                {
                    if (Warzone.THIS.Enemies.Count > 0)
                    {
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
                        }

                        _Data.time += Time.deltaTime;
                    }

                    yield return null;
                }
            }
        }

        public void StopSearching()
        {
            if (_searchRoutine != null)
            {
                StopCoroutine(_searchRoutine);
                _searchRoutine = null;
            }
        }
        public void Replenish()
        {
            ReplenishHealth();

            transform.DOKill();
            transform.DORotate(Vector3.zero, 1.25f);
                // .onComplete += () =>
            // {
            //     this.enabled = true;
            // };

            if (_shouldGetUp)
            {
                animator.SetTrigger(GETUP_HASH);
                _shouldGetUp = false;
            }
        }
        
        public void RotateToPlayer(float rotateDuration)
        {
            transform.DOKill();
            transform.DORotate(new Vector3(0.0f, 180.0f, 0.0f), rotateDuration);
        }
        
        public void Reset()
        {
            _CurrentHealth = 0;
            _Data = _data;
        }
        
        [System.Serializable]
        public class Data : ICloneable
        {
            [SerializeField] public float time;
            [SerializeField] public int currentHealth = 0;
            [SerializeField] public float turnRate = 6.0f;
            [SerializeField] public Gun.Data gunData;
            [SerializeField] public Shield.Data shieldData;

            
            public Data()
            {
                this.time = 0.0f;
                this.currentHealth = 0;
                this.turnRate = 6.0f;
                this.gunData = null;
                this.shieldData = null;
            }
            public Data(Data data)
            {
                this.time = data.time;
                this.currentHealth = data.currentHealth;
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
