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
    public GunSo[] Guns;
    public Pawn.Usage[] PowerUps;
    
    
    [Header("Defaults")]
    public Player.Data DefaultPlayerData;
    public User.Data DefaultUserData;
    public AdManager.Data DefaultAdData;
    public Gun.UpgradeData[] GunUpgradeData;
    
    [Header("Colors")] 
    public Color defaultColor;
    public Color steadyColor;
    public Color mergerColor;
    public Color[] placeColors;
    public Color shooterPlaceColor;
    public Color[] enemyColors; // enemy colors
    public Color singleColor;
    public Color comboColor;
    public Gradient frontLineGradient;
    public Color piggyExplodeColor;
    [Header("Meta Settings")] 
    public Material[] metaTextMaterials;
    public Color[] metaTextColors;
    [Header("Images")] 
    public Sprite[] purchaseOptionSprites;
    [Header("Purchase Option")] 
    public Color defaultFrameColor;
    public Color acceptedFrameColor;
    public Color deniedFrameColor;
    
    [Header("Visuals")] 
    public DirectionRadiusPair[] rewardDirectionRadiusPair;
    
    [Header("Values")] 
    public float rotationDuration = 0.15f;
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
    }
    
    [Serializable]
    public enum CurrencyType
    {
        Coin,
        PiggyCoin,
        Ad,
    }

    public void SetCurrencyColor(TextMeshProUGUI text, CurrencyType overridenCurrencyType)
    {
        int enumInt = (int)overridenCurrencyType;

        text.color = Const.THIS.metaTextColors[enumInt];
        text.fontSharedMaterial = Const.THIS.metaTextMaterials[enumInt];
    }

    // public static void SetPriceStamp(TextMeshProUGUI priceText, CurrencyData currencyData)
    // {
    //     int enumInt = (int)currencyData.type;
    //     
    //     priceText.text = currencyData.type.ToTMProKey() + (amount == 0 ? "" : amount);
    //     priceText.color = Const.THIS.metaTextColors[enumInt];
    //     priceText.fontSharedMaterial = Const.THIS.metaTextMaterials[enumInt];
    // }
    // public static void SetMaxStamp(TextMeshProUGUI priceText, Currency currency)
    // {
    //     int enumInt = (int)currency;
    //     
    //     priceText.text = "MAX";
    //     priceText.color = Const.THIS.metaTextColors[enumInt];
    //     priceText.fontSharedMaterial = Const.THIS.metaTextMaterials[enumInt];
    // }
    // public static void SetPriceStamp(TextMeshProUGUI priceText, Button button, TextMeshProUGUI purchaseButtonText, bool able2Purchase, Currency currency, int amount)
    // {
    //     int enumInt = (int)currency;
    //     
    //     priceText.text = currency.ToTMProKey() + (amount == 0 ? "" : amount);
    //     priceText.color = Const.THIS.metaTextColors[enumInt];
    //     priceText.fontSharedMaterial = Const.THIS.metaTextMaterials[enumInt];
    //     
    //     purchaseButtonText.text = able2Purchase ? (currency.Equals(Currency.Ad) ? "FREE" :  "SPEND") : UIManager.NO_FUNDS_TEXT;
    //
    //     
    //     button.image.sprite = Const.THIS.purchaseOptionSprites[enumInt];
    // }
}

public static class ConstExtensions
{
    public static Color Health2Color(this int health)
    {
        return Const.THIS.enemyColors[health - 1];
    } 
    public static Vector3 Direction(this int rewardCount)
    {
        return Const.THIS.rewardDirectionRadiusPair[rewardCount - 1].dir;
    } 
    public static float Radius(this int rewardCount)
    {
        return Const.THIS.rewardDirectionRadiusPair[rewardCount - 1].radius;
    }
    
    // public static void Stamp(this TextMeshProUGUI priceText, Const.Currency currency, int amount)
    // {
    //     Const.SetPriceStamp(priceText, currency, amount);
    // }
    // public static void StampMax(this TextMeshProUGUI priceText)
    // {
    //     Const.SetMaxStamp(priceText, Const.Currency.Ad);
    // }
}