using System;
using System.Collections;
using System.Collections.Generic;
using Internal.Core;
using UnityEngine;
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
        [System.NonSerialized] private Coroutine spawnRoutine = null;
        [System.NonSerialized] public readonly List<Enemy> Enemies = new();

        
        
    #region Warzone
        void Awake()
        {
            this.Player.OnDeath += () =>
            {
                LevelManager.THIS.GameOver();
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
            this.Player._DamageTaken = enemy._Health;
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
            StopSpawning();
            spawnRoutine = StartCoroutine(SpawnRoutine());
            
            IEnumerator SpawnRoutine()
            {
                
                yield return new WaitForSeconds(LevelData.spawnDelay);

                int totalHealth = LevelData.totalHealth;
                
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
            }
            
            Player.Begin();
        }

        private void StopSpawning()
        {
            if (spawnRoutine != null)
            {
                StopCoroutine(spawnRoutine);
                spawnRoutine = null;
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
    [System.Serializable]
    public class Data : ICloneable
    {
        [SerializeField] public float spawnDelay = 0.0f; // delay of the spawn
        [SerializeField] public int totalHealth = 100;
        [Range(1, 5)] [SerializeField] public int maxMerge = 2;
        [Range(0.0f, 0.9f)] [SerializeField] public float mergeProbability = 0.5f;
        [SerializeField] public float enemySpeed = 1.0f;
        [SerializeField] public float spawnWidth = 2.5f;
        [SerializeField] public Vector2 spawnInterval;
                
        public Data()
        {
            this.spawnDelay = 0.0f;
            this.totalHealth = 0;
            this.maxMerge = 1;
            this.enemySpeed = 1.0f;
            this.spawnWidth = 2.5f;
        }
        public Data(Data data)
        {
            this.spawnDelay = data.spawnDelay;
            this.totalHealth = data.totalHealth;
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