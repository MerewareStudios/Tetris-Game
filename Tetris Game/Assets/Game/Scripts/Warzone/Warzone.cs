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
        [Header("Level")]
        [System.NonSerialized] public Enemy.SpawnData EnemySpawnData;
        [Header("Players")]
        [SerializeField] public Player Player;
        [Header("Zones")]
        [SerializeField] public Area Zone;
        [SerializeField] public ParticleSystem bloodPS;
        [SerializeField] public Vector3 goreOffset;
        [System.NonSerialized] public ParticleSystem.MainModule psMain;
        [System.NonSerialized] public ParticleSystem.ShapeModule psShape;
        [System.NonSerialized] public Transform psTransform;
        //Routines
        [System.NonSerialized] private Coroutine _spawnRoutine = null;
        [System.NonSerialized] private List<Enemy> _enemies = new();
        [System.NonSerialized] private float _spawnRange = 0.5f;
        [System.NonSerialized] private const float SpawnRange = 1.6f;
        [System.NonSerialized] private const float SpawnMinOffset = 0.3f;
        [System.NonSerialized] private const float SpawnMaxOffset = 1.0f - SpawnMinOffset;

        public bool Spawning { get; set; }
        public bool HasEnemy => _enemies.Count > 0;
        public bool IsCleared => !Spawning && !HasEnemy;
        public int EnemyCount => _enemies.Count;
        public Enemy GetEnemy(int index) => _enemies[index];
        
    #region Warzone
        void Awake()
        {
            psMain = bloodPS.main;
            psShape = bloodPS.shape;
            psTransform = bloodPS.transform;
            
            UIManager.OnMenuModeChanged = state =>
            {
                if (state)
                {
                    this.Player.shield.Pause();
                }
                else
                {
                    if (Spawning)
                    {
                        this.Player.shield.Resume();
                    }
                }
            };
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
        public void EnemyKamikaze(Enemy enemy)
        {
            bool blocked = Player.shield.Remove();
            enemy.Kamikaze(blocked);
            
            if (blocked)
            {
                UIManagerExtensions.ShieldPs(enemy.thisTransform.position);
                return;
            }
            
            CameraManager.THIS.Shake();
            this.Player._CurrentHealth -= enemy.Damage;
            
            LevelManager.THIS.CheckEndLevel();
        } 
        public void EnemyKilled(Enemy enemy)
        {
            enemy.Kill();
        }
        
        public void ResetSelf()
        {
            Player.ResetSelf();
        }
        public void Begin(bool countdown = true)
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

                if (countdown)
                {
                    string startingText = string.Format(Onboarding.THIS.waveText, LevelManager.CurrentLevel);
                    yield return Announcer.THIS.Count(startingText, EnemySpawnData.spawnDelay);
                }
                


                List<Pool> enemyPool = new();
                int enemyIndex = 0;
                
                foreach (var countData in EnemySpawnData.countDatas)
                {
                    for (int i = 0; i < countData.count; i++)
                    {
                        enemyPool.Add(countData.enemyType);                        
                    }
                }
                
                enemyPool.Shuffle();

                _spawnRange = 0.0f;
                
                Player.StartSearching();

                this.enabled = true;


                while (enemyIndex < enemyPool.Count)
                {
                    _enemies.Add(SpawnEnemy(enemyPool[enemyIndex++]));
                    UIManager.THIS.LevelProgress = 1.0f - ((float)enemyIndex / enemyPool.Count);
                    if (!Player.CurrentEnemy)
                    {
                        AssignClosestEnemy();
                    }

                    float stamp = Time.time;
                    while(HasEnemy && Time.time - stamp < EnemySpawnData.spawnInterval)
                    {
                        yield return new WaitForSeconds(1.0f);
                    }
                }

                Spawning = false;
                _spawnRoutine = null;
            }
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
        private Enemy SpawnEnemy(Pool pool)
        {
            Enemy enemy = pool.Spawn<Enemy>(this.transform);
            enemy.OnSpawn(NextSpawnPosition());
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
            Player.CurrentEnemy = _enemies
                .OrderBy(enemy => Mathf.Abs(enemy.transform.position.z - Zone.endLine))
                .FirstOrDefault();
        }
        private Vector3 NextSpawnPosition()
        {
            _spawnRange = Mathf.Repeat(_spawnRange + Random.Range(SpawnMinOffset, SpawnMaxOffset), 1.0f);
            return new Vector3(Mathf.Lerp(-SpawnRange, SpawnRange, _spawnRange), 0.0f, Zone.startLine);
        }

        #endregion

        public void Emit(int amount, Vector3 position, Color color, float radius)
        {
            psTransform.position = position + goreOffset;

            psMain.startColor = (Color)color;
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

        [System.Serializable]
        public class Area
        {
            [SerializeField] public float startLine;
            [SerializeField] public float endLine;

            public bool IsOutside(Transform transform)
            {
                return transform.position.z < endLine;
            }
        }
    }
}

   

namespace  Level
{
    
}