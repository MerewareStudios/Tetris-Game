using System.Collections;
using System.Collections.Generic;
using Internal.Core;
using UnityEngine;
using UnityEngine.Serialization;

public class FireArea : Singleton<FireArea>
{
    [SerializeField] private Turret _turret;
    [SerializeField] private Transform spawnPosition;
    [SerializeField] private float spawnRange = 2.5f;
    [SerializeField] private float speed = 0.2f;
    [SerializeField] private float spawnInterval = 2.0f;

    public void Shoot()
    {
        _turret.Shoot();
    }

    IEnumerator Start()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            Enemy enemy = Pool.Enemy.Spawn<Enemy>(this.transform);
            enemy.transform.position = new Vector3(Random.Range(-spawnRange, spawnRange), spawnPosition.position.y, spawnPosition.position.z);
            enemy.transform.forward = Vector3.back;
            _turret.AddTarget(enemy);
        }    
    }
}
