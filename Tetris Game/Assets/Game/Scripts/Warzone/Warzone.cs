using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.UI;
using Internal.Core;
using UnityEngine;

namespace  Game
{
    public class Warzone : Singleton<Warzone>
    {
        [Header("Level")] [System.NonSerialized]
        public Enemy.SpawnData EnemySpawnData;

        [Header("Players")] [SerializeField] public Player Player;
        [Header("Zones")] [SerializeField] public ParticleSystem bloodPS;
        [SerializeField] public Vector3 goreOffset;
        [System.NonSerialized] public ParticleSystem.MainModule psMain;
        [System.NonSerialized] public ParticleSystem.ShapeModule psShape;
        [System.NonSerialized] public Transform psTransform;
        [System.NonSerialized] public float StartLine;
        [System.NonSerialized] public float EndLine;
        [System.NonSerialized] public float SpawnRange;

        //Routines
        [System.NonSerialized] private Coroutine _spawnRoutine = null;
        [System.NonSerialized] private List<Enemy> _enemies = new();
        [System.NonSerialized] private float _spawnRangeNorm = 0.5f;
        [System.NonSerialized] private int _enemyID = 0;
        [System.NonSerialized] private const float SpawnMinOffset = 0.3f;
        [System.NonSerialized] private const float SpawnMaxOffset = 1.0f - SpawnMinOffset;

        [System.NonSerialized] private List<SubModel> landMines = new List<SubModel>();
        

        public bool Spawning { get; set; }
        public bool HasEnemy => _enemies.Count > 0;
        public bool IsCleared => !Spawning && !HasEnemy;
        public int EnemyCount => _enemies.Count;
        private int _totalEnemyHealth;
        private int _leftEnemyHealth;
        public Enemy GetEnemy(int index) => _enemies[index];
        public int GetNewEnemyID() => ++_enemyID;

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
                _enemies[i].Walk();
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
            
            
            this.enabled = true;


            IEnumerator SpawnRoutine()
            {
                Spawning = true;

                List<EnemyData> enemyPool = new();
                int enemyIndex = 0;

                _totalEnemyHealth = 0;

                foreach (var countData in EnemySpawnData.countDatas)
                {
                    for (int i = 0; i < countData.count; i++)
                    {
                        enemyPool.Add(countData.enemyData);
                        _totalEnemyHealth += countData.enemyData.maxHealth;
                    }
                }
                _leftEnemyHealth = _totalEnemyHealth;

                UpdateProgress();

                yield return new WaitForSeconds(0.25f);
                
                
                Player.StartSearching();
                
                if (EnemySpawnData.spawnDelay > 0)
                {
                    string startingText = string.Format(Onboarding.THIS.waveText, LevelManager.CurrentLevel);
                    yield return Announcer.THIS.Count(startingText, EnemySpawnData.spawnDelay);
                }
                else
                {
                    string startingText = Onboarding.THIS.targetPracticeText;
                    yield return Announcer.THIS.Show(startingText, 0.5f);
                }



               

                
                UpdateProgress();

                enemyPool.Shuffle();

                _spawnRangeNorm = 0.0f;



                
                while (enemyIndex < enemyPool.Count)
                {
                    EnemyData enemyData = enemyPool[enemyIndex++];
                    if (enemyData.extraWait > 0.0f)
                    {
                        yield return new WaitForSeconds(enemyData.extraWait);
                    }
                    _enemies.Add(SpawnEnemy(enemyData));
                    if (!Player.CurrentEnemy)
                    {
                        AssignClosestEnemy();
                    }

                    if (enemyIndex >= enemyPool.Count)
                    {
                        break;
                    }

                    float stamp = Time.time;
                    while (HasEnemy && Time.time - stamp < EnemySpawnData.spawnInterval)
                    {
                        yield return new WaitForSeconds(1.0f);
                    }
                }

                Spawning = false;
                _spawnRoutine = null;
            }
        }

        private void UpdateProgress()
        {
            UIManager.THIS.SetLevelProgress(_leftEnemyHealth / (float) _totalEnemyHealth, _leftEnemyHealth);
        }

        public void OnLevelLoad()
        {
            Player.Replenish();
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

        public Enemy SpawnEnemy(EnemyData enemyData)
        {
            Enemy enemy = enemyData.type.Spawn<Enemy>(this.transform);
            enemy.so = enemyData;
            enemy.OnSpawn(NextSpawnPosition(enemy.so.RandomForwardRange()), GetNewEnemyID());
            enemy.Replenish();

            _leftEnemyHealth -= enemyData.maxHealth;
            UpdateProgress();
            
            return enemy;
        }
        public Enemy CustomSpawnEnemy(EnemyData enemyData, Vector3 position)
        {
            Enemy enemy = enemyData.type.Spawn<Enemy>(this.transform);
            enemy.so = enemyData;
            enemy.OnSpawn(position, GetNewEnemyID());
            enemy.Replenish();
            
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
            //Assign first enemy as current target
            Player.CurrentEnemy = _enemies.FirstOrDefault();
        }

        public Vector3 NextSpawnPosition(float forwardPercent)
        {
            _spawnRangeNorm = Mathf.Repeat(_spawnRangeNorm + Random.Range(SpawnMinOffset, SpawnMaxOffset), 1.0f);
            return new Vector3(Mathf.Lerp(-SpawnRange, SpawnRange, _spawnRangeNorm), 0.0f, Mathf.Lerp(EndLine, StartLine, forwardPercent));
        }
        
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
        
        public Enemy GetProjectileTarget(Vector3 requestPosition)
        {
            if (_enemies.Count == 0)
            {
                return null;
            }

            List<Enemy> availableEnemies = new List<Enemy>();
            
            for (int i = 0; i < _enemies.Count; i++)
            {
                if (_enemies[i].DragTarget)
                {
                    continue;
                }

                availableEnemies.Add(_enemies[i]);
            }

            if (availableEnemies.Count == 0)
            {
                return _enemies[0];
            }
            
            return availableEnemies.OrderBy(enemy => (enemy.PositionXZ - requestPosition.XZ()).sqrMagnitude).FirstOrDefault();
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

        public void AddLandMine(SubModel subModel)
        {
            this.landMines.Add(subModel);
        }
        
        public void ExplodeLandMine(Enemy enemy, SubModel subModel)
        {
            subModel.OnDespawn();
            landMines.Remove(subModel);
            Vector3 pos = subModel.Position;
            Particle.Missile_Explosion.Play(pos);
            enemy.TakeDamage(20);
        }
        
        public void ClearLandMines()
        {
            for (int i = 0; i < landMines.Count; i++)
            {
                SubModel landMine = landMines[i];
                landMine.OnDespawn();
            }
            landMines.Clear();
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

            ResetSelf();
        }

        public void OnVictory()
        {
            StopSpawning();
            Player.OnVictory();
        }

        public void OnFail()
        {
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
            foreach (var landMine in landMines)
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