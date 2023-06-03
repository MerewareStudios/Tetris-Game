using Internal.Core;
using System;
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

            int resourceCount = System.Enum.GetValues(typeof(MetaResource.Type)).Length;
            saveData.resourceInventory = new int[resourceCount];
        }

        
        ScoreBoard.OnLoad = () => { return saveData.score; };
        ScoreBoard.OnSave = (value) => { saveData.score = value; };
        
        //Meta.OnConvert = (@enum) => { return (int)Enum.Parse(typeof(MetaResource.Type), @enum.ToString()); };
        //Meta.OnLoad = (index) => { return saveData.resourceInventory[index]; };
        //Meta.OnSave = (index, amount) => { saveData.resourceInventory[index] = amount; };

        //Meta.THIS.SetUp(() => { return Enum.GetNames(typeof(MetaResource.Type)); });
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
}
public partial class SaveData
{
    [SerializeField] public bool saveGenerated = false;
    [SerializeField] public bool[] onboardingList;
    [SerializeField] public float playTime;
    [SerializeField] public int[] resourceInventory;
    [SerializeField] public int score;
}

public enum ONBOARDING
{
    TEMP_STEP,
}