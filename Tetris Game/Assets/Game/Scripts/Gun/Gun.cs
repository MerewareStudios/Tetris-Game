using System;
using System.Text;
using DG.Tweening;
using Game;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] public Transform muzzle; 
    [SerializeField] public Game.GunSo GunSo; 
    [System.NonSerialized] private Data _data;
    [System.NonSerialized] private Tween _boostTween = null;

    
    public Data _Data
    {
        set
        {
            _data = value;
            
            transform.Set(GunSo.holsterTransformData);

            _data.Mult = 1;
            
            UpdateAllStats();
        }
        get => _data;
    }

    public void Boost(int mult)
    {
        this._Data.Mult = mult;
        
        UpdateAllStats(true);
        
        _boostTween?.Kill();
        float percent = 0.0f;
        _boostTween = DOTween.To(x => percent = x, 1.0f, 0.0f, 6.0f).SetEase(Ease.Linear);
        _boostTween.onUpdate = () =>
        {
            StatDisplayArranger.THIS.UpdatePercent(StatDisplay.Type.Damage, percent);
            StatDisplayArranger.THIS.UpdatePercent(StatDisplay.Type.Splitshot, percent);
            StatDisplayArranger.THIS.UpdatePercent(StatDisplay.Type.Firerate, percent);
        };
        _boostTween.onComplete = () =>
        {
            _Data.Mult = 1;
            UpdateAllStats();
        };
    }

    private void UpdateAllStats(bool markSpecial = false)
    {
        SetStat(StatDisplay.Type.Damage, _data.DamageAmount, markSpecial);
        SetStat(StatDisplay.Type.Splitshot, _data.SplitAmount, markSpecial);
        SetStat(StatDisplay.Type.Firerate, _data.RateAmount, markSpecial);
    }

    private void SetStat(StatDisplay.Type statType, int value, bool markSpecial = false)
    {
        if (value <= 1)
        {
            StatDisplayArranger.THIS.HideImmediate(statType);
        }
        else
        {
            StatDisplayArranger.THIS.UpdateAmount(statType, value, 0.5f, markSpecial);
        }
    }
    
    public void Bubble()
    {
        Particle.Bubble.EmitForward(1, muzzle.position, muzzle.forward);
    }
    
    public void Shoot(Enemy enemy)
    {
        Transform enemyTransform = enemy.transform;
        
        // Vector3 targetPosition = enemyTransform.position;


        // enemy.OnRemoved = () =>
        // {
        //     enemyTransform = null;
        // };
            
        Transform bullet = Pool.Bullet.Spawn().transform;
        
        TrailRenderer trail = Pool.Trail.Spawn<TrailRenderer>(bullet);
        
        bullet.DOKill();

        bullet.transform.position = muzzle.position;
        
        trail.Clear();

        Tween bulletTween = bullet.DOJump(enemyTransform.position + enemyTransform.forward * 0.25f, 2.25f, 1, 0.45f).SetEase(Ease.Linear);
        // bulletTween.onUpdate = () =>
        // {
        //     if (enemyTransform)
        //     {
        //         targetPosition = enemyTransform.position;
        //     }
        //     bulletTween.SetTarget(targetPosition);
        // };
        bulletTween.onComplete = () =>
        {
            if (enemyTransform)
            {
                enemy.TakeDamage(_Data.DamageAmount);
            }
            bullet.Despawn();
            trail.gameObject.Despawn();
        };
    }

    public void ResetSelf()
    {
        _boostTween?.Kill(true);
    }
    
    [Serializable]
    public enum StatType
    {
        Damage,
        Firerate,
        Splitshot,
    }
    
    
    
    [System.Serializable]
    public class Data : ICloneable
    {
        [SerializeField] public Pool gunType;
        [SerializeField] public float prevShoot = 0.0f;
        [SerializeField] private int rate = 1;
        [SerializeField] private int split = 1;
        [SerializeField] private int damage = 1;
        [System.NonSerialized] public int Mult = 1;

        public float FireInterval { get; set; }
        public int RateAmount => rate * Mult;
        public int SplitAmount => rate * Mult;
        public int DamageAmount => rate * Mult;

        public int FireRate
        {
            set
            {
                this.rate = value;
                FireInterval = 1.1f - (value - 1) * 0.05f;
            }
            get => this.rate;
        }
            
        public Data()
        {
            this.rate = 1;
            this.split = 1;
            this.prevShoot = 0.0f;
            this.damage = 1;
        }
        public Data(Pool gunType, int damage, int rate, int split)
        {
            this.gunType = gunType;
            this.FireRate = rate;
            this.split = split;
            this.damage = damage;
        }
        public Data(Data data)
        {
            this.gunType = data.gunType;
            this.FireRate = data.rate;
            this.prevShoot = data.prevShoot;
            this.split = data.split;
            this.damage = data.damage;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine("Gun Type : " + gunType.ToString());
            stringBuilder.AppendLine("Rate : " + rate.ToString());
            stringBuilder.AppendLine("Split : " + split.ToString());
            stringBuilder.AppendLine("Damage : " + damage.ToString());
            stringBuilder.AppendLine("FireInterval : " + FireInterval.ToString());
            return stringBuilder.ToString();
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
        [SerializeField] public int[] defaultValues;
        [SerializeField] public Const.Currency[] damagePrice;
        [SerializeField] public Const.Currency[] fireRatePrice;
        [SerializeField] public Const.Currency[] splitShotPrice;
        // [System.NonSerialized] private List<Const.Currency[]> _upgradePrices = new();
            
        public int DefaultValue(Gun.StatType statType) => defaultValues[(int)statType];
        public int UpgradedValue(Gun.StatType statType, int upgradeIndex) => DefaultValue(statType) + upgradeIndex;
        public Const.Currency Price(Gun.StatType statType, int upgradeIndex) => Prices(statType)[upgradeIndex];
        public int UpgradeCount(Gun.StatType statType) => Prices(statType).Length;
        public bool HasAvailableUpgrade(Gun.StatType statType, int upgradeIndex) => upgradeIndex < Prices(statType).Length;
        public bool IsFull(Gun.StatType statType, int upgradeIndex) => upgradeIndex >= Prices(statType).Length;


        public Const.Currency[] Prices(Gun.StatType statType)
        {
            switch (statType)
            {
                case StatType.Damage:
                    return damagePrice;
                case StatType.Firerate:
                    return fireRatePrice;
                case StatType.Splitshot:
                    return splitShotPrice;
            }
            return null;
        }
    }
}