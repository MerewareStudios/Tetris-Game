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
        [System.NonSerialized] public Level.Data LevelData;
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

        public bool Spawning => _spawnRoutine != null;
        public bool HasEnemy => Enemies.Count > 0;
        public bool IsWarzoneCleared => !Spawning && !HasEnemy;
        
    #region Warzone
        void Awake()
        {
            this.Player.OnDeath += () =>
            {
                LevelManager.THIS.CheckFail();
            };
            
            psMain = bloodPS.main;
            psShape = bloodPS.shape;
            psTransform = bloodPS.transform;
        }
    #endregion

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
            this.Player._DamageTaken = -detakenDamage;
        } 
        public void EnemyKilled(Enemy enemy)
        {
            enemy.Kill();
        }
        
        public void Reset()
        {
            Player.Reset();
        }
        public void Begin()
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
                Countdown.THIS.Count((int)LevelData.spawnDelay);
                yield return new WaitForSeconds(LevelData.spawnDelay);

                int totalHealth = LevelData.totalEnemyHealth;
                
                while (totalHealth > 0)
                {
                    int health = 1;
                    while (Helper.Possible(LevelData.mergeProbability)) //check
                    {
                        if (health >= LevelData.maxMerge)
                        {
                            break;
                        }
                        health++;
                    }


                    Enemies.Add(SpawnEnemy(health, LevelData.enemySpeed));

                    totalHealth -= health;

                    yield return new WaitForSeconds(LevelData.spawnInterval.Random());
                }

                _spawnRoutine = null;
                LevelManager.THIS.CheckVictory();
            }
        }

        public void OnLevelLoad()
        {
            Player.Begin();
        }

        private void StopSpawning()
        {
            if (_spawnRoutine != null)
            {
                StopCoroutine(_spawnRoutine);
                _spawnRoutine = null;
            }
        }
        private Enemy SpawnEnemy(int health, float speed)
        {
            Enemy enemy = Pool.Enemy.Spawn<Enemy>(this.transform);
            enemy.Set(health, speed);
            enemy.OnSpawn(RandomSpawnPosition());
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
        private Vector3 RandomSpawnPosition()
        {
            return new Vector3(Random.Range(-LevelData.spawnWidth, LevelData.spawnWidth), 0.0f, Zone.startLine);
        }
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
    
        public void GiveHeart(int amount)
        {
            Player._HealthGained += amount;
        }
        public void GiveShield(int amount, float duration)
        {
            Player.shield.AddShield(amount, duration);
        }

    #endregion
    }
}

   

namespace  Level
{
    [System.Serializable]
    public class Data : ICloneable
    {
        [SerializeField] public float spawnDelay = 0.0f; // delay of the spawn
        [SerializeField] public int totalEnemyHealth = 100;
        [Range(1, 6)] [SerializeField] public int maxMerge = 2;
        [Range(0.0f, 0.9f)] [SerializeField] public float mergeProbability = 0.5f;
        [SerializeField] public float enemySpeed = 1.0f;
        [SerializeField] public float spawnWidth = 2.5f;
        [SerializeField] public Vector2 spawnInterval;
                
        public Data()
        {
            this.spawnDelay = 0.0f;
            this.totalEnemyHealth = 0;
            this.maxMerge = 1;
            this.enemySpeed = 1.0f;
            this.spawnWidth = 2.5f;
        }
        public Data(Data data)
        {
            this.spawnDelay = data.spawnDelay;
            this.totalEnemyHealth = data.totalEnemyHealth;
            this.maxMerge = data.maxMerge;
            this.enemySpeed = data.enemySpeed;
            this.spawnWidth = data.spawnWidth;
            this.spawnInterval = data.spawnInterval;
        }

        public object Clone()
        {
            return new Data(this);
        }
    } 
}