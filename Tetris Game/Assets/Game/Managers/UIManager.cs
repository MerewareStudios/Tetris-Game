using Game;
using Game.UI;
using Internal.Core;
using IWI.Tutorial;
using IWI.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Visual.Effects;
using ValueType = IWI.Emitter.Enums.ValueType;

public class UIManager : Singleton<UIManager>
{
   [SerializeField] public TextMeshProUGUI levelText;
   [Header("Canvases")]
   [SerializeField] private BlockMenu blockMenu;
   [SerializeField] private WeaponMenu weaponMenu;
   [SerializeField] private UpgradeMenu upgradeMenu;
   [SerializeField] private SlashScreen slashScreen;
   [SerializeField] private MenuNavigator menuNavigator;
   [SerializeField] private PiggyMenu piggyMenu;
   [SerializeField] private RewardScreen rewardScreen;
   [SerializeField] private Powerup powerup;
   [SerializeField] private AdBreakScreen adBreakScreen;
   [Header("Bars")]
   [SerializeField] public Shop shop;
   [FormerlySerializedAs("particleImageCoin")]
   [Header("UI Emitter")]
   [SerializeField] public UIEmitter coinEmitter;
   [SerializeField] public UIEmitter heartEmitter;
   [SerializeField] public UIEmitter shieldEmitter;
   [SerializeField] public UIEmitter ticketEmitter;
   [SerializeField] public MotionData motionData_Block;
   [SerializeField] public MotionData motionData_Enemy;
   [SerializeField] public MotionData motionData_Enemy_Burst;
   [SerializeField] public MotionData motionData_LevelReward;
   [SerializeField] public MotionData motionData_PiggyFill;
   [SerializeField] public MotionData motionData_Shop;
   [SerializeField] public MotionData motionData_UpgradeBurst;
   [SerializeField] public MotionData motionData_Ticket;
   [Header("Level")]
   [System.NonSerialized] public static string NO_FUNDS_TEXT = "NO FUNDS";
   [System.NonSerialized] public static bool MENU_VISIBLE = false;
   [Header("Transactors")]
   [SerializeField] public CurrencyTransactor coin;
   [SerializeField] public CurrencyTransactor gem;
   [SerializeField] public CurrencyTransactor ticket;
   [System.NonSerialized] public static System.Action<bool> OnMenuModeChanged;
   [Header("Tutorial")]
   [SerializeField] public SpeechBubble speechBubble;
   [SerializeField] public Finger finger;
   [System.NonSerialized] public static IMenu currentMenu = null;


   // Make them info list

    void Awake()
    {
         BlockMenu.THIS = blockMenu;
         WeaponMenu.THIS = weaponMenu;
         UpgradeMenu.THIS = upgradeMenu;
         SlashScreen.THIS = slashScreen;
         MenuNavigator.THIS = menuNavigator.Setup();
         PiggyMenu.THIS = piggyMenu;
         RewardScreen.THIS = rewardScreen;
         Powerup.THIS = powerup;
         AdBreakScreen.THIS = adBreakScreen;

         RewardScreen.THIS.OnClose = () =>
         {
            MenuMode(false);
            Wallet.ScaleTransactors(1.0f);
            LevelManager.THIS.LoadLevel();
         };

         Wallet.CurrencyTransactors = new[] { Wallet.COIN, Wallet.PIGGY, Wallet.TICKET };

         Glimmer.OnComplete = glimmer => glimmer.Despawn();

         MENU_VISIBLE = false;
         currentMenu = null;
    }

#if UNITY_EDITOR
   private void Update()
   {
      if (Input.GetKeyDown(KeyCode.P))
      {
         PiggyMenu.THIS.Open();
      }
      if (Input.GetKeyDown(KeyCode.O))
      {
         PiggyMenu.THIS.GiveRewards();
      }
      if (Input.GetKeyDown(KeyCode.R))
      {
         MenuNavigator.THIS.Open();
      }
      if (Input.GetKeyDown(KeyCode.M))
      {
         Warzone.THIS.Player._CurrentHealth += 1;
      }
      if (Input.GetKeyDown(KeyCode.N))
      {
         Warzone.THIS.Player.shield.Add(1);
      }
      if (Input.GetKeyDown(KeyCode.G))
      {
         LevelManager.THIS.OnVictory();
      }
      if (Input.GetKeyDown(KeyCode.F))
      {
         LevelManager.THIS.OnFail();
      }
      if (Input.GetKeyDown(KeyCode.S))
      {
         shop.AnimatedShow();
      }
      if (Input.GetKeyDown(KeyCode.X))
      {
         if (ONBOARDING.LEARNED_POWERUP.IsNotComplete())
         {
            Powerup.THIS.Enabled = true;
            Onboarding.TalkAboutPowerUp();
         }
      }
      if (Input.GetKeyDown(KeyCode.B))
      {
         Pool.Cube_Explosion.Spawn<CubeExplosion>().Explode(Vector3.zero + new Vector3(0.0f, 0.3516f + 0.254f, 0.0f));
         UIManagerExtensions.Distort(Vector3.up * 0.45f, 0.0f);
      }
      
      if (Input.GetKeyDown(KeyCode.C))
      {
         if (Wallet.Consume(Const.Currency.OneAd))
         {
            UIManagerExtensions.RequestTicketFromWallet(Powerup.THIS.currencyTarget.position, 1, 1,
               (value) =>
               {
                  
               },
               () =>
               {
                  Powerup.THIS.OpenAnimated(true);
               });
         }
      }
      if (Input.GetKeyDown(KeyCode.T))
      {
         Wallet.TICKET.Transaction(1);
      }
      if (Input.GetKeyDown(KeyCode.H))
      {
         UIManagerExtensions.HeartToPlayer(Vector3.zero,  1, 1);
      }
   }
#endif

