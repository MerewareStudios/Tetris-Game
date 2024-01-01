using System;
using System.Collections;
using DG.Tweening;
using Game.UI;
using Internal.Core;
using UnityEngine;

namespace Game
{
    public class Player : MonoBehaviour
    {
        [SerializeField] public Renderer skin;
        [SerializeField] public Animator animator;
        [SerializeField] private Transform holster;
        [SerializeField] private Transform crossHair;
        [SerializeField] private Transform crossHairScalePivot;
        [SerializeField] public MeshRenderer crossHairMR;
        [GradientUsage(true)] [SerializeField] private Gradient emissionGradient;
        
        [System.NonSerialized] private Tween _crossColorTween;
        [System.NonSerialized] public Gun Gun;
        [System.NonSerialized] private Data _data;
        [System.NonSerialized] private Vector2 _selfPosition;
        [System.NonSerialized] private float _currentAngle = 0.0f;
        [System.NonSerialized] private bool _shouldGetUp = false;
        [System.NonSerialized] private Coroutine _searchRoutine = null;
        [System.NonSerialized] private Enemy _currentEnemy = null;
        
        [System.NonSerialized] public float AutoEnemySortInterval = 1.0f;
        // [System.NonSerialized] private bool _playBubbleSound = true;
        // [System.NonSerialized] private int _shootBubbleCount = 0;

        public float Emission
        {
            set => skin.sharedMaterial.SetColor(GameManager.EmisKey, emissionGradient.Evaluate(value));
        }

        public Enemy CurrentEnemy
        {
            set
            {
                _data.LastTimeEnemySorted = Time.time;
                this._currentEnemy = value;
                crossHair.gameObject.SetActive(value);
            }
            get => this._currentEnemy;
        }
        
        public Vector3 Position
        {
            set
            {
                this.transform.position = value;
                _selfPosition = value.XZ();
            }
            get => this.transform.position;
        }
        
        [System.NonSerialized] public static readonly int IDLE_HASH = Animator.StringToHash("Idle");
        [System.NonSerialized] public static readonly int SHOOT_HASH = Animator.StringToHash("Shoot");
        [System.NonSerialized] public static readonly int VICTORY_HASH = Animator.StringToHash("Victory");
        [System.NonSerialized] public static readonly int VICTORY_INF_HASH = Animator.StringToHash("VictoryInf");
        [System.NonSerialized] public static readonly int DEATH_HASH = Animator.StringToHash("Death");
        [System.NonSerialized] public static readonly int GETUP_HASH = Animator.StringToHash("GetUp");
        [System.NonSerialized] public static readonly int WAVE_HASH = Animator.StringToHash("Wave");
        [System.NonSerialized] public static readonly int SHOW_HASH = Animator.StringToHash("Show");
        [System.NonSerialized] public static readonly int POINT_HASH = Animator.StringToHash("Point");

#region  Mono

        void Start()
        {
            WeaponMenu.THIS.GunDataChanged += (newGunData) =>
            {
                UpdateGunData(newGunData);
            };
            crossHairMR.sharedMaterial.SetColor(GameManager.BaseColor, Color.white);
        }

#endregion

        private void UpdateGunData(Gun.Data newGunData)
        {
            newGunData.PrevShoot = _GunData.PrevShoot;
            _GunData = newGunData;
        }
                
        public Data _Data
        {
            set
            {
                _data = value;

                _CurrentHealth = _data.currentHealth;

                _GunData = WeaponMenu.THIS.EquippedGunData;

                if (ONBOARDING.PASSIVE_META.IsNotComplete())
                {
                    StatDisplayArranger.THIS.HideImmediate(StatDisplay.Type.Health);
                }
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
                    StatDisplayArranger.THIS.Show(StatDisplay.Type.Health, _data.currentHealth, punch:true);
                }
                else
                {
                    StatDisplayArranger.THIS.Hide(StatDisplay.Type.Health);
                }
            }
            get => _data.currentHealth;
        }
        
        public Gun.Data _GunData
        {
            set
            {
                if (Gun && !Gun._Data.gunType.Equals(value.gunType))
                {
                    Gun.Despawn(Gun._Data.gunType);
                    Gun = null;
                }
                if (!Gun)
                {
                    Gun = value.gunType.Spawn<Gun>(holster);
                }

                Gun._Data = new Gun.Data(value);
            }
            get => Gun._Data;
        }
        
        public void ReplenishHealth()
        {
            if (_CurrentHealth > 0) return;
            
            _CurrentHealth = 1;
        }

        public void Shoot(int bulletCount)
        {
            int shootCount = Mathf.Min(bulletCount, Warzone.THIS.EnemyCount);
#if CREATIVE
            if (!Const.THIS.creativeSettings.playerBubble && shootCount == 0)
            {
                return;
            }
#endif
            if (shootCount == 0)
            {
                // if (_shootBubbleCount > 0)
                {
                    Audio.Bubble.PlayOneShot();
                    Gun.Bubble(4);
                    animator.SetTrigger(SHOOT_HASH);
                    // _shootBubbleCount--;
                }
               


                if (ONBOARDING.ALL_BLOCK_STEPS.IsNotComplete())
                {
                    Onboarding.TalkAboutNeedMoreAmmo();
                    ONBOARDING.ALL_BLOCK_STEPS.SetComplete();
                    return;
                }
                return;
            }

            // _shootBubbleCount = 1;
            // _volume = 1.0f;
            // _playBubbleSound = true;
            // _canShootBubble = 4;
            
            animator.SetTrigger(SHOOT_HASH);
            for (int i = 0; i < shootCount; i++)
            {
               Gun.Shoot(Warzone.THIS.GetEnemy(i));
            }
            Gun.PlaySound();
        }

