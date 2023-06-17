using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game;
using UnityEngine;
using UnityEngine.Serialization;

public class Gun : MonoBehaviour
{
    [SerializeField] public Transform muzzle; 
    [System.NonSerialized] private Data _data;

    
    public Data _Data
    {
        set
        {
            _data = value;
            
            transform.Set(_data.type.GetTransformData());
        }
        get => _data;
    }
    
    public void Shoot(Enemy enemy)
    {
        Transform enemyTransform = enemy.transform;
        Vector3 targetPosition = enemyTransform.position;

        enemy.OnRemoved += () =>
        {
            enemyTransform = null;
        };
            
        Transform bullet = Pool.Bullet.Spawn().transform;
        bullet.DOKill();
        bullet.transform.position = muzzle.position;
        Tween bulletTween = bullet.DOJump(enemyTransform.position, 2.25f, 1, 0.45f).SetEase(Ease.Linear);
        bulletTween.onUpdate += () =>
        {
            if (enemyTransform)
            {
                targetPosition = enemyTransform.position;
            }
            bulletTween.SetTarget(targetPosition);
        };
        bulletTween.onComplete += () =>
        {
            if (enemyTransform)
            {
                enemy._DamageTaken = 1;
            }
            bullet.Despawn();
        };
    }
    
    [System.Serializable]
    public class Data : ICloneable
    {
        [SerializeField] public Type type;
        [SerializeField] public float shootInterval = 0.5f;
        [SerializeField] public float prevShoot = 0.0f;
        [SerializeField] public int split = 1;

            
        public Data()
        {
            this.shootInterval = 0.5f;
            this.split = 1;
            this.prevShoot = 0.0f;
        }
        public Data(Data data)
        {
            this.type = data.type;
            this.shootInterval = data.shootInterval;
            this.prevShoot = data.prevShoot;
            this.split = data.split;
        }

        public object Clone()
        {
            return new Data(this);
        }
    }

    [Serializable]
    public enum Type
    {
        GUN1,
        GUN2,
        GUN3,
        GUN4,
        GUN5,
    }
}
