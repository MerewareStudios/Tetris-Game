using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DG.Tweening;
using Internal.Core;
using IWI;
using IWI.Tutorial;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Game.UI
{
    public class WeaponMenu : Menu<WeaponMenu>, IMenu
    {
        [Header("Stage Bars")]
        [SerializeField] private Image frame;
        [SerializeField] private StageBar stageBarDamage;
        [SerializeField] private StageBar stageBarFireRate;
        [SerializeField] private StageBar stageBarSplitShot;
        [SerializeField] private Image gunImage;
        [SerializeField] private RectTransform newTextBanner;
        [SerializeField] private RectTransform equippedTextBanner;
        [SerializeField] private TextMeshProUGUI equipText;
        [SerializeField] private RectTransform stageBarParent;
        [SerializeField] private RectTransform purchaseParent;
        [SerializeField] private RectTransform purchaseClickTarget;
        [SerializeField] private CurrencyDisplay currencyDisplay;
        [SerializeField] private CurrenyButton purchaseButton;
        [SerializeField] private RectTransform priceTextPivot;
        [SerializeField] private RectTransform buttonRectTransform;
        [SerializeField] private Button equipButton;
        [SerializeField] private TextMeshProUGUI gunStatText;
        [SerializeField] private GameObject nextButton;
        [SerializeField] private GameObject previousButton;

        [System.NonSerialized] private WeaponShopData _weaponShopData;
        [System.NonSerialized] private Gun.UpgradeData _gunUpgradeData;
        [System.NonSerialized] public System.Action<Gun.Data> GunDataChanged = null;

        public WeaponShopData _Data
        {
            set => _weaponShopData = value;
            get => this._weaponShopData;
        }
        
        public new bool Open(float duration = 0.5f)
        {
            if (base.Open(duration))
            {
                return true;
            }
            Show();
            return false;
        }

        public void OnClick_Close()
        {
            if (base.Close())
            {
                return;
            }
        }
        
        public new void Show()
        {
            base.Show();
            CustomShow();
        }

        public void CustomShow(float gunPunchAmount = 0.1f, bool glimmer = false)
        {
            Onboarding.HideFinger();

            bool purchasedWeapon = _weaponShopData.Purchased;
            bool equippedWeapon = _weaponShopData.Equipped;

            stageBarParent.gameObject.SetActive(purchasedWeapon);
            purchaseParent.gameObject.SetActive(!purchasedWeapon);
            equipButton.gameObject.SetActive(purchasedWeapon && !equippedWeapon);
            
            _gunUpgradeData = Const.THIS.GunUpgradeData[_weaponShopData.inspectIndex];
            
            SetSprite(_gunUpgradeData.sprite, gunPunchAmount);
            if (glimmer)
            {
                frame.Glimmer(AnimConst.THIS.glimmerSpeedWeapon);
            }
            
            bool availableByLevel = LevelManager.CurrentLevel >= _gunUpgradeData.unlockedAt;


            newTextBanner.gameObject.SetActive(!purchasedWeapon);
            equippedTextBanner.gameObject.SetActive(!availableByLevel || equippedWeapon);
            
            equipText.text = equippedWeapon ? Onboarding.THIS.equippedText : Onboarding.THIS.unlockedAtText + _gunUpgradeData.unlockedAt;
            
            
            if (!purchasedWeapon)
            {
                PunchNewBanner(0.4f);
            }
            
            int damage = CurrentDamage(_gunUpgradeData);
            int rate = CurrentFireRate(_gunUpgradeData);
            int split = CurrentSplitShot(_gunUpgradeData);

            int damageDefault = _gunUpgradeData.DefaultValue(Gun.StatType.Damage);
            int rateDefault = _gunUpgradeData.DefaultValue(Gun.StatType.Firerate);
            int splitDefault = _gunUpgradeData.DefaultValue(Gun.StatType.Splitshot);

            SetStats(damage, damage != damageDefault, rate, rate != rateDefault, split, split != splitDefault);

            PunchPurchasedText(0.2f);
           
            if (purchaseParent.gameObject.activeSelf && ONBOARDING.PURCHASE_WEAPON.IsNotComplete())
            {
                if (Wallet.HasFunds(_gunUpgradeData.GunCost))
                {
                    Onboarding.ClickOn(purchaseClickTarget.position, Finger.Cam.UI, () =>
                    {
                        Transform buttonTransform = purchaseButton.transform;
                        buttonTransform.DOKill();
                        buttonTransform.localEulerAngles = Vector3.zero;
                        buttonTransform.localScale = Vector3.one;
                        buttonTransform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 1).SetUpdate(true);
                    });
                }
            }
            
            previousButton.SetActive(_weaponShopData.inspectIndex != 0 || _weaponShopData.gunShopDatas.Last().purchased);
            nextButton.SetActive(purchasedWeapon);
            
            if (!purchasedWeapon)
            {
                SetPrice(_gunUpgradeData.GunCost, availableByLevel);
                return;
            }
            
            FillStageBar(Gun.StatType.Damage, stageBarDamage);
            FillStageBar(Gun.StatType.Firerate, stageBarFireRate);
            FillStageBar(Gun.StatType.Splitshot, stageBarSplitShot);
            
            
            if (stageBarParent.gameObject.activeSelf && ONBOARDING.PURCHASE_FIRERATE.IsNotComplete() && stageBarFireRate.Available)
            {
                if (_gunUpgradeData.HasAvailableUpgrade(Gun.StatType.Firerate, _weaponShopData.CurrentIndex(Gun.StatType.Firerate)))
                {
                    Onboarding.ClickOn(stageBarFireRate.clickTarget.position, Finger.Cam.UI, () =>
                    {
                        stageBarFireRate.PunchPurchaseButton(0.2f);
                    });
                }
            }
        }
        
        private void PunchPurchasedText(float amount)
        {
            equippedTextBanner.DOKill();
            equippedTextBanner.localScale = Vector3.one;
            equippedTextBanner.DOPunchScale(Vector3.one * amount, 0.25f, 1).SetUpdate(true);
        }

        private void FillStageBar(Gun.StatType statType, StageBar stageBar)
        {
            int currentIndex = _weaponShopData.CurrentIndex(statType);

            bool max = _gunUpgradeData.IsFull(statType, currentIndex);
            int defaultValue = _gunUpgradeData.DefaultValue(statType);
            
            stageBar
                .SetMaxed(!max)
                .SetCurrencyStampVisible(!max)
                .SetBars(_gunUpgradeData.UpgradeCount(statType), currentIndex, defaultValue);

            if (max)
            {
                stageBar.Available = true;
                return;
            }
            Const.Currency price = _gunUpgradeData.UpgradePrice(statType, currentIndex);

            bool ticket = price.type.Equals(Const.CurrencyType.Ticket);
            
            stageBar
                .SetPrice(price)
                .Available = Wallet.HasFunds(price) || ticket;
        }
        
        public void SetStats(int damage, bool changedDamage, int rate, bool changedRate, int split, bool changedSplit)
        {
            StringBuilder stringBuilder = new();
            
            stringBuilder.Append(Onboarding.THIS.damageText);
            stringBuilder.Append(changedDamage ? Onboarding.THIS.weaponStatChange : Onboarding.THIS.weaponStatUnchange);
            stringBuilder.Append(damage);
            
            stringBuilder.Append(Onboarding.THIS.fireRateText);
            stringBuilder.Append(changedRate ? Onboarding.THIS.weaponStatChange : Onboarding.THIS.weaponStatUnchange);
            stringBuilder.Append(rate);
            
            stringBuilder.Append(Onboarding.THIS.splitShotText);
            stringBuilder.Append(changedSplit ? Onboarding.THIS.weaponStatChange : Onboarding.THIS.weaponStatUnchange);
            stringBuilder.Append(split);


            gunStatText.text = stringBuilder.ToString();
        }
        
        private bool SetPrice(Const.Currency currency, bool availableByLevel)
        {
            bool hasFunds = Wallet.HasFunds(currency);

            bool ticket = currency.type.Equals(Const.CurrencyType.Ticket);
            purchaseButton.Available = (hasFunds || ticket) && availableByLevel;
            // purchaseButton.ButtonSprite = ticket ? Const.THIS.watchButtonTexture : Const.THIS.getButtonTexture;
            purchaseButton.ButtonSprite = Const.THIS.GetButtonSprite(currency.type);

            if (hasFunds)
            {   
                PunchButton(0.2f);
            }
            
            currencyDisplay.Display(currency);
            PunchMoney(0.2f);
            return hasFunds;
        }
        
        private void SetSprite(Sprite sprite, float punchAmount)
        {
            gunImage.sprite = sprite;
            Transform gunImageTransform;
            (gunImageTransform = gunImage.transform).DOKill();
            gunImageTransform.localScale = Vector3.one;
            gunImageTransform.DOPunchScale(Vector3.one * punchAmount, 0.25f, 1).SetUpdate(true);
        }
        
        private void PunchMoney(float amount)
        {
            priceTextPivot.DOKill();
            priceTextPivot.localScale = Vector3.one;
            priceTextPivot.DOPunchScale(Vector3.one * amount, 0.25f, 1).SetUpdate(true);
        }
        private void PunchButton(float amount)
        {
            buttonRectTransform.DOKill();
            buttonRectTransform.localEulerAngles = Vector3.zero;
            buttonRectTransform.DOPunchRotation(new Vector3(0.0f, 0.0f, 10.0f), 0.3f, 15).SetUpdate(true);
        }
        private void PunchNewBanner(float amount)
        {
            newTextBanner.DOKill();
            newTextBanner.localScale = Vector3.one;
            newTextBanner.DOPunchScale(Vector3.one * amount, 0.25f, 1).SetUpdate(true);
        }

        
        public Gun.Data EquippedGunData
        {
            get
            {

                Gun.UpgradeData gunUpgradeData = Const.THIS.GunUpgradeData[_weaponShopData.equipIndex];

                int damage = CurrentDamage(gunUpgradeData);
                int rate = CurrentFireRate(gunUpgradeData);
                int split = CurrentSplitShot(gunUpgradeData);

                Pool gunType = gunUpgradeData.gunType;

                return new Gun.Data(gunType, damage, rate, split);
            }
        }

        private int CurrentDamage(Gun.UpgradeData gunUpgradeData)
        {
            int currentIndexDamage = _weaponShopData.CurrentIndex(Gun.StatType.Damage);
            int damage = gunUpgradeData.UpgradedValue(Gun.StatType.Damage, currentIndexDamage);
            return damage;
        }
        
        private int CurrentFireRate(Gun.UpgradeData gunUpgradeData)
        {
            int currentIndexFireRate = _weaponShopData.CurrentIndex(Gun.StatType.Firerate);
            int rate = gunUpgradeData.UpgradedValue(Gun.StatType.Firerate, currentIndexFireRate);
            return rate;
        }

        private int CurrentSplitShot(Gun.UpgradeData gunUpgradeData)
        {
            int currentIndexSplitShot = _weaponShopData.CurrentIndex(Gun.StatType.Splitshot);
            int split = gunUpgradeData.UpgradedValue(Gun.StatType.Splitshot, currentIndexSplitShot);
            return split;
        }

        public void OnClick_PurchaseUpgrade(int statType)
        {
            Gun.StatType type = (Gun.StatType)statType;

            Const.Currency price = _gunUpgradeData.UpgradePrice(type, _weaponShopData.CurrentIndex(type));

            if (Wallet.Consume(price))
            {
                _weaponShopData.Upgrade(type, 1);

                if (_weaponShopData.Equipped)
                {
                    GunDataChanged?.Invoke(EquippedGunData);
                }
                
                if (ONBOARDING.PURCHASE_FIRERATE.IsNotComplete())
                {
                    ONBOARDING.PURCHASE_FIRERATE.SetComplete();
                }
                
                CustomShow(0.2f, true);

                CheckMax();
                void CheckMax()
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Gun.StatType stat = (Gun.StatType)i;
                        int currentIndex = _weaponShopData.CurrentIndex(stat);
                        bool max = _gunUpgradeData.IsFull(stat, currentIndex);
                        if (!max)
                        {
                            return;
                        }
                    }
                    AnalyticsManager.WeaponMaxed(_weaponShopData.inspectIndex);
                }
            } 
            else
            {
                if (price.type.Equals(Const.CurrencyType.Ticket))
                {
                    AdManager.ShowTicketAd(() =>
                    {
                        Wallet.Transaction(Const.Currency.OneAd);
                        OnClick_PurchaseUpgrade(statType);
                    });
                }
            }
        }
        
        public void OnClick_PurchaseWeapon()
        {
            if (_weaponShopData.Purchased)
            {
                return;
            }
            
            bool availableByLevel = LevelManager.CurrentLevel >= _gunUpgradeData.unlockedAt;
            if (!availableByLevel)
            {
                PunchPurchasedText(0.25f);
                return;
            }
            
            if (Wallet.Consume(_gunUpgradeData.GunCost))
            {
                if (ONBOARDING.PURCHASE_WEAPON.IsNotComplete())
                {
                    ONBOARDING.PURCHASE_WEAPON.SetComplete();
                }
                
                _weaponShopData.Purchase();

                OnClick_Equip();
                
                AnalyticsManager.PurchasedWeaponCount(_weaponShopData.PurchasedCount() - 1);
            }
            else
            {
                if (_gunUpgradeData.GunCost.type.Equals(Const.CurrencyType.Ticket))
                {
                    AdManager.ShowTicketAd(() =>
                    {
                        Wallet.Transaction(Const.Currency.OneAd);
                        OnClick_PurchaseWeapon();
                    });
                }
            }
        }
        
        public void OnClick_Equip()
        {
            _weaponShopData.Equip();
            GunDataChanged?.Invoke(EquippedGunData);
            CustomShow(0.3f, true);
        }
        
        public void OnClick_ShowNext()
        {
            _weaponShopData.inspectIndex++;
            if (_weaponShopData.inspectIndex >= Const.THIS.GunUpgradeData.Length)
            {
                _weaponShopData.inspectIndex = 0;
            }
            Show();
        }
        public void OnClick_ShowPrevious()
        {
            _weaponShopData.inspectIndex--;
            if (_weaponShopData.inspectIndex < 0)
            {
                _weaponShopData.inspectIndex = Const.THIS.GunUpgradeData.Length - 1;
            }
            Show();
        }
        
        [System.Serializable]
        public class WeaponShopData : ICloneable
        {
            [SerializeField] public int inspectIndex;
            [SerializeField] public int equipIndex;
            [SerializeField] public List<GunShopData> gunShopDatas = new();

            public WeaponShopData()
            {
                
            }
            public WeaponShopData(WeaponShopData weaponShopData)
            {
                inspectIndex = weaponShopData.inspectIndex;
                equipIndex = weaponShopData.equipIndex;
                gunShopDatas.CopyFrom(weaponShopData.gunShopDatas);
            }
            
            public int CurrentIndex(Gun.StatType statType)
            {
                return gunShopDatas[inspectIndex].upgradeIndexes[(int)statType];
            }
            public int GetUpgradeIndexOfEquippedGun(Gun.StatType statType)
            {
                return gunShopDatas[equipIndex].upgradeIndexes[(int)statType];
            }
            public void Upgrade(Gun.StatType statType, int amount)
            {
                gunShopDatas[inspectIndex].upgradeIndexes[(int)statType] += amount;
            }
            public void Purchase()
            {
                gunShopDatas[inspectIndex].purchased = true;
            }
            public void Equip()
            {
                if (Equipped)
                {
                    return;
                }

                equipIndex = inspectIndex;
            }
            public int PurchasedCount()
            {
                int total = 0;
                foreach (var data in gunShopDatas)
                {
                    if (data.purchased)
                    {
                        total++;
                    }
                }
                return total;
            }
            public bool Purchased => gunShopDatas[this.inspectIndex].purchased;
            public bool Equipped => inspectIndex == equipIndex;

            public Gun.Data GetCurrentGunData()
            {
                return null;
            }
            
            public object Clone()
            {
                return new WeaponShopData(this);
            }
        } 
        
        [System.Serializable]
        public class GunShopData : ICloneable
        {
            [SerializeField] public bool purchased = false;
            [SerializeField] public int[] upgradeIndexes = new int[3];

            public GunShopData()
            {
                
            }
            public GunShopData(GunShopData gunShopData)
            {
                purchased = gunShopData.purchased;
                upgradeIndexes = gunShopData.upgradeIndexes.Clone() as int[];
            }

            public object Clone()
            {
                return new GunShopData(this);
            }
        } 
    }
}