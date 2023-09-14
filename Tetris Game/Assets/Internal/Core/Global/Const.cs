using System;
using DG.Tweening;
using Game;
using Game.UI;
using Internal.Core;
using IWI;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


[CreateAssetMenu(fileName = "Game Const", menuName = "Game/Const", order = 0)]
public class Const : SSingleton<Const>
{
    [Header("Look Up")]
    public LevelSo[] Levels;
    public Pawn.Usage[] PowerUps;
    
    
    [Header("Save Default")]
    public Player.Data DefaultPlayerData;
    public User.Data DefaultUserData;
    public AdManager.Data DefaultAdData;
    [Header("Default Lookup")]
    public Gun.UpgradeData[] GunUpgradeData;
    public BlockMenu.BlockData[] DefaultBlockData;
    public UpgradeMenu.PurchaseDataLookUp[] purchaseDataLookUp;
    
    [Header("Reward")] 
    public RewardData[] rewardDatas;
    [Header("Colors")] 
    public Color[] gridTileColors;
    public Color defaultColor;
    public Color steadyColor;
    public Color mergerColor;
    public Color[] placeColorsDouble;
    public Color piggyExplodeColor;
    [Header("Meta Settings")] 
    public Material[] metaTextMaterials;
    public Color[] metaTextColors;
    [Header("Button Textures")] 
    public Sprite getButtonTexture;
    public Sprite buyButtonTexture;
    [Header("Curreny Visuals")] 
    public Color currenyButtonNormalColor;
    public Color currenyButtonFadedColor;
    [Header("Purchase Option")] 
    public Color defaultFrameColor;
    public Color acceptedFrameColor;
    public Color deniedFrameColor;
    
    [Header("Values")] 
    public float jumpDuration = 0.15f;
    public Vector3 jumpPower;
    public Ease rotationEase;
    public Ease piggyExplodeEase;

    [Header("Shield Settings")]
    public float shieldMaxDuration = 30.0f;

    [Header("Ad Settings")] public AdSettings adSettings;
    
    [Serializable]
    public struct AdSettings
    {
        [SerializeField] public float loanBarProtectionInterval;
        [SerializeField] public int adBreakSkipTime;
        [SerializeField] public int mergePerAdBreak;
    }
    
    [Serializable]
    public struct RewardData
    {
        [SerializeField] public PiggyMenu.PiggyReward.Type type;
        [SerializeField] public Sprite backgroundSprite;
        [SerializeField] public Sprite iconSprite;
        [TextArea][SerializeField] public string formatText;
        [TextArea][SerializeField] public string title;
        [SerializeField] public Color color;
    }
    
    [Serializable]
    public struct DirectionRadiusPair
    {
        [SerializeField] public Vector3 dir;
        [SerializeField] public float radius;
    }
    
    [Serializable]
    public struct Currency
    {
        [SerializeField] public CurrencyType type;
        [SerializeField] public int amount;

        public Currency(CurrencyType type, int amount)
        {
            this.type = type;
            this.amount = amount;
        }

        public static Currency OneAd = new Currency(CurrencyType.Ad, 1);
        public static Currency OneAdConsume = new Currency(CurrencyType.Ad, -1);
        
        
        // public static Currency operator +(Currency currency, int amount)
        // {
        //     currency.amount += amount;
        //     return currency;
        // }
        //
        // public static Currency operator -(Currency currency, int amount)
        // {
        //     currency.amount -= amount;
        //     return currency;
        // }
    }
    
    [Serializable]
    public enum CurrencyType
    {
        Coin,
        PiggyCoin,
        Ad,
        Dollar,
    }

    public void SetCurrencyColor(TextMeshProUGUI text, CurrencyType overridenCurrencyType)
    {
        int enumInt = (int)overridenCurrencyType;

        text.color = Const.THIS.metaTextColors[enumInt];
        text.fontSharedMaterial = Const.THIS.metaTextMaterials[enumInt];
    }
}

public static class ConstExtensions
{
    public static Const.RewardData RewardData(this PiggyMenu.PiggyReward.Type type)
    {
        return Const.THIS.rewardDatas[(int)type];
    }
}