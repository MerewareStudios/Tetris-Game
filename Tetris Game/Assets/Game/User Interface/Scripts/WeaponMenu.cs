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
        [SerializeField] private GameObject nextButton;
        [SerializeField] private GameObject previousButton;
        [System.NonSerialized] private Gun.UpgradeData _gunUpgradeData;
        [System.NonSerialized] public System.Action<Gun.Data> GunDataChanged = null;
        [System.NonSerialized] private int[] _statNotifies = new int[3] {-1, -1, -1};

        [field : System.NonSerialized] public WeaponShopData SavedData { set; get; }

        public int AvailablePurchaseCount(bool updatePage)
        {
            base.TotalNotify = 0;
            for (int i = 0; i < Const.THIS.GunUpgradeData.Length; i++)
            {
                Gun.UpgradeData lookUp = Const.THIS.GunUpgradeData[i];
                
                if (i == SavedData.equipIndex)
                {
                    continue;
                }

                Const.Currency cost = lookUp.Cost;

                
                bool purchased = SavedData.gunShopDatas[i].purchased;
                bool hasFunds = Wallet.HasFunds(cost);
                bool ticketType = lookUp.CostType.Equals(Const.CurrencyType.Ticket);
                bool availableByLevel = LevelManager.CurrentLevel >= lookUp.unlockedAt;
                bool newShown = SavedData.newShown[i];

                if (!purchased && (hasFunds || ticketType) && availableByLevel && !newShown)
                {
                    if (updatePage)
                    {
                        SavedData.inspectIndex = i;
                    }

                    base.TotalNotify++;
                    // return 1;
                }
                
                if (!purchased)
                {
                    break;
                }
            }


            if(NotifyForUpgrade(Gun.StatType.Damage))
            {
                base.TotalNotify++;
            }
            if(NotifyForUpgrade(Gun.StatType.Firerate))
            {
                base.TotalNotify++;
            }
            if(NotifyForUpgrade(Gun.StatType.Splitshot))
            {
                base.TotalNotify++;
            }
            
            return base.TotalNotify;
        }

        private bool NotifyForUpgrade(Gun.StatType statType)
        {
            Gun.UpgradeData equippedUpgradeData = Const.THIS.GunUpgradeData[SavedData.equipIndex];
            GunShopData equippedSavedData = SavedData.gunShopDatas[SavedData.equipIndex];

            int statIndex = (int)statType;
            
            int currentDamageIndex = equippedSavedData.upgradeIndexes[statIndex];
            if (!equippedUpgradeData.HasAvailableUpgrade(statType, currentDamageIndex))
            {
                return false;
            }
            
            Const.Currency upgradeCost = equippedUpgradeData.UpgradeCost(statType, currentDamageIndex);
            bool hasFunds = Wallet.HasFunds(upgradeCost);
            bool ticketType = upgradeCost.type.Equals(Const.CurrencyType.Ticket);
            if (!hasFunds && !ticketType)
            {
                return false;
            }
            Debug.Log("checking " + statType);
            Debug.Log(_statNotifies[statIndex] + " " + currentDamageIndex);
            return _statNotifies[statIndex] < currentDamageIndex;
        }

        // private bool UpgradeAvailable(int gunIndex, Gun.StatType statType)
        // {
        //     int upgradeIndex = SavedData.GetUpgradeIndex(gunIndex, statType);
        //     Gun.UpgradeData upgradeData = Const.THIS.GunUpgradeData[gunIndex];
        //     if (!upgradeData.HasAvailableUpgrade(statType, upgradeIndex))
        //     {
        //         return false;
        //     }
        //     
        //     Const.Currency cost = upgradeData.UpgradeCost(statType, upgradeIndex);
        //
        //     bool hasFunds = Wallet.HasFunds(cost);
        //     bool ticketType = cost.type.Equals(Const.CurrencyType.Ticket);
        //     
        //     return hasFunds || ticketType;
        // }
        
        public new bool Open(float duration = 0.5f)
        {
            if (base.Open(duration))
            {
                return true;
            }
            Show();
            return false;
        }

        // public void OnClick_Close()
        // {
        //     if (base.Close())
        //     {
        //         return;
        //     }
        // }
        
        public new void Show()
        {
            base.Show();
            CustomShow();
            UIManager.UpdateNotifications();
        }

        public void CustomShow(float gunPunchAmount = 0.1f, bool glimmer = false)
        {
            Onboarding.HideFinger();

            bool purchasedWeapon = SavedData.Purchased;
            bool equippedWeapon = SavedData.Equipped;

            stageBarParent.gameObject.SetActive(purchasedWeapon);
            purchaseParent.gameObject.SetActive(!purchasedWeapon);
            equipButton.gameObject.SetActive(purchasedWeapon && !equippedWeapon);
            
            _gunUpgradeData = Const.THIS.GunUpgradeData[SavedData.inspectIndex];
            
            SetSprite(_gunUpgradeData.sprite, gunPunchAmount);
            if (glimmer)
            {
                frame.Glimmer(AnimConst.THIS.glimmerSpeedWeapon);
            }
            
            Const.Currency cost = _gunUpgradeData.Cost;
            // (Const.Currency cost, bool reduced) = _gunUpgradeData.Cost;
            
            
            bool hasFunds = Wallet.HasFunds(cost);
            bool ticketType = cost.type.Equals(Const.CurrencyType.Ticket);
            bool availableByLevel = LevelManager.CurrentLevel >= _gunUpgradeData.unlockedAt;
            
            bool canPurchase = (hasFunds || ticketType) && availableByLevel;


            bool newBannerVisible = !purchasedWeapon && canPurchase && !SavedData.newShown[SavedData.inspectIndex];
            newTextBanner.gameObject.SetActive(newBannerVisible);
            if (newBannerVisible)
            {
                SavedData.newShown[SavedData.inspectIndex] = true;
            }
            

            if (availableByLevel)
            {
                equipText.text = equippedWeapon ? Onboarding.THIS.equippedText : "";
                equippedTextBanner.gameObject.SetActive(purchasedWeapon);
            }
            else
            {
                equippedTextBanner.gameObject.SetActive(true);
                equipText.text = Onboarding.THIS.unlockedAtText + _gunUpgradeData.unlockedAt;
            }
            
            
            if (!purchasedWeapon)
            {
                PunchNewBanner(0.4f);
            }

           

            // int damageDefault = _gunUpgradeData.DefaultValue(Gun.StatType.Damage);
            // int rateDefault = _gunUpgradeData.DefaultValue(Gun.StatType.Firerate);
            // int splitDefault = _gunUpgradeData.DefaultValue(Gun.StatType.Splitshot);

            // SetStats(damage, damage != damageDefault, rate, rate != rateDefault, split, split != splitDefault);

            PunchPurchasedText(0.2f);
           
            if (purchaseParent.gameObject.activeSelf && ONBOARDING.PURCHASE_WEAPON.IsNotComplete())
            {
                if (Wallet.HasFunds(cost) && availableByLevel)
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
            
            previousButton.SetActive(SavedData.inspectIndex != 0 || SavedData.gunShopDatas.Last().purchased);
            nextButton.SetActive(purchasedWeapon);
            
            if (!purchasedWeapon)
            {
                // SetPrice(cost, canPurchase, reduced);
                SetPrice(cost, canPurchase);
                return;
            }

            int currentDamage = CurrentStat(_gunUpgradeData, Gun.StatType.Damage);
            int currentFireRate = CurrentStat(_gunUpgradeData, Gun.StatType.Firerate);
            int currentSplitShot = CurrentStat(_gunUpgradeData, Gun.StatType.Splitshot);

            // if (SavedData.equipIndex == SavedData.inspectIndex)
            // {
            //     this._statNotifies[0] = currentDamage;
            //     this._statNotifies[1] = currentFireRate;
            //     this._statNotifies[2] = currentSplitShot;
            // }
            //
            FillStageBar(Gun.StatType.Damage, stageBarDamage, currentDamage);
            FillStageBar(Gun.StatType.Firerate, stageBarFireRate, currentFireRate);
            FillStageBar(Gun.StatType.Splitshot, stageBarSplitShot, currentSplitShot);
            
            
            if (stageBarParent.gameObject.activeSelf && ONBOARDING.PURCHASE_FIRERATE.IsNotComplete() && stageBarFireRate.Available)
            {
                if (_gunUpgradeData.HasAvailableUpgrade(Gun.StatType.Firerate, SavedData.CurrentIndex(Gun.StatType.Firerate)))
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

        private void FillStageBar(Gun.StatType statType, StageBar stageBar, int currentStat)
        {
            int currentIndex = SavedData.CurrentIndex(statType);

            bool max = _gunUpgradeData.IsFull(statType, currentIndex);
            int defaultValue = _gunUpgradeData.DefaultValue(statType);
            
            stageBar
                .SetMaxed(!max)
                .SetCurrencyStampVisible(!max)
                .SetStat(currentStat)
                .SetBars(_gunUpgradeData.UpgradeCount(statType), currentIndex, defaultValue);

            if (SavedData.equipIndex == SavedData.inspectIndex)
            {
                this._statNotifies[(int)statType] = currentIndex;
            }
            
            if (max)
            {
                stageBar.Available = true;
                return;
            }
            
            Const.Currency price = _gunUpgradeData.UpgradePrice(statType, currentIndex);

            bool ticketType = price.type.Equals(Const.CurrencyType.Ticket);
            
            stageBar
                .SetPrice(price)
                .Available = Wallet.HasFunds(price) || ticketType;
        }
        
        public void SetStats(int damage, bool changedDamage, int rate, bool changedRate, int split, bool changedSplit)
        {
            // StringBuilder stringBuilder = new();
            //
            // stringBuilder.Append(Onboarding.THIS.damageText);
            // stringBuilder.Append(changedDamage ? Onboarding.THIS.weaponStatChange : Onboarding.THIS.weaponStatUnchange);
            // stringBuilder.Append(damage);
            //
            // stringBuilder.Append(Onboarding.THIS.fireRateText);
            // stringBuilder.Append(changedRate ? Onboarding.THIS.weaponStatChange : Onboarding.THIS.weaponStatUnchange);
            // stringBuilder.Append(rate);
            //
            // stringBuilder.Append(Onboarding.THIS.splitShotText);
            // stringBuilder.Append(changedSplit ? Onboarding.THIS.weaponStatChange : Onboarding.THIS.weaponStatUnchange);
            // stringBuilder.Append(split);


            // gunStatText.text = stringBuilder.ToString();
        }
        
        private void SetPrice(Const.Currency currency, bool canPurchase)
        {
            purchaseButton.Available = canPurchase;
            // purchaseButton.ButtonSprite = Const.THIS.GetButtonSprite(currency.type);

            if (canPurchase)
            {   
                PunchButton(0.2f);
            }
            currencyDisplay.Display(currency);
            PunchMoney(0.2f);
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

                Gun.UpgradeData gunUpgradeData = Const.THIS.GunUpgradeData[SavedData.equipIndex];

                int damage = CurrentStat(gunUpgradeData, Gun.StatType.Damage);
                int rate = CurrentStat(gunUpgradeData, Gun.StatType.Firerate);
                int split = CurrentStat(gunUpgradeData, Gun.StatType.Damage);

                Pool gunType = gunUpgradeData.gunType;

                return new Gun.Data(gunType, damage, rate, split);
            }
        }

        private int CurrentStat(Gun.UpgradeData gunUpgradeData, Gun.StatType statType) => gunUpgradeData.UpgradedValue(statType, SavedData.CurrentIndex(statType));
        
        // private int CurrentFireRate(Gun.UpgradeData gunUpgradeData)
        // {
        //     int currentIndexFireRate = SavedData.CurrentIndex(Gun.StatType.Firerate);
        //     int rate = gunUpgradeData.UpgradedValue(Gun.StatType.Firerate, currentIndexFireRate);
        //     return rate;
        // }
        //
        // private int CurrentSplitShot(Gun.UpgradeData gunUpgradeData)
        // {
        //     int currentIndexSplitShot = SavedData.CurrentIndex(Gun.StatType.Splitshot);
        //     int split = gunUpgradeData.UpgradedValue(Gun.StatType.Splitshot, currentIndexSplitShot);
        //     return split;
        // }

        public void OnClick_PurchaseUpgrade(int statType)
        {
            Gun.StatType type = (Gun.StatType)statType;

            Const.Currency cost = _gunUpgradeData.UpgradePrice(type, SavedData.CurrentIndex(type));

            if (Wallet.Consume(cost))
            {
                SavedData.Upgrade(type, 1);

                if (SavedData.Equipped)
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
                        int currentIndex = SavedData.CurrentIndex(stat);
                        bool max = _gunUpgradeData.IsFull(stat, currentIndex);
                        if (!max)
                        {
                            return;
                        }
                    }
                    AnalyticsManager.WeaponMaxed(SavedData.inspectIndex);
                }
            } 
            else
            {
                if (cost.type.Equals(Const.CurrencyType.Ticket))
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
            if (SavedData.Purchased)
            {
                return;
            }
            
            bool availableByLevel = LevelManager.CurrentLevel >= _gunUpgradeData.unlockedAt;
            if (!availableByLevel)
            {
                PunchPurchasedText(0.25f);
                return;
            }

            Const.Currency cost = _gunUpgradeData.Cost;
            
            if (Wallet.Consume(cost))
            {
                if (ONBOARDING.PURCHASE_WEAPON.IsNotComplete())
                {
                    ONBOARDING.PURCHASE_WEAPON.SetComplete();
                }
                
                SavedData.Purchase();

                OnClick_Equip();
                
                AnalyticsManager.PurchasedWeaponCount(SavedData.PurchasedCount() - 1);
            }
            else
            {
                if (cost.type.Equals(Const.CurrencyType.Ticket))
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
            SavedData.Equip();
            GunDataChanged?.Invoke(EquippedGunData);
            CustomShow(0.3f, true);
        }
        
        public void OnClick_ShowNext()
        {
            SavedData.inspectIndex++;
            if (SavedData.inspectIndex >= Const.THIS.GunUpgradeData.Length)
            {
                SavedData.inspectIndex = 0;
            }
            Show();
        }
        public void OnClick_ShowPrevious()
        {
            SavedData.inspectIndex--;
            if (SavedData.inspectIndex < 0)
            {
                SavedData.inspectIndex = Const.THIS.GunUpgradeData.Length - 1;
            }
            Show();
        }
        
        [System.Serializable]
        public class WeaponShopData : ICloneable
        {
            [SerializeField] public int inspectIndex;
            [SerializeField] public int equipIndex;
            [SerializeField] public List<GunShopData> gunShopDatas = new();
            [SerializeField] public List<bool> newShown;

            public WeaponShopData()
            {
                
            }
            public WeaponShopData(WeaponShopData weaponShopData)
            {
                inspectIndex = weaponShopData.inspectIndex;
                equipIndex = weaponShopData.equipIndex;
                gunShopDatas.CopyFrom(weaponShopData.gunShopDatas);
                newShown = new List<bool>(weaponShopData.newShown);
            }
            
            public int CurrentIndex(Gun.StatType statType)
            {
                return gunShopDatas[inspectIndex].upgradeIndexes[(int)statType];
            }
            public int GetUpgradeIndex(int gunIndex, Gun.StatType statType)
            {
                return gunShopDatas[gunIndex].upgradeIndexes[(int)statType];
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