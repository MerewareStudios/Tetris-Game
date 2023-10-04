using System;
using DG.Tweening;
using Game;
using Game.UI;
using IWI;
using TMPro;
using UnityEngine;


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
    
    [Header("Colors")] 
    public Color defaultColor;
    public Color steadyColor;
    public Color mergerMaxColor;
    public Color mergerColor;
    public Color powerColor;
    public Color ghostNormal;
    public Color ghostFade;
    public Sprite[] pawnIcons;
    public Color[] placeColorsDouble;
    public Vector3[] placePosDouble;
    public bool[] ghostPawnStateDouble;
    [Header("Meta Settings")] 
    public Material[] metaTextMaterials;
    public Color[] metaTextColors;
    [Header("Button Textures")] 
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
    public enum CurrencyType
    {
        Coin,
        PiggyCoin,
        Ticket,
        Dollar,
    }

    public void SetCurrencyColor(TextMeshProUGUI text, CurrencyType overridenCurrencyType)
    {
        int enumInt = (int)overridenCurrencyType;

        text.color = Const.THIS.metaTextColors[enumInt];
        text.fontSharedMaterial = Const.THIS.metaTextMaterials[enumInt];
    }
}