using System;
using System.Collections;
using DG.Tweening;
using Game.UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game
{
    public class Player : MonoBehaviour
    {
        [SerializeField] public Renderer skin;
        [FormerlySerializedAs("_animator")]
        [Header("Motion Settings")]
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

        public float Emission
        {
            set => skin.sharedMaterial.SetColor(GameManager.EmissionKey, emissionGradient.Evaluate(value));
        }

        public Enemy CurrentEnemy
        {
            set
            {
                this._currentEnemy = value;
                crossHair.gameObject.SetActive(value);
            }
            get => this._currentEnemy;
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

                _GunData = WeaponMenu.THIS.EquippedGunData;

                if (ONBOARDING.INSPECT_HEART_DISPLAY.IsNotComplete())
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
                    Gun.Despawn();
                    Gun = null;
                }
                if (!Gun)
                {
                    Gun = value.gunType.Spawn<Gun>(holster);
                }

                Gun._Data = value;
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

            if (shootCount == 0)
            {
                animator.SetTrigger(SHOOT_HASH);
                Gun.Bubble();

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
               Gun.Shoot(Warzone.THIS.GetEnemy(i));
            }
            // CameraManager.THIS.Shake(0.1f, 0.3f);
        }

        public void Deconstruct()
        {
            StopSearching();

            _currentAngle = 0.0f;
            transform.eulerAngles = new Vector3(0.0f, _currentAngle, 0.0f);

            if (Gun)
            {
                Gun.Despawn();
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
            StopSearching();
            animator.SetTrigger(DEATH_HASH);
            _shouldGetUp = true;
        }
        public void StartSearching()
        {
            Warzone.THIS.Player.animator.SetTrigger(Player.IDLE_HASH);

            _searchRoutine = StartCoroutine(SearchEnemyRoutine());

            
            IEnumerator SearchEnemyRoutine()
            {
                _currentAngle = transform.eulerAngles.y;
                float smoothFactor = 0.0f;
                crossHair.gameObject.SetActive(true);

                while (true)
                {
                    if (CurrentEnemy)
                    {
                        var targetPosition = CurrentEnemy.thisTransform.position;

                        crossHair.position = Vector3.Lerp(crossHair.position, targetPosition, Time.deltaTime * _Data.turnRate * smoothFactor);
                        crossHair.localScale = Vector3.Lerp(crossHair.localScale, CurrentEnemy.CrossSize, Time.deltaTime * _Data.turnRate * smoothFactor);
                        
                        Vector2 direction = new Vector2(targetPosition.x, targetPosition.z) - _selfPosition;
                        float targetAngle = -Vector2.SignedAngle(Vector2.up, direction);

                        smoothFactor = Mathf.Lerp(smoothFactor, 1.0f, Time.deltaTime * 10.0f);
                        _currentAngle = Mathf.LerpAngle(_currentAngle, targetAngle, Time.deltaTime * _Data.turnRate * smoothFactor);

                        transform.eulerAngles = new Vector3(0.0f, _currentAngle, 0.0f);

                        float angleDif = Mathf.DeltaAngle(_currentAngle, targetAngle);
                        
                        if ((_Data.time - Gun._Data.prevShoot > Gun._Data.FireInterval) && angleDif <= 1.0f)
                        {
                            int givenBulletCount = Board.THIS.ConsumeBullet(_GunData.SplitAmount);
                            Shoot(givenBulletCount);
                            Gun._Data.prevShoot = _Data.time;
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

            CurrentEnemy = null;
            crossHair.gameObject.SetActive(false);
        }
        public void Replenish()
        {
            ReplenishHealth();
            
            crossHair.gameObject.SetActive(false);
            crossHair.position = Vector3.zero;

            Emission = 0.0f;

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
            // _CurrentHealth = 0;
            // _Data = _data;
            Gun.ResetSelf();
        }
        
        [System.Serializable]
        public class Data : ICloneable
        {
            [SerializeField] public float time;
            [SerializeField] public int currentHealth = 0;
            [SerializeField] public int turnRate = 6;

            
            public Data()
            {
                this.time = 0.0f;
                this.currentHealth = 0;
                this.turnRate = 6;
            }
            public Data(Data data)
            {
                this.time = data.time;
                this.currentHealth = data.currentHealth;
                this.turnRate = data.turnRate;
            }

            public object Clone()
            {
                return new Data(this);
            }
        } 
    }
}
