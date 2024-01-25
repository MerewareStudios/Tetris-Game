using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.UI;
using Internal.Core;
using UnityEngine;

namespace  Game
{
    public class Warzone : Lazyingleton<Warzone>
    {
        [Header("Players")] [SerializeField] public Player Player;
        [Header("Zones")] [SerializeField] public ParticleSystem bloodPS;
        [SerializeField] public Vector3 goreOffset;
        [SerializeField] public Airplane airplane;
        [SerializeField] public Progressbar enemyProgressbar;
        [System.NonSerialized] public ParticleSystem.MainModule psMain;
        [System.NonSerialized] public ParticleSystem.ShapeModule psShape;
        [System.NonSerialized] public Transform psTransform;
        [System.NonSerialized] public float StartLine;
        [System.NonSerialized] public float EndLine;
        [System.NonSerialized] public float SpawnRange;

        [System.NonSerialized] private Coroutine _spawnRoutine = null;
        [System.NonSerialized] private List<Enemy> _enemies = new();
        [System.NonSerialized] private int _enemyID = 0;

        [System.NonSerialized] private readonly List<SubModel> _landMines = new();

        private bool Spawning { get; set; }
        private bool HasEnemy => _enemies.Count > 0;
        public bool IsCleared => !Spawning && !HasEnemy;
        public int EnemyCount => _enemies.Count;
        public Enemy GetEnemy(int index) => _enemies[index];
        private int GetNewEnemyID() => ++_enemyID;

        private float _maxEnemyCount = 0;
        private int _totalEnemyCount = 0;

        #region Warzone

        void Awake()
        {
            psMain = bloodPS.main;
            psShape = bloodPS.shape;
            psTransform = bloodPS.transform;
        }

        #endregion

        void Update()
        {
            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                _enemies[i].Move();
            }
        }

        #region Warzone

        public void EnemyKilled(Enemy enemy, bool selfKill = false)
        {
            enemy.ID = 0;
            if (selfKill)
            {
                enemy.Kamikaze();
                CameraManager.THIS.Shake();
                this.Player._CurrentHealth -= enemy.Damage;
                if (Player._CurrentHealth > 0)
                {
                    enemy.GiveRewards();
                }
                LevelManager.THIS.CheckEndLevel();
            }
            else
            {
                enemy.Kill();
            }
        }

        public void ResetSelf()
        {
            Player.ResetSelf();
        }

