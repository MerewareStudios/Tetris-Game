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
            
            transform.Set(_data.gunType.GetTransformData());
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
                enemy._DamageTaken = _Data.damage;
            }
            bullet.Despawn();
        };
    }
    
    [Serializable]
    public enum StatType
    {
        Firerate,
        Splitshot,
        Damage
    }
    
    
    
    [System.Serializable]
    public class Data : ICloneable
    {
        [SerializeField] public Gun.Type gunType;
        [SerializeField] public float prevShoot = 0.0f;
        [SerializeField] public float fireRate = 0.5f;
        [SerializeField] public int split = 1;
        [SerializeField] public int damage = 1;

            
        public Data()
        {
            this.fireRate = 0.5f;
            this.split = 1;
            this.prevShoot = 0.0f;
            this.damage = 1;
        }
        public Data(Gun.Type gunType, float fireRate, int split, int damage)
        {
            this.gunType = gunType;
            this.fireRate = fireRate;
            this.split = split;
            this.damage = damage;
        }
        public Data(Data data)
        {
            this.gunType = data.gunType;
            this.fireRate = data.fireRate;
            this.prevShoot = data.prevShoot;
            this.split = data.split;
            this.damage = data.damage;
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

    
    [Serializable]
    public class UpgradeData : ICloneable
    {
        [SerializeField] public Gun.Type gunType;
        [SerializeField] public Sprite sprite;
        [SerializeField] public StageBar.StageData<float>[] stageData_Firerate;
        [SerializeField] public StageBar.StageData<int>[] stageData_Splitshot;
        [SerializeField] public StageBar.StageData<int>[] stageData_Damage;
            
        public UpgradeData()
        {
                
        }

        public bool IsFireRateFull(int atIndex)
        {
            return atIndex >= stageData_Firerate.Length - 1;
        }
        public bool IsSplitShotFull(int atIndex)
        {
            return atIndex >= stageData_Splitshot.Length - 1;
        }
        public bool IsDamageFull(int atIndex)
        {
            return atIndex >= stageData_Damage.Length - 1;
        }

        public bool IsAllFull(int fireRateIndex, int splitShotIndex, int damageIndex)
        {
            return IsFireRateFull(fireRateIndex) && IsSplitShotFull(splitShotIndex) && IsDamageFull(damageIndex);
        }
        public UpgradeData(UpgradeData gunUpgradeData)
        {
            stageData_Firerate = gunUpgradeData.stageData_Firerate as StageBar.StageData<float>[];
            stageData_Splitshot = gunUpgradeData.stageData_Splitshot as StageBar.StageData<int>[];
            stageData_Damage = gunUpgradeData.stageData_Damage as StageBar.StageData<int>[];
        }
        public object Clone()
        {
            return new UpgradeData(this);
        }
    }
        
    
    
}
