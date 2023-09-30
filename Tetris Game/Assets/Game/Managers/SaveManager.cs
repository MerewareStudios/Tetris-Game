using Internal.Core;
using System;
using Game;
using Game.UI;
using IWI;
using UnityEngine;

public class SaveManager : SaveManagerBase<SaveManager>
{
    [SerializeField] public Const Const;
    [SerializeField] public AnimConst AnimConst;
    [SerializeField] public Onboarding Onboarding;
    [SerializeField] public bool SKIP_ONBOARDING = true;

    public override void Awake()
    {
        base.Awake();

        Const.THIS = this.Const;
        AnimConst.THIS = this.AnimConst;
        Onboarding.THIS = this.Onboarding;

        if (!saveData.saveGenerated)
        {
            //"Save Generated".LogW();
            saveData.saveGenerated = true;

            int onboardingCount = System.Enum.GetValues(typeof(ONBOARDING)).Length;
            saveData.onboardingList = new bool[onboardingCount].Fill(true);

            saveData.playerData = Const.THIS.DefaultPlayerData.Clone() as Player.Data;
            saveData.userData = Const.THIS.DefaultUserData.Clone() as User.Data;
            saveData.adData = Const.THIS.DefaultAdData.Clone() as AdManager.Data;
        }
        

        Wallet.COIN.Set(ref saveData.userData.coinTransactionData);
        Wallet.COIN.Active = ONBOARDING.LEARNED_META.IsComplete();
        Wallet.PIGGY.Set(ref saveData.userData.gemTransactionData);
        Wallet.PIGGY.Active = ONBOARDING.LEARNED_META.IsComplete();
        Wallet.TICKET.Set(ref saveData.userData.adTransactionData);
        Wallet.TICKET.Active = ONBOARDING.LEARNED_META.IsComplete();

        AdManager.THIS._Data = saveData.adData;
        
        UIManager.THIS.shop._Data = saveData.userData.shopData;
        
        Powerup.THIS._Data = saveData.userData.pupData;
        BlockMenu.THIS._Data = saveData.userData.blockShopData;
        WeaponMenu.THIS._Data = saveData.userData.weaponShopData;
        PiggyMenu.THIS._Data = saveData.userData.piggyData;

        Warzone.THIS.Player._Data = saveData.playerData;
        Board.THIS._Data = saveData.userData.boardData;

        MenuNavigator.THIS._Data = saveData.userData.menuNavData;
        
        UIManager.THIS.levelText.enabled = ONBOARDING.LEARNED_LEVEL_VISUALS.IsComplete();
        UIManager.THIS.levelProgressbar.SetActive(ONBOARDING.LEARNED_LEVEL_VISUALS.IsComplete());
        UIManager.THIS.shop.BarEnabled = ONBOARDING.EARN_SHOP_POINT.IsComplete();
    }

    void Start()
    {
        UpgradeMenu.THIS._Data = saveData.userData.upgradeMenuData;
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
    }
    public static void AutoComplete(this ONBOARDING onboardingStep)
    {
        SaveManager.THIS.saveData.onboardingList[((int)onboardingStep)] = false;
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
    [SerializeField] public float playTime;
    [SerializeField] public Player.Data playerData;
    [SerializeField] public User.Data userData;
    [SerializeField] public AdManager.Data adData;

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
        [SerializeField] public Shop.Data shopData;
        [SerializeField] public BlockMenu.BlockShopData blockShopData;
        [SerializeField] public WeaponMenu.WeaponShopData weaponShopData;
        [SerializeField] public MenuNavigator.Data menuNavData;
        [SerializeField] public PiggyMenu.Data piggyData;
        [SerializeField] public UpgradeMenu.Data upgradeMenuData;

        
        public Data()
        {
            
        }
        public Data(Data data)
        {
            level = data.level;
            pupData = data.pupData.Clone() as Powerup.Data;
            
            boardData = data.boardData.Clone() as Board.Data;
            
            
            coinTransactionData = data.coinTransactionData.Clone() as TransactionData<int>;
            gemTransactionData = data.gemTransactionData.Clone() as TransactionData<int>;
            adTransactionData = data.adTransactionData.Clone() as TransactionData<int>;
            
            shopData = data.shopData.Clone() as Shop.Data;
            
            blockShopData = data.blockShopData.Clone() as BlockMenu.BlockShopData;
            weaponShopData = data.weaponShopData.Clone() as WeaponMenu.WeaponShopData;
            menuNavData = data.menuNavData.Clone() as MenuNavigator.Data;
            piggyData = data.piggyData.Clone() as PiggyMenu.Data;
            upgradeMenuData = data.upgradeMenuData.Clone() as UpgradeMenu.Data;
        }
       

        public object Clone()
        {
            return new User.Data(this);
        }
    } 
}


public enum ONBOARDING
{
    LEARNED_LEVEL_VISUALS,
    LEARNED_META,
    LEARNED_POWERUP,
    PLACE_POWERUP,
    INSPECT_HEART_DISPLAY,
    TEACH_PICK,
    TEACH_PLACEMENT,
    LEARN_ROTATION,
    TALK_ABOUT_MERGE,
    HAVE_MERGED,
    NEED_MORE_AMMO_SPEECH,
    ALL_BLOCK_STEPS,
    LEARN_TO_INVEST,
    LEARN_TO_BREAK,
    LEARN_TO_CONTINUE,
    EARN_SHOP_POINT,
    ABLE_TO_USE_WEAPON_TAB,
    ABLE_TO_USE_UPGRADE_TAB,
    LEARNED_ALL_TABS,
    LEARN_TO_PURCHASE_BLOCK,
    LEARN_TO_PURCHASE_FIRERATE,
    LEARN_TO_PURCHASE_WEAPON,
}
