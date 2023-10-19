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
            StatDisplayArranger.THIS.Hide(StatDisplay.Type.Boost);
            Warzone.THIS.Player.Emission = 0.0f;
        }
        get => _data;
    }

    public void Boost()
    {
        this._Data.Mult++;
        
        StatDisplayArranger.THIS.UpdateAmount(StatDisplay.Type.Boost, this._Data.Mult, 0.5f, true);

        
        _boostTween?.Kill();
        float percent = 0.0f;
        _boostTween = DOTween.To(x => percent = x, 1.0f, 0.0f, 3.5f).SetEase(Ease.Linear);
        _boostTween.onUpdate = () =>
        {
            StatDisplayArranger.THIS.UpdatePercent(StatDisplay.Type.Boost, percent);

            Warzone.THIS.Player.Emission = Mathf.Repeat(percent * 10.0f, 1.0f);
        };
        _boostTween.onComplete = () =>
        {
            _Data.Mult = 1;
            StatDisplayArranger.THIS.Hide(StatDisplay.Type.Boost);
            Warzone.THIS.Player.Emission = 0.0f;
        };
    }

    public void Bubble()
    {
        Particle.Bubble.EmitForward(1, muzzle.position, muzzle.forward);
    }
    
    public void Shoot(Enemy enemy)
    {
        int enemyID = enemy.ID;
        
        Transform bullet = Pool.Bullet.Spawn().transform;
        
        TrailRenderer trail = Pool.Trail.Spawn<TrailRenderer>(bullet);
        
        bullet.DOKill();
        bullet.transform.position = muzzle.position;
        trail.Clear();
        
        Tween bulletTween = null;
        if (GunSo.jump)
        {
            bulletTween = bullet.DOJump(enemy.hitTarget.position, GunSo.jumpPower, 1, GunSo.travelDur).SetEase(Ease.Linear);
        }
        else
        {
            bulletTween = bullet.DOMove(enemy.hitTarget.position, GunSo.travelDur).SetEase(Ease.Linear);
        }
        bulletTween.onComplete = () =>
        {
            if (enemyID == enemy.ID)
            {
                enemy.TakeDamage(_Data.DamageAmount, 1.0f);
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
        [System.NonSerialized] private int _mult = 1;

        public int Mult
        {
            set
            {
                _mult = value;
                FireRate = rate;
            }
            get => _mult;
        }

        public float FireInterval { get; set; }
        public int RateAmount => rate * Mult;
        public int SplitAmount => split * Mult;
        public int DamageAmount => damage * Mult;

        public int FireRate
        {
            set
            {
                this.rate = value;
                FireInterval = (1.1f - (value - 1) * 0.05f) / _mult;
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
        [SerializeField] private Const.Currency currency;
        [SerializeField] public Pool gunType;
        [SerializeField] public Sprite sprite;
        [SerializeField] private int[] defaultValues;
        [SerializeField] private Const.Currency[] damagePrice;
        [SerializeField] private Const.Currency[] fireRatePrice;
        [SerializeField] private Const.Currency[] splitShotPrice;
        [SerializeField] public int unlockedAt = 1;

        public Const.Currency GunCost => currency.ReduceCost(Const.CurrencyType.Coin, Wallet.CostReduction);
        public Const.Currency UpgradePrice(Gun.StatType statType, int upgradeIndex) => (Prices(statType)[upgradeIndex]).ReduceCost(Const.CurrencyType.Coin, Wallet.CostReduction);

        public int DefaultValue(Gun.StatType statType) => defaultValues[(int)statType];
        public int UpgradedValue(Gun.StatType statType, int upgradeIndex) => DefaultValue(statType) + upgradeIndex;
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