using System;
using DG.Tweening;
using Game;
using Game.UI;
using Internal.Core;
using RootMotion;
using TMPro;
using UnityEngine;


[CreateAssetMenu(fileName = "Game Const", menuName = "Game/Const", order = 0)]
public class Const : SSingleton<Const>
{
    
#if CREATIVE
    [Header("Creative Settings")]
    [SerializeField] public CreativeSettings creativeSettings;

    [Serializable]
    public class CreativeSettings
    {
        [SerializeField] public bool fingerEnabled = true;
        [SerializeField] public bool customSize = false;
        [ShowIf("customSize", true)]
        [SerializeField] public Vector2Int boardSize = new Vector2Int(5, 5);
        [SerializeField] public bool airplane = false;
        [SerializeField] public bool nextBlockEnabled = false;
        [SerializeField] public bool assignFurthest = true;
        [SerializeField] public bool shopEnabled = true;
        [SerializeField] public bool powerUpEnabled = true;
        [SerializeField] public bool coinEnabled = true;
        [SerializeField] public bool gemEnabled = true;
        [SerializeField] public bool ticketEnabled = true;
        [SerializeField] public bool settingsEnabled = false;
        [SerializeField] public bool statsEnabled = false;
        [SerializeField] public bool levelTextEnabled = false;
        [SerializeField] public bool showTip = false;
        [SerializeField] public bool adsEnabled = false;
        [SerializeField] public bool clearOnStart = true;
        [SerializeField] public int ammoMult = 4;
        [SerializeField] public Vector3 distanceFromDraggingFinger;
        [SerializeField] public float bottomOffset;
        [SerializeField] public float addedFov;
        [SerializeField] public Vector3 addedCamAngle;
        [SerializeField] public int seed;
        [SerializeField] public int giftIndex;
        [SerializeField] public Vector2Int[] poses;
        [SerializeField] public bool randomBlock = true;
        [SerializeField] public bool playerBubble = true;
        [SerializeField] public Pool[] blocks;
        [TextArea] [SerializeField] public string helpText;
        [SerializeField] public bool canSpeak = false    ;
        [SerializeField] public float speechDelay = 1.0f;
        [SerializeField] public float firstBlockSpawnDelay = 5.0f;
        [SerializeField] public float genericBlockSpawnDelay = 5.0f;

        public void Speak()
        {
            if (speechDelay <= 0.0f)
            {
                return;
            }
            DOVirtual.DelayedCall(speechDelay, () =>
            {
                UIManager.THIS.speechBubble.Speak(helpText, 0.0f, 1.1f);
            });
        }
    }
#endif
    
    [Header("Look Up")]
    public LevelSo[] Levels;
    public LevelSo spareLevelSo;
    // public LevelSo[] AutoLevels;
    public SaveData DefaultSaveData;
    [Header("Default Lookup")]
    public Gun.UpgradeData[] GunUpgradeData;
    public BlockMenu.BlockData[] DefaultBlockData;

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

    // public LevelSo GetAutoLevel(int index)
    // {
        // return AutoLevels[index];
    // }
    // public LevelSo GetRandomAutoLevel()
    // {
    //     return AutoLevels.Random();
    // }
    public LevelSo GetModLevel(int level)
    {
        int modded = (level - 1) % Levels.Length;
        return modded == 0 ? spareLevelSo : Levels[modded];
    }

    public int MaxLevel => Levels.Length;
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