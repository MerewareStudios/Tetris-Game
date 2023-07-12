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
    public Color mergerPlaceColor;
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

    [Header("Time")]
    public float loanBarInterval = 30.0f;
    public float loanBarProtectionInterval = 10.0f;
    
    [Serializable]
    public struct DirectionRadiusPair
    {
        [SerializeField] public Vector3 dir;
        [SerializeField] public float radius;
    }
    [Serializable]
    public enum PurchaseType
    {
        Coin,
        Gem,
        Ad,
    }

    public static void SetPriceStamp(TextMeshProUGUI priceText, PurchaseType purchaseType, int amount)
    {
        int enumInt = (int)purchaseType;
        
        priceText.text = purchaseType.ToTMProKey() + (amount == 0 ? "" : amount);
        priceText.color = Const.THIS.metaTextColors[enumInt];
        priceText.fontSharedMaterial = Const.THIS.metaTextMaterials[enumInt];
    }
    public static void SetMaxStamp(TextMeshProUGUI priceText, PurchaseType purchaseType)
    {
        int enumInt = (int)purchaseType;
        
        priceText.text = "MAX";
        priceText.color = Const.THIS.metaTextColors[enumInt];
        priceText.fontSharedMaterial = Const.THIS.metaTextMaterials[enumInt];
    }
    public static void SetPriceStamp(TextMeshProUGUI priceText, Button button, TextMeshProUGUI purchaseButtonText, bool able2Purchase, PurchaseType purchaseType, int amount)
    {
        int enumInt = (int)purchaseType;
        
        priceText.text = purchaseType.ToTMProKey() + (amount == 0 ? "" : amount);
        priceText.color = Const.THIS.metaTextColors[enumInt];
        priceText.fontSharedMaterial = Const.THIS.metaTextMaterials[enumInt];
        
        purchaseButtonText.text = able2Purchase ? (purchaseType.Equals(PurchaseType.Ad) ? "FREE" :  "SPEND") : UIManager.NO_FUNDS_TEXT;

        
        button.image.sprite = Const.THIS.purchaseOptionSprites[enumInt];
    }
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
    
    public static void Stamp(this TextMeshProUGUI priceText, Const.PurchaseType purchaseType, int amount)
    {
        Const.SetPriceStamp(priceText, purchaseType, amount);
    }
    public static void StampMax(this TextMeshProUGUI priceText)
    {
        Const.SetMaxStamp(priceText, Const.PurchaseType.Ad);
    }
}