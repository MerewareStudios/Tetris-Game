using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
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
        [System.NonSerialized] public ParticleSystem.MainModule psMain;
        [System.NonSerialized] public ParticleSystem.ShapeModule psShape;
        [System.NonSerialized] public Transform psTransform;
        //Routines
        [System.NonSerialized] private Coroutine _spawnRoutine = null;
        [System.NonSerialized] private readonly List<Enemy> Enemies = new();
        [System.NonSerialized] private bool _busy = false;

        public bool Spawning => _spawnRoutine != null;
        public bool HasEnemy => Enemies.Count > 0;
        public bool IsWarzoneCleared => !Spawning && !HasEnemy;
        public int EnemyCount => Enemies.Count;
        
    #region Warzone
        void Awake()
        {
            this.Player.OnDeath += () =>
            {
                LevelManager.THIS.OnFail();
            };
            
            psMain = bloodPS.main;
            psShape = bloodPS.shape;
            psTransform = bloodPS.transform;
            
            UIManager.OnMenuModeChanged = state =>
            {
                Debug.Log("OnMenuModeChanged " + state);
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
        foreach (var enemy in Enemies)
        {
            enemy.Walk();
        }
    }

    #region Warzone
        public void EnemyKamikaze(Enemy enemy)
        {
            enemy.Kamikaze();

            bool blocked = Player.shield.Remove();
            
            if (blocked)
            {
                Particle.Shield.Play(enemy.transform.position);
                return;
            }
            
            CameraManager.THIS.Shake();
            this.Player._CurrentHealth -= enemy.Damage;
        } 
        public void EnemyKilled(Enemy enemy)
        {
            enemy.Kill();
        }
        
        public void Reset()
        {
            Player.Reset();
        }
        public void Begin(bool countdown = true)
        {
            if (_busy)
            {
                return;
            }

            _busy = true;
            
            StopSpawning();
            _spawnRoutine = StartCoroutine(SpawnRoutine());
            

            IEnumerator SpawnRoutine()
            {
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
                
                Player.StartSearching();


                while (enemyIndex < enemyPool.Count)
                {
                    Enemies.Add(SpawnEnemy(enemyPool[enemyIndex++]));
                    if (!Player.CurrentEnemy)
                    {
                        AssignClosestEnemy();
                    }
                    yield return new WaitForSeconds(EnemySpawnData.spawnInterval);
                }

                _spawnRoutine = null;
                
                
                // LevelManager.THIS.CheckVictory();
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
        }
        private Enemy SpawnEnemy(Pool pool)
        {
            Enemy enemy = pool.Spawn<Enemy>(this.transform);
            enemy.OnSpawn(RandomSpawnPosition);
            return enemy;
        }
        public void RemoveEnemy(Enemy enemy)
        {
            Enemies.Remove(enemy);
            AssignClosestEnemy();
        }

        public void AssignClosestEnemy()
        {
            Player.CurrentEnemy = Enemies
                .OrderBy(enemy => Mathf.Abs(enemy.transform.position.z - Zone.endLine))
                .FirstOrDefault();
        }
        private Vector3 RandomSpawnPosition => new Vector3(Random.Range(-2.5f, 2.5f), 0.0f, Zone.startLine);
            
    #endregion

        public void Emit(int amount, Vector3 position, Color color, float radius)
        {
            psTransform.position = position;

            psMain.startColor = (Color)color;
            psShape.radius = radius;
        
            bloodPS.Emit(amount);
        }
    
    #region IO

        public void Deconstruct()
        {
            StopSpawning();
            Player.Deconstruct();
            foreach (var enemy in Enemies)
            {
                enemy.Deconstruct();

            }
            Enemies.Clear();
            _busy = false;
        }

        public void OnVictory()
        {
            StopSpawning();
            Player.OnVictory();
            foreach (var enemy in Enemies)
            {
                enemy.Deconstruct();
            }
            Enemies.Clear();
            _busy = false;
        }
        
        public void OnFail()
        {
            StopSpawning();
            Player.OnFail();
            
            _busy = false;
            
            int enemyCount = Enemies.Count;

            if (enemyCount <= 0)
            {
                return;
            }
            
            float delayIncrease = 1.0f / enemyCount;
            float delay = 0.0f;
            foreach (var enemy in Enemies)
            {
                enemy.enabled = false;
                DOVirtual.DelayedCall(delay, enemy.KamikazeDeconstruct);
                delay += delayIncrease;
            }
            Enemies.Clear();
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