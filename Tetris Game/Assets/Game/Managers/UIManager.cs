using System;
using DG.Tweening;
using Game;
using Game.UI;
using Internal.Core;
using Internal.Visuals;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class UIManager : Singleton<UIManager>
{
   
   [SerializeField] public ShopBar shopBar;
   [Header("Flying Text")]
   [SerializeField] public FlyingText ft_Level;
   [SerializeField] public FlyingText ft_Combo;
   [SerializeField] public FlyingText ft_Icon;
   [SerializeField] public FlyingText ft_Icon_MenuOnTop;
   [Header("Level")]
   [SerializeField] public TextMeshProUGUI levelText;
   [System.NonSerialized] public static string COIN_TEXT = "<sprite name=Coin>";
   [System.NonSerialized] public static string GEM_TEXT = "<sprite name=Gem>";
   [System.NonSerialized] public static string AD_TEXT = "<sprite name=AD>";
   [System.NonSerialized] public static string NO_FUNDS_TEXT = "NO FUNDS";

   
   
   void Awake()
   {
      ft_Level.OnGetInstance = () => Pool.Flying_Text___Level.Spawn<TextMeshProUGUI>();
      ft_Level.ReturnInstance = (mono) => { mono.Despawn(); };
      
      ft_Combo.OnGetInstance = () => Pool.Flying_Text___Combo.Spawn<TextMeshProUGUI>();
      ft_Combo.ReturnInstance = (mono) => { mono.Despawn(); };
      
      ft_Icon.OnGetInstance = () => Pool.Flying_Text___Icon.Spawn<TextMeshProUGUI>();
      ft_Icon.ReturnInstance = (mono) => { mono.Despawn(); };
      
      ft_Icon_MenuOnTop.OnGetInstance = () => Pool.Flying_Text___Icon.Spawn<TextMeshProUGUI>();
      ft_Icon_MenuOnTop.ReturnInstance = (mono) => { mono.Despawn(); };
   }

   private void Update()
   {
      if (Input.GetKeyDown(KeyCode.E))
      {
         EndLevelScreen.THIS.Open();
      }
      if (Input.GetKeyDown(KeyCode.R))
      {
         OpenShop();
      }
   }

   public void OpenShop()
   {
      MenuNavigator.THIS.Open();
   }

   public void ScaleTransactors(float scale, bool distance = false)
   {
      Wallet.COIN.Scale(scale, distance);
      Wallet.GEM.Scale(scale, distance);
   }
}

public static class UIManagerExtensions
{
   public static string CoinAmount(this int amount)
   {
      return UIManager.COIN_TEXT + amount;
   }
 
   
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
      return UIManager.THIS.ft_Icon_MenuOnTop.DragScreen(UIManager.COIN_TEXT, screenStart, screenDrag, screenEnd, duration, true, endAction);
   }
   
}