        public void Deconstruct()
        {
            StopSearching();

            _currentAngle = 0.0f;
            transform.eulerAngles = new Vector3(0.0f, _currentAngle, 0.0f);

            if (Gun)
            {
                Gun.Despawn(Gun._Data.gunType);
                Gun = null;
            }
        }
        
        public void OnVictory()
        {
            StopSearching();
            animator.SetTrigger(VICTORY_HASH);
        }
        public void OnFail()
        {
            Audio.Scream.Play();
            StopSearching();
            animator.SetTrigger(DEATH_HASH);
            _shouldGetUp = true;
        }
        public void StartSearching()
        {
            Warzone.THIS.Player.animator.SetTrigger(Player.IDLE_HASH);

            StopSearching();
            _searchRoutine = StartCoroutine(SearchEnemyRoutine());

            
            IEnumerator SearchEnemyRoutine()
            {
                _currentAngle = transform.eulerAngles.y;
                float smoothFactor = 0.0f;
                crossHair.gameObject.SetActive(true);

                _Data.Time = 0.0f;
                Gun._Data.PrevShoot = 0.0f;

                while (true)
                {
                    if (CurrentEnemy)
                    {
                        var targetPosition = CurrentEnemy.Position;

                        crossHair.position = Vector3.Lerp(crossHair.position, targetPosition, Time.deltaTime * _Data.TurnRate * smoothFactor);
                        float enemyRotFactor = CurrentEnemy.so.speed * 20.0f;
                        crossHair.localScale = Vector3.Lerp(crossHair.localScale, CurrentEnemy.CrossSize, Time.deltaTime * _Data.TurnRate * smoothFactor * enemyRotFactor);
                        
                        Vector2 direction = targetPosition.XZ() - _selfPosition;
                        float targetAngle = -Vector2.SignedAngle(Vector2.up, direction);

                        smoothFactor = Mathf.Lerp(smoothFactor, 1.0f, Time.deltaTime * 10.0f);
                        _currentAngle = Mathf.LerpAngle(_currentAngle, targetAngle, Time.deltaTime * _Data.TurnRate * smoothFactor * _GunData.Mult);

                        transform.eulerAngles = new Vector3(0.0f, _currentAngle, 0.0f);

                        float angleDif = Mathf.DeltaAngle(_currentAngle, targetAngle);
                        
                        if ((_Data.Time - Gun._Data.PrevShoot >= Gun._Data.FireInterval) && angleDif <= 2.0f)
                        {
                            int givenBulletCount = Board.THIS.TakeBullet(_GunData.SplitAmount);
#if CREATIVE
                            if (Input.GetKey(KeyCode.Space))
                            {
                                Shoot(givenBulletCount);
                            }
#else
                            Shoot(givenBulletCount);
#endif
                            Gun._Data.PrevShoot = _Data.Time;
                        }

                        _Data.Time += Time.deltaTime;


                        if (Time.time - _data.LastTimeEnemySorted > AutoEnemySortInterval)
                        {
                            Warzone.THIS.AssignClosestEnemy();
                            _data.LastTimeEnemySorted = Time.time;
                        }
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

            CurrentEnemy = null;
            crossHair.gameObject.SetActive(false);
        }
        public void Replenish(float sortInterval)
        {
            ReplenishHealth();

            // _shootBubbleCount = 1;

            this.AutoEnemySortInterval = sortInterval;
            
            crossHair.gameObject.SetActive(false);
            crossHair.position = Position + new Vector3(0.0f, 0.0f, -4.0f);

            Emission = 0.0f;
            UIManager.THIS.powerEffect.enabled = false;

            if (_shouldGetUp)
            {
                animator.SetTrigger(GETUP_HASH);
                _shouldGetUp = false;
            }
        }

        public void PunchCrossHair()
        {
            _crossColorTween?.Kill();
            
            float timeStep = 0.0f;
            _crossColorTween = DOTween.To((x) => timeStep = x, 0.0f, 1.0f, 0.25f);
            _crossColorTween.onUpdate = () =>
            {
                Color currentColor = Const.THIS.hitGradient.Evaluate(timeStep);
                crossHairMR.sharedMaterial.SetColor(GameManager.BaseColor, currentColor);

                crossHairScalePivot.localScale = Vector3.one * Const.THIS.hitScaleCurve.Evaluate(timeStep);
            };
        }
        
        public void RotateToPlayer(float rotateDuration)
        {
            transform.DOKill();
            transform.DORotate(new Vector3(0.0f, 180.0f, 0.0f), rotateDuration);
        }
        
        public void ResetSelf()
        {
            Gun.ResetSelf();
        }
        
        [System.Serializable]
        public class Data : ICloneable
        {
            [SerializeField] public int currentHealth = 0;
            [System.NonSerialized] public float Time;
            [System.NonSerialized] public float LastTimeEnemySorted;
            [System.NonSerialized] public int TurnRate = 12;

            
            public Data()
            {
                this.Time = 0.0f;
                this.currentHealth = 0;
            }
            public Data(Data data)
            {
                this.currentHealth = data.currentHealth;
            }

            public object Clone()
            {
                return new Data(this);
            }
        } 
    }
}