   public static void ForceUpdateAvailableMenu()
   {
      if (UIManager.currentMenu != null)
      {
         UIManager.currentMenu.Show();
      }
   }

   public static void MenuMode(bool value)
   {
      if (MENU_VISIBLE == value)
      {
         return;
      }

      CameraManager.THIS.gameCamera.enabled = !value;
      MENU_VISIBLE = value;
      Time.timeScale = value ? 0.0f : 1.0f;
      
      OnMenuModeChanged?.Invoke(value);
   }
   public static void Pause(bool value)
   {
      Time.timeScale = value ? 0.0f : 1.0f;
   }
}

public static class UIManagerExtensions
{
   public static void ShieldPs(Vector3 worldPosition)
   {
      Transform camTransform = CameraManager.THIS.gameCamera.transform;
      Vector3 pos = worldPosition + camTransform.forward * -2.0f + new Vector3(0.0f, 0.6f, 0.0f);
      Particle.Shield.Play(pos);
   }
   public static void Distort(Vector3 worldPosition, float delay)
   {
      if (!ApplicationManager.THIS.GrabFeatureEnabled)
      {
         ApplicationManager.THIS.GrabFeatureEnabled = true;
      }
      
      Transform camTransform = CameraManager.THIS.gameCamera.transform;
      var forward = camTransform.forward;

      Plane plane = new Plane(forward, camTransform.position);
      Ray ray = new Ray(worldPosition, -forward);

      Vector3 hitPoint = Vector3.zero;
      if (plane.Raycast(ray, out float enter))
      {
       hitPoint = ray.GetPoint(enter);
      }

      Pool.Distortion.Spawn<Distortion>().Distort(hitPoint, forward, AnimConst.THIS.distortScale, AnimConst.THIS.distortPower, AnimConst.THIS.distortDuration, AnimConst.THIS.distortEase, delay);
   }
   
   public static void Glimmer(this Image image, float speed)
   {
      Glimmer glimmer = Pool.Glimmer.Spawn<Glimmer>();
      RectTransform rectTransform = image.rectTransform;
      glimmer.Show(image, rectTransform, speed, AnimConst.THIS.glimmerEase);
   }

