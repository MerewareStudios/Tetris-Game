using Internal.Core;
using System;
using System.Collections.Generic;
using Game;
using Game.UI;
using IWI;
using UnityEngine;
using UnityEngine.Serialization;

public class SaveManager : SaveManagerBase<SaveManager>
{
    [SerializeField] public Const Const;
    [SerializeField] public AnimConst AnimConst;
    [SerializeField] public bool SKIP_ONBOARDING = true;

    public override void Awake()
    {
        base.Awake();

        Const.THIS = this.Const;
        AnimConst.THIS = this.AnimConst;

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
        Wallet.GEM.Set(ref saveData.userData.gemTransactionData);
        Wallet.AD.Set(ref saveData.userData.adTransactionData);

        AdManager.THIS._Data = saveData.adData;
        
        UIManager.THIS.shopBar.Set(ref saveData.userData.shopFillTransactionData);
        
        BlockMenu.THIS.Set(ref saveData.userData.blockShopData);
        WeaponMenu.THIS.Set(ref saveData.userData.weaponShopData);
        PiggyMenu.THIS._Data = saveData.userData.piggyData;
        UpgradeMenu.THIS._Data = saveData.userData.upgradeMenuData;

        Warzone.THIS.Player._Data = saveData.playerData;
        Board.THIS._Data = saveData.userData.boardData;

        MenuNavigator.THIS._Data = saveData.userData.menuNavData;
        
    }
}
public static class SaveManagerExtensions
{
    public static bool IsNotComplete(this ONBOARDING onboardingStep)
    {
        return !SaveManager.THIS.SKIP_ONBOARDING && SaveManager.THIS.saveData.onboardingList[((int)onboardingStep)];
    }
    public static void SetComplete(this ONBOARDING onboardingStep)
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
    public static int[] GetLookUp(this Block block, Pool pool)
    {
        return SaveManager.THIS.saveData.userData.blockShopData.LookUps(pool);
    }
    public static int CurrentLevel(this LevelManager levelManager)
    {
        return SaveManager.THIS.saveData.userData.level;
    }
    public static int NextLevel(this LevelManager levelManager)
    {
        return SaveManager.THIS.saveData.userData.level;
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
        [SerializeField] public Board.Data boardData;
        [SerializeField] public TransactionData<int> coinTransactionData = new();
        [SerializeField] public TransactionData<int> gemTransactionData = new();
        [SerializeField] public TransactionData<int> adTransactionData = new();
        [SerializeField] public TransactionData<float> shopFillTransactionData = new();
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
            boardData = data.boardData.Clone() as Board.Data;
            
            coinTransactionData = data.coinTransactionData.Clone() as TransactionData<int>;
            gemTransactionData = data.gemTransactionData.Clone() as TransactionData<int>;
            adTransactionData = data.adTransactionData.Clone() as TransactionData<int>;
            
            shopFillTransactionData = data.shopFillTransactionData.Clone() as TransactionData<float>;
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
    TEMP_STEP,
}
