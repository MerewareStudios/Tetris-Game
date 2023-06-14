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
        [System.NonSerialized] public LevelSo LevelData;
        [Header("Players")]
        [SerializeField] public Player Player;
        [Header("Zones")]
        [SerializeField] public Area Zone;
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
        }
    #endregion

    #region Warzone
        public void EnemyKamikaze(Enemy enemy)
        {
            enemy.Kamikaze();
            this.Player._DamageTaken = enemy.Damage;
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
                float time = 0.0f;
                
                yield return new WaitForSeconds(LevelData.spawnDelay);
                
                while (true)
                {
                    float step = time / LevelData.totalDuration;
                    
                    
                    Enemies.Add(SpawnEnemy());

                    time += Time.deltaTime;
                    yield return null;
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
        private Enemy SpawnEnemy()
        {
            Enemy enemy = Pool.Enemy.Spawn<Enemy>(this.transform);
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

// namespace  Level
// {
//     [System.Serializable]
//     public class Data : ICloneable
//     {
//         
//
//                 
//         public Data()
//         {
//             this.spawnDelay = 0.0f;
//             this.totalHealth = 0;
//             this.maxMerge = 1;
//             this.speed = 1.0f;
//             this.spawnWidth = 2.5f;
//         }
//         public Data(Data data)
//         {
//             this.spawnDelay = data.spawnDelay;
//             this.totalHealth = data.totalHealth;
//             this.maxMerge = data.maxMerge;
//             this.speed = data.speed;
//             // this.spawnInterval = data.spawnInterval;
//             // this.time = data.time;
//         }
//
//         public object Clone()
//         {
//             return new Data(this);
//         }
//     } 
// }