        public void Begin()
        {
            if (this.enabled)
            {
                return;
            }
            
            StopSpawning();
            _spawnRoutine = StartCoroutine(SpawnRoutine());
            
            AnalyticsManager.LevelStart(LevelManager.CurrentLevel);
            
            this.enabled = true;
            return;


            IEnumerator SpawnRoutine()
            {
                Spawning = true;

                _totalEnemyCount = 0;

                int spawnIndex = 0;

                int totalHealth = 0;
                foreach (var data in LevelManager.LevelSo.enemySpawnData)
                {
                    totalHealth += data.enemyData.maxHealth * data.count;
                    _totalEnemyCount += data.count;
                }

                _maxEnemyCount = _totalEnemyCount;
                UpdateEnemyProgress();

                int totalCoinLeft = LevelManager.LevelSo.totalCoin;
                float coinPerHealth = totalCoinLeft / (float)totalHealth;

                
                Player.StartSearching();
                
                Audio.Level_Begin.Play();
                
#if CREATIVE
                yield return new WaitForSeconds(0.5f);
#else
                if (LevelManager.LevelSo.countdown > 0)
                {
                    // string startingText = string.Format(Onboarding.THIS.waveText, LevelManager.CurrentLevel);
                    string startingText = Onboarding.THIS.enemiesComingText;

                    // yield return Announcer.THIS.Count(startingText, LevelManager.LevelSo.countdown);
                    yield return Announcer.THIS.Show(startingText, 0.4f);
                }
                else
                {
                    string startingText = Onboarding.THIS.targetPracticeText;
                    yield return Announcer.THIS.Show(startingText, 0.4f);
                }
#endif
                
#if CREATIVE
                if (LevelManager.CurrentLevel > airplane.SavedData.arrival && Const.THIS.creativeSettings.airplane)
#else
                if (LevelManager.CurrentLevel > airplane.SavedData.arrival)
#endif
                {
                    Airplane.CarryData carryData = LevelManager.LevelSo.carryData;
                    if (!(carryData.type.Equals(Cargo.Type.MaxStack) && Board.THIS.SavedData.unlimitedStack))
                    {
                        if (carryData.delay >= 0)
                        {
                            airplane.CarryCargo(carryData);
                        }
                    }
                }


                while (true)
                {
                    LevelSo.EnemySpawnDatum enemySpawnDatum = LevelManager.LevelSo.enemySpawnData[spawnIndex];

                    Enemy enemy = null;

                    float start = 0.0f;
                    for (int i = 0; i < enemySpawnDatum.count; i++)
                    {
                        enemy = SpawnEnemy(enemySpawnDatum.enemyData, start + (enemySpawnDatum.count - 1) * enemySpawnDatum.distance * -0.5f - enemySpawnDatum.offset);
                        start += enemySpawnDatum.distance;
                        
                        int coinAmount = Mathf.Max(Mathf.FloorToInt(enemy.Health * coinPerHealth), 1);

                        enemy.CoinAmount = coinAmount;

                        totalCoinLeft -= coinAmount;
                            
                        _enemies.Add(enemy);
                    }
                    
                    if (!Player.CurrentEnemy)
                    {
                        AssignClosestEnemy();
                    }

                    spawnIndex++;

                    if (spawnIndex == LevelManager.LevelSo.enemySpawnData.Length && enemy && totalCoinLeft > 0)
                    {
                        enemy.CoinAmount += totalCoinLeft;
                    }

                    float waitTill = Time.time + enemySpawnDatum.delay * 2.0f;

                    if (spawnIndex >= LevelManager.LevelSo.enemySpawnData.Length)
                    {
                        break;
                    }

                    yield return new WaitWhile(() => Time.time < waitTill && HasEnemy);

                    if (Time.time < waitTill)
                    {
                        yield return new WaitForSeconds((waitTill - Time.time) * 0.04f);
                    }
                }

                Spawning = false;
                _spawnRoutine = null;
            }
        }

        private void UpdateEnemyProgress()
        {
            enemyProgressbar.Fill = _totalEnemyCount / _maxEnemyCount;
        }

        public void OnLateLoad(float sortInterval)
        {
            Player.Replenish(sortInterval);
        }

        private void StopSpawning()
        {
            if (_spawnRoutine != null)
            {
                StopCoroutine(_spawnRoutine);
                _spawnRoutine = null;
            }

            this.enabled = false;
            Spawning = false;
        }

        public Enemy SpawnEnemy(EnemyData enemyData, float hor)
        {
            Enemy enemy = enemyData.type.Spawn<Enemy>(this.transform);
            enemy.so = enemyData;
            
            Vector3 pos = new Vector3(hor * SpawnRange, 0.0f, Mathf.Lerp(EndLine, StartLine, enemy.so.RandomForwardRange()));

            enemy.OnSpawn(pos, GetNewEnemyID());
            enemy.Replenish();

            _totalEnemyCount--;
            UpdateEnemyProgress();

            return enemy;
        }
        public Enemy CustomSpawnEnemy(EnemyData enemyData, Vector3 position, int coinAmount)
        {
            if (EnemyCount > 30)
            {
                return null;
            }
            Enemy enemy = enemyData.type.Spawn<Enemy>(this.transform);
            enemy.so = enemyData;
            enemy.CoinAmount = coinAmount;
            enemy.OnSpawn(position, GetNewEnemyID());
            enemy.Replenish();
            
            Warzone.THIS.Emit(10, enemy.Position, enemy.so.colorGrad, 1.0f);

            _enemies.Add(enemy);
            
            return enemy;
        }

        public void RemoveEnemy(Enemy enemy)
        {
            _enemies.Remove(enemy);
            AssignClosestEnemy();
        }

