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
    public SaveData DefaultSaveData;
    [Header("Default Lookup")]
    public Gun.UpgradeData[] GunUpgradeData;
    public BlockMenu.BlockData[] DefaultBlockData;
    [Header("Auto")]
    public LevelSo[] AutoLevels;

    public Pawn.Usage[] gifts;
    [Header("Colors")] 
    public Color steadyColor;
    public Gradient mergeGradient;
    public Color mergerMaxColor;
    public Color mergerColor;
    public Color ghostNormal;
    public Color ghostFade;
    public Color[] placeColorsDouble;
    public Vector3[] placePosDouble;
    public bool[] ghostPawnStateDouble;
    public Gradient powerEffectGradient;
    [Header("Meta Settings")] 
    public Material[] metaTextMaterials;
    public Color[] metaTextColors;
    [Header("Currency Visuals")] 
    public Color currenyButtonNormalColor;
    public Color currenyButtonFadedColor;
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
    [Header("Ad Break Icons")] 
    public Sprite earnTicketBackgroundImage;
    public Sprite skipAdBackgroundImage;


    public LevelSo GetLevelSo(int level)
    {
        if (level > Levels.Length)
        {
            Debug.Log(level);
            return LevelSo.AutoGenerate(level);
        }
        return Levels[level - 1];
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

        public static Currency OneAd = new Currency(CurrencyType.Ticket, 1);
    }
    
    [Serializable]
    public class PawnPlacement
    {
        [SerializeField] public Vector2Int boardSize;
        [SerializeField] public PawnPlacementData[] data;
    }
    
    [Serializable]
    public class PawnPlacementData
    {
        [SerializeField] public Board.PawnPlacement[] PawnPlacements;
    }
    
    [Serializable]
    public enum CurrencyType
    {
        Coin,
        Gem,
        Ticket,
        Local,
    }

    public LevelSo GetAutoLevel(int index)
    {
        return AutoLevels[index % AutoLevels.Length];
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
    public static Sprite PowerUpIcon(this Pawn.Usage usage)
    {
        return Const.THIS.pawnVisualData[(int)usage].powerUpIcon; 
    }
    public static int ExtraValue(this Pawn.Usage usage)
    {
        return Const.THIS.pawnVisualData[(int)usage].externValue; 
    }
    public static Sprite Icon(this Pawn.Usage usage)
    {
        return Const.THIS.pawnVisualData[(int)usage].powerUpIcon; 
    }
    public static bool IsLocal(this Const.CurrencyType currencyType)
    {
        return currencyType.Equals(Const.CurrencyType.Local); 
    }
}