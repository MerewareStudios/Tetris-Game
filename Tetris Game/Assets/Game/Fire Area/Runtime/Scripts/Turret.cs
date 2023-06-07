using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private float turnRate = 6.0f;
    [System.NonSerialized] private int ammo = 0;
    [System.NonSerialized] public static int SHOOT_HASH = Animator.StringToHash("Shoot");
    [System.NonSerialized] public Vector2 thisPosition;
    [System.NonSerialized] private float currentAngle = 0.0f;
    [System.NonSerialized] private Enemy currentEnemy;
    [System.NonSerialized] private Transform currentEnemyTransform;
    [System.NonSerialized] private Queue<Enemy> enemyQueue = new();

    void Start()
    {
        var position = transform.position;
        thisPosition = new Vector2(position.x, position.z);
    }

    public void AddAmmo(int amount)
    {
        ammo += amount;
    }

    public void SetTarget(Enemy enemy)
    {
        currentEnemy = enemy;
        currentEnemyTransform = enemy.transform;
    }
    public void AddTarget(Enemy target)
    {
        if (currentEnemy == null)
        {
            if (enemyQueue.TryDequeue(out currentEnemy))
            {
                return;
            }
            SetTarget(target);
            return;
        }

        enemyQueue.Enqueue(target);
    }
    public void NextTarget()
    {
        if (enemyQueue.TryDequeue(out Enemy enemy))
        {
            SetTarget(enemy);
            return;
        }
        currentEnemy = null;
        currentEnemyTransform = null;
    }

    public void Shoot()
    {
        if (!currentEnemy)
        {
            return;
        }
        _animator.SetTrigger(SHOOT_HASH);
        currentEnemy.Kill();
        NextTarget();
    }

    private void Aim()
    {
        if (!currentEnemy)
        {
            return;
        }
        var targetPosition = currentEnemyTransform.position;
        Vector2 direction = new Vector2(targetPosition.x, targetPosition.z) - thisPosition;
        float angle = -Vector2.SignedAngle(Vector2.up, direction);
        currentAngle = Mathf.Lerp(currentAngle, angle, Time.deltaTime * turnRate);

        transform.eulerAngles = new Vector3(0.0f, currentAngle, 0.0f);
    }
    
    void Update()
    {
        Aim();
    }
}