        public void AssignClosestEnemy()
        {

            
            //Sort enemies by distance
            _enemies = _enemies.OrderBy(enemy => enemy.PositionZ - EndLine).ToList();
            
#if CREATIVE
            if (Const.THIS.creativeSettings.assignFurthest)
            {
                _enemies.Reverse();
            }
#endif
            
            //Assign first enemy as current target
            Player.CurrentEnemy = _enemies.FirstOrDefault();
        }

        public Vector3 RandomPos(float forwardPercent)
        {
            return new Vector3(Random.Range(-SpawnRange, SpawnRange), 0.0f, Mathf.Lerp(EndLine, StartLine, forwardPercent));
        }

        public bool EnemyExits() => _enemies.Count > 0;
        
        public Enemy GetRandomTarget()
        {
            if (_enemies.Count == 0)
            {
                return null;
            }

            return _enemies.Random();
        }
        
        public Enemy GetAoeTarget()
        {
            if (_enemies.Count == 0)
            {
                return null;
            }

            return _enemies[0];
        } 
        
        public Enemy GetClosestTarget()
        {
            if (_enemies.Count == 0)
            {
                return null;
            }

            List<Enemy> sortedEnemies = _enemies.OrderBy(enemy => enemy.PositionZ).ToList();

            foreach (var enemy in sortedEnemies)
            {
                if (!enemy.DragTarget)
                {
                    return enemy;
                }
            }
            
            return _enemies[0];
        } 
        
        public Vector3 GetLandMineTarget()
        {
            return new Vector3(Random.Range(-SpawnRange, SpawnRange), 0.0f, EndLine);
        }

        public void AEODamage(Vector3 position, int damage, float maxDistance)
        {
            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                float distance = (position.XZ() - _enemies[i].PositionXZ).magnitude;
                if (distance <= maxDistance)
                {
                    _enemies[i].TakeDamage((int)Mathf.Lerp(damage, 0.0f, distance / maxDistance), 2.0f);
                }
            }
        }
        
        public Vector3 RandomInvalidPosition()
        {
            return new Vector3(Random.Range(-SpawnRange, SpawnRange), 0.0f, Random.Range(EndLine, StartLine));
        }
        
        public Vector3 RandomInvalidForwardPosition()
        {
            return new Vector3(Random.Range(-SpawnRange, SpawnRange), 1.0f, StartLine + 0.5f);
        }

        public void AddLandMine(SubModel subModel)
        {
            this._landMines.Add(subModel);
        }
        
        public void ExplodeLandMine(Enemy enemy, SubModel subModel)
        {
            subModel.OnDespawn();
            _landMines.Remove(subModel);
            Vector3 pos = subModel.Position;
            Particle.Missile_Explosion.Play(pos);
            Audio.Bomb_Explode.PlayOneShot();

            enemy.TakeDamage(20);
        }
        
        public void ClearLandMines()
        {
            for (int i = 0; i < _landMines.Count; i++)
            {
                SubModel landMine = _landMines[i];
                landMine.OnDespawn();
            }
            _landMines.Clear();
        }

        #endregion

        public void Emit(int amount, Vector3 position, ParticleSystem.MinMaxGradient color, float radius)
        {
            psTransform.position = position + goreOffset;

            psMain.startColor = color;
            psShape.radius = radius;

            bloodPS.Emit(amount);
        }

        #region IO

        public void Deconstruct()
        {
            StopSpawning();
            foreach (var enemy in _enemies)
            {
                enemy.Deconstruct();
            }

            _enemies.Clear();
            
            ClearLandMines();
            
            airplane.OnDeconstruct();

            ResetSelf();
        }

        public void OnVictory()
        {
            Audio.Victory.Play();
            StopSpawning();
            Player.OnVictory();
        }

        public void OnFail()
        {
            Audio.Fail.Play();

            Announcer.THIS.Stop();
            StopSpawning();
            Player.OnFail();
        }

        #endregion

        public bool IsOutside(Transform transform)
        {
            return transform.position.z < EndLine;
        }
        
        public void CheckLandmine(Enemy enemy)
        {
            foreach (var landMine in _landMines)
            {
                if ((enemy.PositionXZ - landMine.Position.XZ()).sqrMagnitude < 0.75f)
                {
                    ExplodeLandMine(enemy, landMine);
                    break;
                }
            }
        }
    }
}