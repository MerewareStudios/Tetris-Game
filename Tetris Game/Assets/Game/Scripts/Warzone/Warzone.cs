using System.Collections;
using System.Collections.Generic;
using Internal.Core;
using UnityEngine;

namespace  Game
{
    public class Warzone : Singleton<Warzone>
    {
        [Header("Players")]
        [SerializeField] public Player Player;
        [Header("Zones")]
        [SerializeField] public Area Zone;
        [Header("Spawn")]
        [SerializeField] private float spawnWidth = 2.5f;
        [SerializeField] private float spawnInterval = 2.0f;
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

        public void PlayerAttack(int bulletCount)
        {
            this.Player.Shoot(bulletCount);
        }
        public void EnemyKamikaze(Enemy enemy)
        {
            enemy.Kamikaze();
            this.Player._DamageTaken = enemy.Damage;
        } 
        public void EnemyKilled(Enemy enemy)
        {
            enemy.Kill();
        }
        public void StartSpawning()
        {
            StopSpawning();
            spawnRoutine = StartCoroutine(SpawnRoutine());
            
            IEnumerator SpawnRoutine()
            {
                while (true)
                {
                    yield return new WaitForSeconds(spawnInterval);
                    Enemies.Add(SpawnEnemy());
                }
            }
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
            return new Vector3(Random.Range(-spawnWidth, spawnWidth), 0.0f, Zone.startLine);
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