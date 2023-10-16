using System;
using DG.Tweening;
using Game;
using Game.UI;
using Internal.Core;
using IWI;
using TMPro;
using UnityEngine;


[CreateAssetMenu(fileName = "Game Const", menuName = "Game/Const", order = 0)]
public class Const : SSingleton<Const>
{
    [ReadOnly] public int bundleVersionCode;
    [Header("Look Up")]
    public LevelSo[] Levels;
    // public Pawn.Usage[] PowerUps;
    
    
    [Header("Save Default")]
    public Player.Data DefaultPlayerData;
    public User.Data DefaultUserData;
    public AdManager.Data DefaultAdData;
    [Header("Default Lookup")]
    public Gun.UpgradeData[] GunUpgradeData;
    public BlockMenu.BlockData[] DefaultBlockData;
    public UpgradeMenu.PurchaseDataLookUp[] purchaseDataLookUp;
    
    [Header("Colors")] 
    public Color defaultColor;
    public Color steadyColor;
    public Color mergerMaxColor;
    public Color mergerColor;
    public Color powerColor;
    public Color ghostNormal;
    public Color ghostFade;
    public Color[] placeColorsDouble;
    public Vector3[] placePosDouble;
    public bool[] ghostPawnStateDouble;
    [Header("Meta Settings")] 
    public Material[] metaTextMaterials;
    public Color[] metaTextColors;
    [Header("Button Textures")] 
    public Sprite[] buttonTextures;
    public Sprite getButtonTexture;
    public Sprite buyButtonTexture;
    public Sprite watchButtonTexture;
    [Header("Currency Visuals")] 
    public Color currenyButtonNormalColor;
    public Color currenyButtonFadedColor;
    [Header("Purchase Option")] 
    public Color defaultFrameColor;
    public Color acceptedFrameColor;
    public Color deniedFrameColor;
    [Header("Hit")] 
    public Gradient hitGradient;
    public AnimationCurve hitScaleCurve;
    
    [Header("Values")] 
    public float jumpDuration = 0.15f;
    public Vector3 jumpPower;
    public Ease rotationEase;
    
    [Header("Pawn")] 
    public Pawn.VisualData[] pawnVisualData;
    [Header("Tutorıal")] 
    public Color suggestionColorTut;
    public Color suggestionColor;
    
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

        public Currency ReduceCost(Const.CurrencyType specificType, float percent)
        {
            if (!type.Equals(specificType))
            {
                return this;
            }
            int reducedCost = amount.ReduceFloor(percent);
            return new Const.Currency(type, reducedCost);
        }

        public static Currency OneAd = new Currency(CurrencyType.Ticket, 1);
    }
    
    [Serializable]
    public enum CurrencyType
    {
        Coin,
        PiggyCoin,
        Ticket,
        Local,
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
    public static Pool Model(this Pawn.Usage usage)
    {
        return Const.THIS.pawnVisualData[(int)usage].model; 
    }
    // public static bool HoverOnMerge(this Pawn pawn)
    // {
    //     return Const.THIS.pawnVisualData[(int)pawn.UsageType].hoverOnMerge; 
    // }
    public static Sprite PowerUpIcon(this Pawn.Usage usage)
    {
        return Const.THIS.pawnVisualData[(int)usage].powerUpIcon; 
    }
}