   // public static void EmitBlockCoin(Vector3 worldPosition, int count, int totalValue)
   // {
   //    TargetSettings targetSettingsStart = new TargetSettings(UIEmitter.Cam.Game, null, worldPosition);
   //    ValueSettings valueSettings = new ValueSettings(ValueType.TotalValue, totalValue);
   //    UIManager.THIS.coinEmitter.Emit(count, valueSettings, targetSettingsStart, null, UIManager.THIS.motionData_Block);
   // }
   // public static void EmitEnemyCoin(Vector3 worldPosition, int count, int totalValue)
   // {
   //    TargetSettings targetSettingsStart = new TargetSettings(UIEmitter.Cam.Game, null, worldPosition);
   //    ValueSettings valueSettings = new ValueSettings(ValueType.TotalValue, totalValue);
   //    UIManager.THIS.coinEmitter.Emit(count, valueSettings, targetSettingsStart, null, UIManager.THIS.motionData_Enemy);
   // }
   public static void EmitEnemyCoinBurst(Vector3 worldPosition, int count, int totalValue)
   {
      TargetSettings targetSettingsStart = new TargetSettings(UIEmitter.Cam.Game, null, worldPosition);
      ValueSettings valueSettings = new ValueSettings(ValueType.TotalValue, totalValue);
      UIManager.THIS.coinEmitter.Emit(count, valueSettings, targetSettingsStart, null, UIManager.THIS.motionData_Enemy_Burst);
   }
   public static void EmitLevelRewardCoin(Vector3 canvasWorldPosition, int count, int totalValue, System.Action OnAllArrive)
   {
      TargetSettings targetSettingsStart = new TargetSettings(UIEmitter.Cam.Game, null, canvasWorldPosition);
      ValueSettings valueSettings = new ValueSettings(ValueType.TotalValue, totalValue);
      UIManager.THIS.coinEmitter.Emit(count, valueSettings, targetSettingsStart, null, UIManager.THIS.motionData_LevelReward, null, OnAllArrive);
   }
   public static void RequestCoinFromWallet(Vector3 targetCanvasWorldPosition, int count, int totalValue, System.Action<int> OnArrive, System.Action OnAllArrive)
   {
      TargetSettings targetSettingsStart = new TargetSettings(UIEmitter.Cam.UI, null, Const.CurrencyType.Coin.IconPosition());
      TargetSettings targetSettingsEnd = new TargetSettings(UIEmitter.Cam.UI, null, targetCanvasWorldPosition);
      ValueSettings valueSettings = new ValueSettings(ValueType.TotalValue, totalValue);
      UIManager.THIS.coinEmitter.Emit(count, valueSettings, targetSettingsStart, targetSettingsEnd, UIManager.THIS.motionData_PiggyFill, OnArrive, OnAllArrive);
   }
   public static void EmitLevelShopCoin(Vector3 canvasWorldPosition, int count, int totalValue)
   {
      TargetSettings targetSettingsStart = new TargetSettings(UIEmitter.Cam.Game, null, canvasWorldPosition);
      ValueSettings valueSettings = new ValueSettings(ValueType.TotalValue, totalValue);
      UIManager.THIS.coinEmitter.Emit(count, valueSettings, targetSettingsStart, null, UIManager.THIS.motionData_Shop, null, null);
   }
   public static void HeartToPlayer(Vector3 worldPosition, int count, int totalValue)
   {
      TargetSettings targetSettingsStart = new TargetSettings(UIEmitter.Cam.Game, null, worldPosition);
      ValueSettings valueSettings = new ValueSettings(ValueType.TotalValue, totalValue);
      UIManager.THIS.heartEmitter.Emit(count, valueSettings, targetSettingsStart, null, UIManager.THIS.motionData_UpgradeBurst);
   }
   public static void ShieldToPlayer(Vector3 worldPosition, int count, int totalValue)
   {
      TargetSettings targetSettingsStart = new TargetSettings(UIEmitter.Cam.Game, null, worldPosition);
      ValueSettings valueSettings = new ValueSettings(ValueType.TotalValue, totalValue);
      UIManager.THIS.shieldEmitter.Emit(count, valueSettings, targetSettingsStart, null, UIManager.THIS.motionData_UpgradeBurst);
   }
   public static void RequestTicketFromWallet(Vector3 targetCanvasWorldPosition, int count, int totalValue, System.Action<int> OnArrive, System.Action OnAllArrive)
   {
      TargetSettings targetSettingsStart = new TargetSettings(UIEmitter.Cam.UI, null, Const.CurrencyType.Ticket.IconPosition());
      TargetSettings targetSettingsEnd = new TargetSettings(UIEmitter.Cam.Game, null, targetCanvasWorldPosition);
      ValueSettings valueSettings = new ValueSettings(ValueType.TotalValue, totalValue);
      UIManager.THIS.ticketEmitter.Emit(count, valueSettings, targetSettingsStart, targetSettingsEnd, UIManager.THIS.motionData_Ticket, OnArrive, OnAllArrive);
   }
}
