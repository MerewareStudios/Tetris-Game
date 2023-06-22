using System;
using Game;
using Game.UI;
using Internal.Core;
using Internal.Visuals;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class UIManager : Singleton<UIManager>
{
   [Header("Flying Text")]
   [SerializeField] public FlyingText ft_TF2;
   [SerializeField] public FlyingText ft_Combo;
   [SerializeField] public FlyingText ft_Icon;

   void Awake()
   {
      ft_TF2.OnGetInstance = () => Pool.Flying_Text___TF2.Spawn<TextMeshProUGUI>();
      ft_TF2.ReturnInstance = (mono) => { mono.Despawn(); };
      
      ft_Combo.OnGetInstance = () => Pool.Flying_Text___Combo.Spawn<TextMeshProUGUI>();
      ft_Combo.ReturnInstance = (mono) => { mono.Despawn(); };
      
      ft_Icon.OnGetInstance = () => Pool.Flying_Text___Icon.Spawn<TextMeshProUGUI>();
      ft_Icon.ReturnInstance = (mono) => { mono.Despawn(); };
   }

   public void OpenShop()
   {
      // blockMenu.Open();
      WeaponMenu.THIS.Open();
   }
}

public static class UIManagerExtensions
{
   public static void LerpHearth(this FlyingText flyingText, Vector3 worldStart, float delay = 0.0f, float duration = 1.0f, System.Action endAction = null)
   {
      Vector3 viewPort = CameraManager.THIS.gameCamera.WorldToViewportPoint(worldStart);
      Vector3 screenStart = CameraManager.THIS.uiCamera.ViewportToWorldPoint(viewPort);

      Vector3 screenEnd = StatDisplayArranger.THIS.ScreenPosition(StatDisplay.Type.Health);
      
      flyingText.LerpScreen("Heart".ToTMProKey(), screenStart, screenEnd, delay, duration, endAction);
   }
   public static void LerpShield(this FlyingText flyingText, Vector3 worldStart, float delay = 0.0f, float duration = 1.0f, System.Action endAction = null)
   {
      Vector3 viewPort = CameraManager.THIS.gameCamera.WorldToViewportPoint(worldStart);
      Vector3 screenStart = CameraManager.THIS.uiCamera.ViewportToWorldPoint(viewPort);

      Vector3 viewPortPlayer = CameraManager.THIS.gameCamera.WorldToViewportPoint(Warzone.THIS.Player.shiledTarget.position);
      Vector3 screenEnd = CameraManager.THIS.uiCamera.ViewportToWorldPoint(viewPortPlayer);
      
      flyingText.LerpScreen("Shield".ToTMProKey(), screenStart, screenEnd, delay, duration, endAction);
   }
}
