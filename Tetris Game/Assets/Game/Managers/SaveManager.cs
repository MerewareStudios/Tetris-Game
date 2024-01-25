using Internal.Core;
using System;
using Game;
using Game.UI;
using IWI;
using UnityEngine;

public class SaveManager : SaveManagerBase<SaveManager>
{
    [SerializeField] public bool SKIP_ONBOARDING = true;
    

    public override void Init()
    {
        base.Init();

        GameManager.THIS.Init();

        if (SaveData == null)
        {
            SaveData = Const.THIS.DefaultSaveData.Clone() as SaveData;
            SaveData.accountData.NewGuid();
            
            int onboardingCount = System.Enum.GetValues(typeof(ONBOARDING)).Length;
            SaveData.onboardingList = new bool[onboardingCount].Fill(true);
        }

        if (ONBOARDING.ALL_BLOCK_STEPS.IsNotComplete())
        {
            ONBOARDING.DRAG_AND_DROP.ClearStep();
            ONBOARDING.BLOCK_ROTATION.ClearStep();
            // ONBOARDING.SPEECH_MERGE.ClearStep();
            ONBOARDING.SPEECH_CHEER.ClearStep();
            ONBOARDING.PASSIVE_META.ClearStep();
            ONBOARDING.ALL_BLOCK_STEPS.ClearStep();
        }
        
        
        Wallet.COIN.Set(ref SaveData.userData.coinTransactionData);
        Wallet.COIN.Active = ONBOARDING.PASSIVE_META.IsComplete();
        Wallet.PIGGY.Set(ref SaveData.userData.gemTransactionData);
        Wallet.PIGGY.Active = ONBOARDING.PASSIVE_META.IsComplete();
        Wallet.TICKET.Set(ref SaveData.userData.adTransactionData);
        Wallet.TICKET.Active = ONBOARDING.PASSIVE_META.IsComplete();

        AdManager.THIS._Data = SaveData.adData;
        
        Powerup.THIS._Data = SaveData.userData.pupData;
        BlockMenu.THIS.SavedData = SaveData.userData.blockShopData;
        WeaponMenu.THIS.SavedData = SaveData.userData.weaponShopData;
        PiggyMenu.THIS.SavedData = SaveData.userData.piggyData;
        OfferScreen.THIS._Data = SaveData.purchaseData;

        Warzone.THIS.Player._Data = SaveData.userData.playerData;
        Warzone.THIS.airplane.SavedData = SaveData.userData.airplaneData;
        Board.THIS.SavedData = SaveData.userData.boardData;

        MenuNavigator.THIS.SavedData = SaveData.userData.menuNavData;
        
        LevelManager.THIS.levelText.enabled = ONBOARDING.PASSIVE_META.IsComplete();
        Warzone.THIS.enemyProgressbar.Visible = ONBOARDING.PASSIVE_META.IsComplete();
        UIManager.THIS.shop.VisibleImmediate = ONBOARDING.WEAPON_TAB.IsComplete();
        Spawner.THIS.nextBlockDisplay.Visible = SaveData.userData.level >= 5;
        Spawner.THIS.nextBlockDisplay.Available = false;
        
        UIManager.THIS.PlusButtonsState = ONBOARDING.WEAPON_TAB.IsComplete();

        #if CREATIVE
            UIManager.THIS.SettingsEnabled = Const.THIS.creativeSettings.settingsEnabled;
        #endif

        HapticManager.THIS.SavedData = SaveData.hapticData;
        SettingsManager.THIS.Set();
    }
}
public static class SaveManagerExtensions
{
    public static bool IsNotComplete(this ONBOARDING onboardingStep)
    {
#if CREATIVE
        return false;
#endif
        return !SaveManager.THIS.SKIP_ONBOARDING && SaveManager.THIS.SaveData.onboardingList[((int)onboardingStep)];
    }
    public static bool IsComplete(this ONBOARDING onboardingStep)
    {
#if CREATIVE
        return true;
#endif
        return SaveManager.THIS.SKIP_ONBOARDING || !SaveManager.THIS.SaveData.onboardingList[((int)onboardingStep)];
    }
    public static void SetComplete(this ONBOARDING onboardingStep)
    {
        SaveManager.THIS.SaveData.onboardingList[((int)onboardingStep)] = false;
        AnalyticsManager.OnboardingStepComplete(onboardingStep.ToString());
    }
    public static void ClearStep(this ONBOARDING onboardingStep)
    {
        SaveManager.THIS.SaveData.onboardingList[((int)onboardingStep)] = true;
    }
    public static Pool RandomBlock(this Spawner spawner)
    {
        if (Input.GetKey(KeyCode.A))
        {
            return Pool.Single_Block;

        }
        if (Input.GetKey(KeyCode.S))
        {
            return Pool.S_Block;

        }
        return SaveManager.THIS.SaveData.userData.blockShopData.GetRandomBlock();
    }
    public static int CurrentLevel(this LevelManager levelManager)
    {
        return SaveManager.THIS.SaveData.userData.level;
    }
    public static int NextLevel(this LevelManager levelManager)
    {
        return ++SaveManager.THIS.SaveData.userData.level;
    }
}
public partial class SaveData : ICloneable
{
    [SerializeField] public bool[] onboardingList;
    [SerializeField] public Account.Data accountData;
    [SerializeField] public HapticManager.Data hapticData;
    [SerializeField] public User.Data userData;
    [SerializeField] public AdManager.Data adData;
    [SerializeField] public OfferScreen.Data purchaseData;
    
