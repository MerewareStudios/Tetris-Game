using System;
using System.Collections.Generic;
using System.Linq;
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
        [SerializeField] private Image statFrame;
        [SerializeField] private Color equippedColor, lockedColor, unequippedColor;
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
        [SerializeField] private RectTransform redDot;
        [System.NonSerialized] private Gun.UpgradeData _gunUpgradeData;
        [System.NonSerialized] public System.Action<Gun.Data> GunDataChanged = null;

        [System.NonSerialized] private int _lastWeaponIndexShown = -1;

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

                if (!purchased && (hasFunds || ticketType) && availableByLevel)
                {
                    if (updatePage && _lastWeaponIndexShown < i)
                    {
                        SavedData.inspectIndex = i;
                        _lastWeaponIndexShown = i;
                    }

                    base.TotalNotify++;
                }
                
                if (!purchased)
                {
                    break;
                }
            }


            if(UpgradeAvailableEquipped(Gun.StatType.Damage))
            {
                base.TotalNotify++;
            }
            if(UpgradeAvailableEquipped(Gun.StatType.Firerate))
            {
                base.TotalNotify++;
            }
            if(UpgradeAvailableEquipped(Gun.StatType.Splitshot))
            {
                base.TotalNotify++;
            }
            
            return base.TotalNotify;
        }
        
        public bool Marked
        {
            set
            {
                redDot.DOKill();
                redDot.localScale = Vector3.zero;
                if (value)
                {
                    redDot.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
                }
            }
        }

        private bool UpgradeAvailableEquipped(Gun.StatType statType)
        {
            Gun.UpgradeData equippedUpgradeData = Const.THIS.GunUpgradeData[SavedData.equipIndex];
            int upgradeIndex = SavedData.GetUpgradeIndex(SavedData.equipIndex, statType);
            
            if (!equippedUpgradeData.HasAvailableUpgrade(statType, upgradeIndex))
            {
                return false;
            }
            
            Const.Currency cost = equippedUpgradeData.UpgradeCost(statType, upgradeIndex);
        
            bool hasFunds = Wallet.HasFunds(cost);
            bool ticketType = cost.type.Equals(Const.CurrencyType.Ticket);
            
            return hasFunds || ticketType;
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
            
            
            bool hasFunds = Wallet.HasFunds(cost);
            bool ticketType = cost.type.Equals(Const.CurrencyType.Ticket);
            bool availableByLevel = LevelManager.CurrentLevel >= _gunUpgradeData.unlockedAt;
            
            bool canPurchase = (hasFunds || ticketType) && availableByLevel;

            statFrame.color = equippedWeapon ? equippedColor : (availableByLevel ? unequippedColor : lockedColor);

            bool newBannerVisible = !purchasedWeapon && availableByLevel;
            newTextBanner.gameObject.SetActive(newBannerVisible);
            
            Marked = canPurchase;

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
                SetPrice(cost, canPurchase);
                return;
            }

            int currentDamage = CurrentStatOf(SavedData.inspectIndex, Gun.StatType.Damage);
            int currentFireRate = CurrentStatOf(SavedData.inspectIndex, Gun.StatType.Firerate);
            int currentSplitShot = CurrentStatOf(SavedData.inspectIndex, Gun.StatType.Splitshot);

            FillStageBar(Gun.StatType.Damage, stageBarDamage, currentDamage, equippedWeapon);
            FillStageBar(Gun.StatType.Firerate, stageBarFireRate, currentFireRate, equippedWeapon);
            FillStageBar(Gun.StatType.Splitshot, stageBarSplitShot, currentSplitShot, equippedWeapon);
            
            
            if (stageBarParent.gameObject.activeSelf && ONBOARDING.PURCHASE_UPGRADE.IsNotComplete() && stageBarFireRate.Available)
            {
                if (_gunUpgradeData.HasAvailableUpgrade(Gun.StatType.Firerate, SavedData.CurrentIndex(SavedData.inspectIndex, Gun.StatType.Firerate)))
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

        private void FillStageBar(Gun.StatType statType, StageBar stageBar, int currentStat, bool equipped)
        {
            int currentIndex = SavedData.CurrentIndex(SavedData.inspectIndex, statType);

            bool max = _gunUpgradeData.IsFull(statType, currentIndex);
            int defaultValue = _gunUpgradeData.DefaultValue(statType);
            
            stageBar
                .SetMaxed(!max)
                .SetCurrencyStampVisible(!max)
                .SetStat(currentStat)
                .SetBars(_gunUpgradeData.UpgradeCount(statType), currentIndex, defaultValue);

            
            if (max)
            {
                stageBar.Available = true;
                stageBar.Marked = false;
                return;
            }
            
            Const.Currency price = _gunUpgradeData.UpgradePrice(statType, currentIndex);

            bool ticketType = price.type.Equals(Const.CurrencyType.Ticket);
            bool canPurchase = Wallet.HasFunds(price) || ticketType;
            
            stageBar
                .SetPrice(price)
                .Available = canPurchase;
            stageBar.Marked = canPurchase && equipped;
        }
        
        private void SetPrice(Const.Currency currency, bool canPurchase)
        {
            purchaseButton.Available = canPurchase;

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

                int damage = CurrentStatOf(SavedData.equipIndex, Gun.StatType.Damage);
                int rate = CurrentStatOf(SavedData.equipIndex, Gun.StatType.Firerate);
                int split = CurrentStatOf(SavedData.equipIndex, Gun.StatType.Splitshot);
                
                Pool gunType = gunUpgradeData.gunType;

                return new Gun.Data(gunType, damage, rate, split);
            }
        }

        private int CurrentStatOf(int gunDataIndex, Gun.StatType statType) => Const.THIS.GunUpgradeData[gunDataIndex].UpgradedValue(statType, SavedData.CurrentIndex(gunDataIndex, statType));
        
        public void OnClick_PurchaseUpgrade(int statType)
        {
            Gun.StatType type = (Gun.StatType)statType;

            Const.Currency cost = _gunUpgradeData.UpgradePrice(type, SavedData.CurrentIndex(SavedData.inspectIndex, type));

            if (Wallet.Consume(cost))
            {
                int current = SavedData.Upgrade(type, 1);
                HapticManager.OnClickVibrate(Audio.Button_Click_Upgrade, 0.9f + current * 0.025f);

                if (SavedData.Equipped)
                {
                    GunDataChanged?.Invoke(EquippedGunData);
                }
                
                if (ONBOARDING.PURCHASE_UPGRADE.IsNotComplete())
                {
                    ONBOARDING.PURCHASE_UPGRADE.SetComplete();
                }
                

                CustomShow(0.2f, true);
                UIManager.UpdateNotifications();

                CheckMax();
                void CheckMax()
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Gun.StatType stat = (Gun.StatType)i;
                        int currentIndex = SavedData.CurrentIndex(SavedData.inspectIndex, stat);
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
                HapticManager.OnClickVibrate(Audio.Button_Click_Forbidden);

                if (cost.type.Equals(Const.CurrencyType.Ticket))
                {
                    AdManager.ShowTicketAd(AdBreakScreen.AdReason.WEAPON_UPG, AdManager.GetTicketOfferForWeapon(),() =>
                    {
                        // Wallet.Transaction(Const.Currency.RewardedEarn);
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
                HapticManager.OnClickVibrate(Audio.Button_Click_Locked);
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
                
                HapticManager.OnClickVibrate(Audio.Button_Click_Purchase);

                OnClick_Equip();
                
                AnalyticsManager.PurchasedWeaponCount(SavedData.PurchasedCount() - 1);
            }
            else
            {
                HapticManager.OnClickVibrate(Audio.Button_Click_Forbidden);

                if (cost.type.Equals(Const.CurrencyType.Ticket))
                {
                    AdManager.ShowTicketAd(AdBreakScreen.AdReason.WEAPON_BUY, AdManager.GetTicketOfferForWeapon(),() =>
                    {
                        // Wallet.Transaction(Const.Currency.RewardedEarn);
                        OnClick_PurchaseWeapon();
                    });
                }
            }
        }
        
        public void OnClick_Equip()
        {
            HapticManager.OnClickVibrate(Audio.Button_Click_Equip);

            SavedData.Equip();
            GunDataChanged?.Invoke(EquippedGunData);
            CustomShow(0.3f, true);
            UIManager.UpdateNotifications();
        }
        
        public void OnClick_ShowNext()
        {
            HapticManager.OnClickVibrate();

            SavedData.inspectIndex++;
            if (SavedData.inspectIndex >= Const.THIS.GunUpgradeData.Length)
            {
                SavedData.inspectIndex = 0;
            }
            Show();
        }
        public void OnClick_ShowPrevious()
        {
            HapticManager.OnClickVibrate();

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

            public WeaponShopData()
            {
                
            }
            public WeaponShopData(WeaponShopData weaponShopData)
            {
                inspectIndex = weaponShopData.inspectIndex;
                equipIndex = weaponShopData.equipIndex;
                gunShopDatas.CopyFrom(weaponShopData.gunShopDatas);
            }
            
            public int CurrentIndex(int gunDataIndex, Gun.StatType statType)
            {
                return gunShopDatas[gunDataIndex].upgradeIndexes[(int)statType];
            }
            public int GetUpgradeIndex(int gunIndex, Gun.StatType statType)
            {
                return gunShopDatas[gunIndex].upgradeIndexes[(int)statType];
            }
            public int Upgrade(Gun.StatType statType, int amount)
            {
                gunShopDatas[inspectIndex].upgradeIndexes[(int)statType] += amount;
                return gunShopDatas[inspectIndex].upgradeIndexes[(int)statType];
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