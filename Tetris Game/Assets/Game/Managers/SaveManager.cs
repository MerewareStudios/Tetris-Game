using Internal.Core;
using System;
using System.Collections.Generic;
using Game;
using Game.UI;
using UnityEngine;

public class SaveManager : SaveManagerBase<SaveManager>
{
    [SerializeField] public bool SKIP_ONBOARDING = true;
    public override void Awake()
    {
        base.Awake();

        if (!saveData.saveGenerated)
        {
            //"Save Generated".LogW();
            saveData.saveGenerated = true;

            int onboardingCount = System.Enum.GetValues(typeof(ONBOARDING)).Length;
            saveData.onboardingList = new bool[onboardingCount].Fill(true);

            saveData.playerData = Const.THIS.DefaultPlayerData.Clone() as Player.Data;
            saveData.userData = Const.THIS.DefaultUserData.Clone() as User.Data;
        }

        MoneyTransactor.THIS.Set(ref saveData.userData.moneyTransactionData);
        ShopBar.THIS.Set(ref saveData.userData.shopFillTransactionData);
        BlockMenu.THIS.Set(ref saveData.userData.blockShopData);

        Warzone.THIS.Player._Data = saveData.playerData;
    }
    void Update()
    {
        saveData.playTime += Time.deltaTime;
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
    public static int CurrentLevel(this LevelManager levelManager)
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
        [SerializeField] public TransactionData<int> moneyTransactionData = new();
        [SerializeField] public TransactionData<float> shopFillTransactionData = new();
        [SerializeField] public BlockMenu.BlockShopData blockShopData;

        
        public Data()
        {
            
        }
        public Data(Data data)
        {
            level = data.level;
            moneyTransactionData = data.moneyTransactionData.Clone() as TransactionData<int>;
            shopFillTransactionData = data.shopFillTransactionData.Clone() as TransactionData<float>;
            blockShopData = data.blockShopData.Clone() as BlockMenu.BlockShopData;
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
