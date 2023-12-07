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
    public EnemyData[] AutoGenerateEnemyDatas;
    public EnemyData[] AutoGenerateEnemySpanwerDatas;
    public PawnPlacement[] AutoGeneratePawnPlacements;

    public Pawn.Usage[] gifts;
    public Pawn.Usage[] powerUps;
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
    [Header("Ad Break Icons")] 
    public Sprite earnTicketBackgroundImage;
    public Sprite skipAdBackgroundImage;


    public LevelSo GetLevelSo(int level)
    {
        if (level >= Levels.Length)
        {
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

    public EnemyData GetRandomEnemyData()
    {
        return AutoGenerateEnemyDatas.Random();
    }
    public EnemyData GetRandomSpawnerEnemyData()
    {
        return AutoGenerateEnemySpanwerDatas.Random();
    }
    public void SetCurrencyColor(TextMeshProUGUI text, CurrencyType overridenCurrencyType)
    {
        int enumInt = (int)overridenCurrencyType;

        text.color = Const.THIS.metaTextColors[enumInt];
        text.fontSharedMaterial = Const.THIS.metaTextMaterials[enumInt];
    }

    public Board.PawnPlacement[] GetRandomPawnPlacement(Vector2Int boardSize)
    {
        if (Helper.IsPossible(0.5f))
        {
            return null;
        }
        return AutoGeneratePawnPlacements[boardSize.x - LevelSo.MinAutoWidth].data.Random().PawnPlacements;
    }
    
    // public void PrintLevelData()
    // {
    //     int accumReward = 0;
    //     for (int i = 0; i < Levels.Length; i++)
    //     {
    //         string colorTag;
    //         
    //         LevelSo levelSo = Levels[i];
    //         (string info, int totalReward) = levelSo.ToString(i + 1);
    //         accumReward += totalReward;
    //
    //         int accumDisplayed = accumReward;
    //         if ((i+1) % 5 == 0)
    //         {
    //             colorTag = "yellow";
    //             accumReward = 0;
    //         }
    //         else
    //         {
    //             colorTag = "cyan";
    //         }
    //         Debug.Log(info + " | <color=" + colorTag + ">Accum Reward : " + accumDisplayed + "</color>");
    //         
    //     }
    // }

    // public Sprite GetButtonSprite(CurrencyType currencyType)
    // {
    //     return buttonSprites[(int)currencyType];
    // }
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