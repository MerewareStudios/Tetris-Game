using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class FireArea : Internal.Core.Singleton<FireArea>
{
    [SerializeField] public Turret _turret;
    [SerializeField] private Transform spawnPosition;
    [SerializeField] public Transform protectionLine;
    [SerializeField] private float spawnWidth = 2.5f;
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
            Vector3 spawnPos = new Vector3(Random.Range(-spawnWidth, spawnWidth), spawnPosition.position.y, spawnPosition.position.z);
            enemy.OnSpawn(spawnPos, Vector3.back);
            _turret.AddTarget(enemy);
        }    
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        var position = spawnPosition.position;
        Gizmos.DrawLine(position + Vector3.left * spawnWidth, position + Vector3.right * spawnWidth);
    }
}
