using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game.UI;
using Internal.Core;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

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
        [System.NonSerialized] public readonly List<Enemy> Enemies = new();
        [System.NonSerialized] private bool _busy = false;

        public bool Spawning { get; private set; }
        public bool HasEnemy => Enemies.Count > 0;
        public bool IsWarzoneCleared => !Spawning && !HasEnemy;
        
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
                if (state)
                {
                    this.Player.shield.PauseProtection();
                }
                else
                {
                    if (Spawning)
                    {
                        this.Player.shield.ResumeProtection();
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

            int detakenDamage = Player.shield.ConsumeShield(enemy._Health);
            if (detakenDamage >= 0)
            {
                Particle.Shield.Play(enemy.transform.position);
                return;
            }
            CameraManager.THIS.Shake();

            this.Player._CurrentHealth += detakenDamage;
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
            
            Player.StartSearching();

            IEnumerator SpawnRoutine()
            {
                if (countdown)
                {
                    string startingText = string.Format(Onboarding.THIS.waveText, LevelManager.CurrentLevel);
                    yield return Announcer.THIS.Count(startingText, EnemySpawnData.spawnDelay);
                }
                

                // int totalHealth = EnemySpawnData.totalEnemyHealth;

                Spawning = true;
                this.Player.shield.ResumeProtection();
                
                while (true)
                {
                    Enemies.Add(SpawnEnemy());
                    yield return new WaitForSeconds(EnemySpawnData.spawnInterval);
                }

                _spawnRoutine = null;
                
                Spawning = false;
                
                LevelManager.THIS.CheckVictory();
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

            Spawning = false;
        }
        private Enemy SpawnEnemy()
        {
            Enemy enemy = Pool.Enemy_1.Spawn<Enemy>(this.transform);
            enemy.Replenish();
            enemy.OnSpawn(RandomSpawnPosition);
            return enemy;
        }

        public Enemy DequeEnemy()
        {
            if (Enemies.Count == 0)
            {
                return null;
            }

            Enemy enemy = Enemies[0];
            Enemies.RemoveAt(0);
            return enemy;
        }
        public void RemoveEnemy(Enemy enemy)
        {
            Enemies.Remove(enemy);
            LevelManager.THIS.CheckVictory();
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
        
    #region  Upgrades
    
        public void GiveHeart(int amount = 1)
        {
            Player._CurrentHealth += amount;
        }
        public void GiveShield(int amount)
        {
            Player.shield.AddShield(amount);
        }

    #endregion
    }
}

   

namespace  Level
{
    
}