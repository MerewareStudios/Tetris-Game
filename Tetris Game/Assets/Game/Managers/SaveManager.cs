using Internal.Core;
using System;
using Game;
using Game.UI;
using IWI;
using UnityEngine;

public class SaveManager : SaveManagerBase<SaveManager>
{
    [SerializeField] public bool SKIP_ONBOARDING = true;
    [SerializeField] public Const Const;
    [SerializeField] public AnimConst AnimConst;
    [SerializeField] public Onboarding Onboarding;


    public override void Awake()
    {
        base.Awake();

        Const.THIS = this.Const;
        AnimConst.THIS = this.AnimConst;
        Onboarding.THIS = this.Onboarding;

        if (!saveData.saveGenerated)
        {
            saveData.saveGenerated = true;

            int onboardingCount = System.Enum.GetValues(typeof(ONBOARDING)).Length;
            saveData.onboardingList = new bool[onboardingCount].Fill(true);

            // saveData.concentData = Const.THIS.DefaultConcentData.Clone() as Concent.Data;
            saveData.accountData = new Account.Data();
            saveData.userData = Const.THIS.DefaultUserData.Clone() as User.Data;
            saveData.playerData = Const.THIS.DefaultPlayerData.Clone() as Player.Data;
            saveData.adData = Const.THIS.DefaultAdData.Clone() as AdManager.Data;
            saveData.purchaseData = Const.THIS.DefaultPurchaseData.Clone() as OfferScreen.Data;
        }

        if (ONBOARDING.ALL_BLOCK_STEPS.IsNotComplete())
        {
            ONBOARDING.DRAG_AND_DROP.ClearStep();
            ONBOARDING.BLOCK_ROTATION.ClearStep();
            ONBOARDING.SPEECH_MERGE.ClearStep();
            ONBOARDING.SPEECH_CHEER.ClearStep();
            ONBOARDING.PASSIVE_META.ClearStep();
            ONBOARDING.ALL_BLOCK_STEPS.ClearStep();
        }
        
        // Concent.THIS._Data = saveData.concentData;

        Wallet.COIN.Set(ref saveData.userData.coinTransactionData);
        Wallet.COIN.Active = ONBOARDING.PASSIVE_META.IsComplete();
        Wallet.PIGGY.Set(ref saveData.userData.gemTransactionData);
        Wallet.PIGGY.Active = ONBOARDING.PASSIVE_META.IsComplete();
        Wallet.TICKET.Set(ref saveData.userData.adTransactionData);
        Wallet.TICKET.Active = ONBOARDING.PASSIVE_META.IsComplete();

        AdManager.THIS._Data = saveData.adData;
        
        Powerup.THIS._Data = saveData.userData.pupData;
        BlockMenu.THIS.SavedData = saveData.userData.blockShopData;
        WeaponMenu.THIS.SavedData = saveData.userData.weaponShopData;
        PiggyMenu.THIS.SavedData = saveData.userData.piggyData;
        OfferScreen.THIS._Data = saveData.purchaseData;

        Warzone.THIS.Player._Data = saveData.playerData;
        Warzone.THIS.airplane.SavedData = saveData.userData.airplaneData;
        Board.THIS._Data = saveData.userData.boardData;

        MenuNavigator.THIS.SavedData = saveData.userData.menuNavData;
        
        LevelManager.THIS.levelText.enabled = ONBOARDING.PASSIVE_META.IsComplete();
        UIManager.THIS.shop.VisibleImmediate = ONBOARDING.WEAPON_TAB.IsComplete();
        
        UIManager.THIS.PlusButtonsState = ONBOARDING.WEAPON_TAB.IsComplete();
    }
}
public static class SaveManagerExtensions
{

    public static bool IsNotComplete(this ONBOARDING onboardingStep)
    {
        return !SaveManager.THIS.SKIP_ONBOARDING && SaveManager.THIS.saveData.onboardingList[((int)onboardingStep)];
    }
    public static bool IsComplete(this ONBOARDING onboardingStep)
    {
        return SaveManager.THIS.SKIP_ONBOARDING || !SaveManager.THIS.saveData.onboardingList[((int)onboardingStep)];
    }
    public static void SetComplete(this ONBOARDING onboardingStep)
    {
        SaveManager.THIS.saveData.onboardingList[((int)onboardingStep)] = false;
        
        AnalyticsManager.OnboardingStepComplete(onboardingStep.ToString());
    }
    public static void ClearStep(this ONBOARDING onboardingStep)
    {
        SaveManager.THIS.saveData.onboardingList[((int)onboardingStep)] = true;
    }
    public static Pool RandomBlock(this Spawner spawner)
    {
        return SaveManager.THIS.saveData.userData.blockShopData.GetRandomBlock();
    }
    public static int CurrentLevel(this LevelManager levelManager)
    {
        return SaveManager.THIS.saveData.userData.level;
    }
    public static int NextLevel(this LevelManager levelManager)
    {
        return ++SaveManager.THIS.saveData.userData.level;
    }
}
public partial class SaveData
{
    [SerializeField] public bool saveGenerated = false;
    [SerializeField] public bool[] onboardingList;
    [SerializeField] public Account.Data accountData;
    [SerializeField] public User.Data userData;
    [SerializeField] public Player.Data playerData;
    [SerializeField] public AdManager.Data adData;
    [SerializeField] public OfferScreen.Data purchaseData;
    // [SerializeField] public Concent.Data concentData;
}


public static class Account
{
    public static Data Current => SaveManager.THIS.saveData.accountData;

    [System.Serializable]
    public class Data : ICloneable
    {
        [SerializeField] public bool commented = false;
        [SerializeField] public string guid;
        public Data(Data data)
        {
            commented = data.commented;
            guid = data.guid;
        }
        public Data()
        {
            commented = false;
            guid = System.Guid.NewGuid().ToString();
        }

        public object Clone()
        {
            return new Account.Data(this);
        }
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
    DRAG_AND_DROP,
    BLOCK_ROTATION,
    SPEECH_MERGE,
    SPEECH_CHEER,
    PASSIVE_META,
    ALL_BLOCK_STEPS,
    
    PIGGY_INVEST,
    PIGGY_BREAK,
    PIGGY_CONTINUE,
    
    BLOCK_TAB,
    WEAPON_TAB,
    
    PURCHASE_BLOCK,
    PURCHASE_FIRERATE,
    PURCHASE_WEAPON,
    
    USE_POWERUP,
    PLACE_POWERUP,
    
}