    public SaveData(SaveData data)
    {
        onboardingList = data.onboardingList.Clone() as bool[];
        accountData = data.accountData.Clone() as Account.Data;
        hapticData = data.hapticData.Clone() as HapticManager.Data;
        userData = data.userData.Clone() as User.Data;
        adData = data.adData.Clone() as AdManager.Data;
        purchaseData = data.purchaseData.Clone() as OfferScreen.Data;
    }
    
    public object Clone()
    {
        return new SaveData(this);
    }
}


public static class Account
{
    public static Data Current => SaveManager.THIS.SaveData.accountData;

    [System.Serializable]
    public class Data : ICloneable
    {
        [SerializeField] public bool commented = false;
        [SerializeField] public string guid;
        [SerializeField] public int age = 100;
        [SerializeField] public bool firstPurchase = false;
        [SerializeField] public bool firstAd = false;

        public Data(Data data)
        {
            commented = data.commented;
            guid = data.guid;
            age = data.age;
            firstPurchase = data.firstPurchase;
            firstAd = data.firstAd;
        }
        public void NewGuid()
        {
            guid = System.Guid.NewGuid().ToString();
        }
        public object Clone()
        {
            return new Account.Data(this);
        }
        
        public bool IsUnderAgeForGDPR() => Account.Current.age < 16;
        public bool IsUnderAgeForCOPPA() => Account.Current.age < 13;
    }
}

namespace User
{
    [System.Serializable]
    public class TransactionData<T> : ICloneable
    {
        [SerializeField] public T value;

        public TransactionData()
        {
            value = default(T);
        }
        public TransactionData(TransactionData<T> transaction)
        {
            this.value = transaction.value;
        }
        public object Clone()
        {
            return new User.TransactionData<T>(this);
        }
    } 
    
    [System.Serializable]
    public class Data : ICloneable
    {
        [SerializeField] public int level = 1;
        [SerializeField] public Player.Data playerData;
        [SerializeField] public Powerup.Data pupData;
        [SerializeField] public Board.Data boardData;
        [SerializeField] public TransactionData<int> coinTransactionData = new();
        [SerializeField] public TransactionData<int> gemTransactionData = new();
        [SerializeField] public TransactionData<int> adTransactionData = new();
        [SerializeField] public BlockMenu.BlockShopData blockShopData;
        [SerializeField] public WeaponMenu.WeaponShopData weaponShopData;
        [SerializeField] public MenuNavigator.Data menuNavData;
        [SerializeField] public PiggyMenu.Data piggyData;
        [SerializeField] public Airplane.Data airplaneData;

        public Data(Data data)
        {
            level = data.level;
            playerData = data.playerData.Clone() as Player.Data;
            pupData = data.pupData.Clone() as Powerup.Data;
            boardData = data.boardData.Clone() as Board.Data;
            coinTransactionData = data.coinTransactionData.Clone() as TransactionData<int>;
            gemTransactionData = data.gemTransactionData.Clone() as TransactionData<int>;
            adTransactionData = data.adTransactionData.Clone() as TransactionData<int>;
            blockShopData = data.blockShopData.Clone() as BlockMenu.BlockShopData;
            weaponShopData = data.weaponShopData.Clone() as WeaponMenu.WeaponShopData;
            menuNavData = data.menuNavData.Clone() as MenuNavigator.Data;
            piggyData = data.piggyData.Clone() as PiggyMenu.Data;
            airplaneData = data.airplaneData.Clone() as Airplane.Data;
        }

        public object Clone()
        {
            return new User.Data(this);
        }
    } 
}


public enum ONBOARDING
{
    BLOCK_ROTATION,
    DRAG_AND_DROP,
    // SPEECH_MERGE,
    SPEECH_CHEER,
    PASSIVE_META,
    ALL_BLOCK_STEPS,
    
    PIGGY_INVEST,
    PIGGY_BREAK,
    PIGGY_CONTINUE,
    
    BLOCK_TAB,
    WEAPON_TAB,
    
    PURCHASE_BLOCK,
    PURCHASE_UPGRADE,
    PURCHASE_WEAPON,
    
    USE_POWERUP,
    NEW_T_BLOCK,
    MAGNET_SUGGESTION,
}

