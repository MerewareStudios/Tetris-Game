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

            _spawnRoutine = StartCoroutine(SpawnRoutine());
            


            IEnumerator SpawnRoutine()
            {
                Spawning = true;

                UIManager.THIS.LevelProgress = 1.0f;

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

                enemyPool.Shuffle();

                _spawnRangeNorm = 0.0f;


                this.enabled = true;

                
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
            UIManager.THIS.LevelProgress = _leftEnemyHealth / (float) _totalEnemyHealth;
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

        private Enemy SpawnEnemy(EnemyData enemyData)
        {
            Enemy enemy = enemyData.type.Spawn<Enemy>(this.transform);
            enemy.so = enemyData;
            enemy.OnSpawn(enemy.so.speed == 0.0f ? MidSpawnPosition(0.4f) : NextSpawnPosition(), GetNewEnemyID());
            enemy.Replenish();

            _leftEnemyHealth -= enemyData.maxHealth;
            UpdateProgress();
            
            return enemy;
        }

        public void RemoveEnemy(Enemy enemy)
        {
            _enemies.Remove(enemy);
            // enemy.OnRemoved?.Invoke();
            AssignClosestEnemy();
        }

        public void AssignClosestEnemy()
        {
            //Sort enemies by distance
            _enemies = _enemies.OrderBy(enemy => enemy.PositionZ - EndLine).ToList();
            //Assign first enemy as current target
            Player.CurrentEnemy = _enemies.FirstOrDefault();
                // .OrderBy(enemy => Mathf.Abs(enemy.transform.position.z - EndLine))
                
        }

        private Vector3 NextSpawnPosition()
        {
            _spawnRangeNorm = Mathf.Repeat(_spawnRangeNorm + Random.Range(SpawnMinOffset, SpawnMaxOffset), 1.0f);
            return new Vector3(Mathf.Lerp(-SpawnRange, SpawnRange, _spawnRangeNorm), 0.0f, StartLine);
        }
        private Vector3 MidSpawnPosition(float factor)
        {
            return new Vector3(Random.Range(-SpawnRange, SpawnRange), 0.0f, Mathf.Lerp(StartLine, EndLine, factor));
        }

        public Vector3 GetMissileTarget()
        {
            if (_enemies.Count > 0)
            {
                return _enemies[0].hitTarget.position;
            }

            return MidSpawnPosition(0.5f);
        }
        
        public Enemy GetProjectileTarget() => _enemies.Count > 0 ? _enemies[0] : null;
        
        public Vector3 GetLandMineTarget()
        {
            return new Vector3(Random.Range(-SpawnRange, SpawnRange), 0.0f, EndLine);
        }

        public void AEODamage(Vector3 position, int damage, float sqrMagnitude)
        {
            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                float sqrDistance = (position - _enemies[i].thisTransform.position).sqrMagnitude;
                if (sqrDistance < sqrMagnitude)
                {
                    _enemies[i].TakeDamage(damage, 2.0f);
                }
            }
        }

        public void AddLandMine(SubModel subModel)
        {
            this.landMines.Add(subModel);
            
        }
        
        public void ExplodeLandMine(SubModel subModel)
        {
            this.landMines.Remove(subModel);
            subModel.Despawn();
            Vector3 pos = subModel.Position;
            AEODamage(pos, 5, 1.0f);
            Particle.Missile_Explosion.Play(pos);
        }
        
        public void ClearLandMines()
        {
            for (int i = landMines.Count - 1; i >= 0; i--)
            {
                landMines[i].Despawn();
                landMines.RemoveAt(i);
            }
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
            StopSpawning();
            Player.OnFail();
        }

        #endregion

        public bool IsOutside(Transform transform)
        {
            return transform.position.z < EndLine;
        }
        
        public void CheckLandmine(Transform transform)
        {
            foreach (var landMine in landMines)
            {
                if (Vector3.SqrMagnitude(transform.position - landMine.Position) < 0.4f)
                {
                    ExplodeLandMine(landMine);
                    break;
                }
            }
        }
    }
}