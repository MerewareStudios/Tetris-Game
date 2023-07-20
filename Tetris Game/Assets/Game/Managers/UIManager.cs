using System;
using DG.Tweening;
using Game;
using Game.UI;
using Internal.Core;
using TMPro;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
   [Header("Bars")]
   [SerializeField] public ShopBar shopBar;
   [SerializeField] public LoanBar loanBar;
   [Header("Flying Text")]
   [SerializeField] public FlyingText ft_Level;
   [SerializeField] public FlyingText ft_Combo;
   [SerializeField] public FlyingText ft_Icon;
   [SerializeField] public FlyingText ft_Icon_MenuOnTop;
   [Header("Level")]
   [SerializeField] public TextMeshProUGUI levelText;
   [System.NonSerialized] public static string NO_FUNDS_TEXT = "NO FUNDS";
   [System.NonSerialized] public static bool MENU_VISIBLE = false;
   [Header("Transactors")]
   [SerializeField] public CurrencyTransactor coin;
   [SerializeField] public CurrencyTransactor gem;
   [SerializeField] public CurrencyTransactor ad;
   [System.NonSerialized] public static System.Action<bool> OnMenuModeChanged;


   // Make them info list

    void Awake()
   {
      Wallet.CurrencyTransactors = new[] { Wallet.COIN, Wallet.GEM, Wallet.AD };
      
      ft_Level.OnGetInstance = () => Pool.Flying_Text___Level.Spawn<TextMeshProUGUI>();
      ft_Level.ReturnInstance = (mono) => { mono.Despawn(); };
      
      ft_Combo.OnGetInstance = () => Pool.Flying_Text___Combo.Spawn<TextMeshProUGUI>();
      ft_Combo.ReturnInstance = (mono) => { mono.Despawn(); };
      
      ft_Icon.OnGetInstance = () => Pool.Flying_Text___Icon.Spawn<TextMeshProUGUI>();
      ft_Icon.ReturnInstance = (mono) => { mono.Despawn(); };
      
      ft_Icon_MenuOnTop.OnGetInstance = () => Pool.Flying_Text___Icon.Spawn<TextMeshProUGUI>();
      ft_Icon_MenuOnTop.ReturnInstance = (mono) => { mono.Despawn(); };
   }

#if UNITY_EDITOR
   private void Update()
   {
      if (Input.GetKeyDown(KeyCode.E))
      {
         PiggyMenu.THIS.Open();
      }
      if (Input.GetKeyDown(KeyCode.R))
      {
         OpenShop();
      }
      if (Input.GetKeyDown(KeyCode.Space))
      {
         Warzone.THIS.GiveShield(1);
      }
      if (Input.GetKeyDown(KeyCode.A))
      {
         LevelManager.THIS.OnVictory();
      }
      if (Input.GetKeyDown(KeyCode.F))
      {
         LevelManager.THIS.OnFail();
      }
   }
#endif

   public void OpenShop()
   {
      MenuNavigator.THIS.Open();
   }

   

   public static void MenuMode(bool value)
   {
      MENU_VISIBLE = value;
      Time.timeScale = value ? 0.0f : 1.0f;
      
      OnMenuModeChanged?.Invoke(value);
   }
}

public static class UIManagerExtensions
{
   public static void LerpHearth(this FlyingText flyingText, Vector3 worldStart, float delay = 0.0f, float duration = 1.0f, System.Action endAction = null)
   {
      Vector3 viewPort = CameraManager.THIS.gameCamera.WorldToViewportPoint(worldStart);
      Vector3 screenStart = CameraManager.THIS.uiCamera.ViewportToWorldPoint(viewPort);

      Vector3 screenEnd = StatDisplayArranger.THIS.ScreenPosition(StatDisplay.Type.Health);
      
      flyingText.LerpScreen("Heart".ToTMProKey(), screenStart, screenEnd, delay, duration, false, endAction);
   }
   public static void LerpShield(this FlyingText flyingText, Vector3 worldStart, float delay = 0.0f, float duration = 1.0f, System.Action endAction = null)
   {
      Vector3 viewPort = CameraManager.THIS.gameCamera.WorldToViewportPoint(worldStart);
      Vector3 screenStart = CameraManager.THIS.uiCamera.ViewportToWorldPoint(viewPort);

      Vector3 viewPortPlayer = CameraManager.THIS.gameCamera.WorldToViewportPoint(Warzone.THIS.Player.shiledTarget.position);
      Vector3 screenEnd = CameraManager.THIS.uiCamera.ViewportToWorldPoint(viewPortPlayer);
      
      flyingText.LerpScreen("Shield".ToTMProKey(), screenStart, screenEnd, delay, duration, false, endAction);
   }
   public static void LerpXP(this FlyingText flyingText, Vector3 worldStart, float delay = 0.0f, float duration = 1.0f, System.Action endAction = null)
   {
      Vector3 viewPort = CameraManager.THIS.gameCamera.WorldToViewportPoint(worldStart);
      Vector3 screenStart = CameraManager.THIS.uiCamera.ViewportToWorldPoint(viewPort);

      Vector3 screenEnd = UIManager.THIS.shopBar.fill.transform.position;
      
      flyingText.LerpScreen("XP".ToTMProKey(), screenStart, screenEnd, delay, duration, false, endAction);
   }
   public static Sequence DragCoin(Vector3 screenStart, Vector3 screenDrag, Vector3 screenEnd, float duration = 0.0f, System.Action endAction = null)
   {
      return UIManager.THIS.ft_Icon_MenuOnTop.DragScreen(Const.CurrencyType.Coin.ToTMProKey(), screenStart, screenDrag, screenEnd, duration, true, endAction);
   }
   public static void EarnCurrencyWorld(Const.CurrencyType currencyType, Vector3 worldStart, float scale, System.Action endAction = null)
   {
      Vector3 viewPort = CameraManager.THIS.gameCamera.WorldToViewportPoint(worldStart);
      Vector3 screenStart = CameraManager.THIS.uiCamera.ViewportToWorldPoint(viewPort);

      EarnCurrencyScreen(currencyType, screenStart, scale, endAction);
   }
   
   public static void EarnCurrencyScreen(Const.CurrencyType currencyType, Vector3 screenStart, float scale, System.Action endAction = null)
   {
      Vector3 screenEnd = Wallet.IconPosition(currencyType);
      
      UIManager.THIS.ft_Icon_MenuOnTop.CurrencyLerp(currencyType.ToTMProKey(), screenStart, screenEnd, scale, true, endAction);
   }
}
