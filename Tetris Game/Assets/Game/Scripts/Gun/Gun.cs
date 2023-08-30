using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game;
using Internal.Core;
using UnityEngine;
using UnityEngine.Serialization;

public class Gun : MonoBehaviour
{
    [SerializeField] public Transform muzzle; 
    [SerializeField] public Game.GunSo GunSo; 
    [System.NonSerialized] private Data _data;

    
    public Data _Data
    {
        set
        {
            _data = value;
            
            transform.Set(GunSo.holsterTransformData);
            
            SetStat(StatDisplay.Type.Damage, _data.damage);
            SetStat(StatDisplay.Type.Splitshot, _data.split);
            SetStat(StatDisplay.Type.Firerate, _data.FireRate);
        }
        get => _data;
    }

    private void SetStat(StatDisplay.Type statType, int value)
    {
        if (value <= 1)
        {
            StatDisplayArranger.THIS.Hide(statType);
        }
        else
        {
            StatDisplayArranger.THIS.Show(statType, value);
        }
    }
    
    public void Bubble()
    {
        Particle.Bubble.EmitForward(1, muzzle.position, muzzle.forward);
    }
    
    public void Shoot(Enemy enemy)
    {
        Transform enemyTransform = enemy.transform;
        Vector3 targetPosition = enemyTransform.position;

        enemy.OnRemoved = () =>
        {
            enemyTransform = null;
        };
            
        Transform bullet = Pool.Bullet.Spawn().transform;
        
        TrailRenderer trail = Pool.Trail.Spawn<TrailRenderer>(bullet);
        
        bullet.DOKill();
        bullet.transform.position = muzzle.position;
        
        trail.Clear();
        
        Tween bulletTween = bullet.DOJump(enemyTransform.position, 2.25f, 1, 0.45f).SetEase(Ease.Linear);
        bulletTween.onUpdate = () =>
        {
            if (enemyTransform)
            {
                targetPosition = enemyTransform.position;
            }
            bulletTween.SetTarget(targetPosition);
        };
        bulletTween.onComplete = () =>
        {
            if (enemyTransform)
            {
                enemy.TakeDamage(_Data.damage);
            }
            bullet.Despawn();
            trail.gameObject.Despawn();
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
        [SerializeField] public Pool gunType;
        [SerializeField] public float prevShoot = 0.0f;
        [SerializeField] private int fireRate = 1;
        [SerializeField] public int split = 1;
        [SerializeField] public int damage = 1;

        public float FireInterval
        {
            get;
            set;
        }

        public int FireRate
        {
            set
            {
                this.fireRate = value;
                FireInterval = 1.0f - (value - 1) * 0.05f;
            }
            get => this.fireRate;
        }
            
        public Data()
        {
            this.fireRate = 1;
            this.split = 1;
            this.prevShoot = 0.0f;
            this.damage = 1;
        }
        public Data(Pool gunType, int fireRate, int split, int damage)
        {
            this.gunType = gunType;
            this.FireRate = fireRate;
            this.split = split;
            this.damage = damage;
        }
        public Data(Data data)
        {
            this.gunType = data.gunType;
            this.FireRate = data.fireRate;
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
    public class UpgradeData
    {
        [SerializeField] public Const.Currency currency;
        [SerializeField] public Pool gunType;
        [SerializeField] public Sprite sprite;
        [SerializeField] public StageBar.StageData<int>[] stageData_Damage;
        [SerializeField] public StageBar.StageData<int>[] stageData_Firerate;
        [SerializeField] public StageBar.StageData<int>[] stageData_Splitshot;
        [System.NonSerialized] public List<StageBar.StageData<int>[]> stageDatas = new();
            
        public StageBar.StageData<int> GetStageData(Gun.StatType statType, int atLevel)
        {
            int index = (int)statType;
            return stageDatas[index][atLevel];
        }
        public int Value(Gun.StatType statType, int atLevel)
        {
            int index = (int)statType;
            return stageDatas[index][atLevel].value;
        }
        public int Price(Gun.StatType statType, int atLevel)
        {
            int index = (int)statType;
            return stageDatas[index][atLevel].currency.amount;
        }
        public bool IsFull(Gun.StatType statType, int atIndex)
        {
            return IsFull((int)statType, atIndex);
        }
        public bool IsFull(int statIndex, int atIndex)
        {
            return atIndex >= stageDatas[statIndex].Length - 1;
        }

        public bool IsAllFull(params int[] indexes)
        {
            int statCount = System.Enum.GetValues(typeof(Gun.StatType)).Length;
            for (int i = 0; i < statCount; i++)
            {
                if (!IsFull(i, indexes[i]))
                {
                    return false;
                }
            }
            return true;
        }
        public void Init()
        {
            // stageDatas.Clear();
            stageDatas.Add(stageData_Damage);  
            stageDatas.Add(stageData_Firerate);
            stageDatas.Add(stageData_Splitshot);
        }
    }
}