using Game;
using Game.UI;
using Internal.Core;
using IWI.Tutorial;
using IWI.UI;
using UnityEngine;
using UnityEngine.Serialization;
using Visual.Effects;
using Space = IWI.Emitter.Enums.Space;
using ValueType = IWI.Emitter.Enums.ValueType;

public class UIManager : Singleton<UIManager>
{
   [Header("Canvases")]
   [SerializeField] private BlockMenu blockMenu;
   [SerializeField] private WeaponMenu weaponMenu;
   [SerializeField] private UpgradeMenu upgradeMenu;
   [SerializeField] private SlashScreen slashScreen;
   [SerializeField] private MenuNavigator menuNavigator;
   [SerializeField] private PiggyMenu piggyMenu;
   [SerializeField] private RewardScreen rewardScreen;
   [Header("Bars")]
   [SerializeField] public Shop shop;
   // [SerializeField] public LoanBar loanBar;
   [FormerlySerializedAs("particleImageCoin")]
   [Header("UI Emitter")]
   [SerializeField] public UIEmitter coinEmitter;
   [SerializeField] public MotionData motionData_Block;
   [SerializeField] public MotionData motionData_Enemy;
   [SerializeField] public MotionData motionData_LevelReward;
   [SerializeField] public MotionData motionData_PiggyFill;
   [SerializeField] public MotionData motionData_Shop;
   [Header("Level")]
   // [SerializeField] public TextMeshProUGUI levelText;
   [System.NonSerialized] public static string NO_FUNDS_TEXT = "NO FUNDS";
   [System.NonSerialized] public static bool MENU_VISIBLE = false;
   [Header("Transactors")]
   [SerializeField] public CurrencyTransactor coin;
   [SerializeField] public CurrencyTransactor gem;
   [SerializeField] public CurrencyTransactor ad;
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

         RewardScreen.THIS.OnClose = () =>
         {
            MenuMode(false);
            Wallet.ScaleTransactors(1.0f);
            LevelManager.THIS.LoadLevel();
         };

         Wallet.CurrencyTransactors = new[] { Wallet.COIN, Wallet.GEM, Wallet.AD };

         MENU_VISIBLE = false;
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
         MenuNavigator.THIS.Open();
      }
      if (Input.GetKeyDown(KeyCode.Space))
      {
         Warzone.THIS.GiveShield(1);
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
      MENU_VISIBLE = value;
      Time.timeScale = value ? 0.0f : 1.0f;
      
      OnMenuModeChanged?.Invoke(value);
   }
}

public static class UIManagerExtensions
{
   public static void Distort(Vector3 worldPosition, float delay)
   {
       Transform camTransform = CameraManager.THIS.gameCamera.transform;
       Vector3 pos = worldPosition + camTransform.forward * -3.0f;
       Pool.Distortion.Spawn<Distortion>().Distort(pos, camTransform.forward, AnimConst.THIS.distortScale, AnimConst.THIS.distortStartRamp, AnimConst.THIS.distortEndRamp, AnimConst.THIS.distortDuration, AnimConst.THIS.distortEase, delay);
   }

   public static void EmitBlockCoin(Vector3 worldPosition, int count, int totalValue)
   {
      TargetSettings targetSettingsStart = new TargetSettings(Space.World, null, worldPosition);
      ValueSettings valueSettings = new ValueSettings(ValueType.TotalValue, totalValue);
      UIManager.THIS.coinEmitter.Emit(count, valueSettings, targetSettingsStart, null, UIManager.THIS.motionData_Block);
   }
   public static void EmitEnemyCoin(Vector3 worldPosition, int count, int totalValue)
   {
      TargetSettings targetSettingsStart = new TargetSettings(Space.World, null, worldPosition);
      ValueSettings valueSettings = new ValueSettings(ValueType.TotalValue, totalValue);
      UIManager.THIS.coinEmitter.Emit(count, valueSettings, targetSettingsStart, null, UIManager.THIS.motionData_Enemy);
   }
   public static void EmitEnemyCoinToPlayer(Vector3 worldPosition, int count, int totalValue)
   {
      TargetSettings targetSettingsStart = new TargetSettings(Space.World, null, worldPosition);
      TargetSettings targetSettingsEnd = new TargetSettings(Space.World, null, Warzone.THIS.Player.acceptTarget.position);
      ValueSettings valueSettings = new ValueSettings(ValueType.TotalValue, totalValue);
      UIManager.THIS.coinEmitter.Emit(count, valueSettings, targetSettingsStart, targetSettingsEnd, UIManager.THIS.motionData_Enemy);
   }
   public static void EmitLevelRewardCoin(Vector3 canvasWorldPosition, int count, int totalValue, System.Action OnAllArrive)
   {
      TargetSettings targetSettingsStart = new TargetSettings(Space.Screen, null, canvasWorldPosition);
      ValueSettings valueSettings = new ValueSettings(ValueType.TotalValue, totalValue);
      UIManager.THIS.coinEmitter.Emit(count, valueSettings, targetSettingsStart, null, UIManager.THIS.motionData_LevelReward, null, OnAllArrive);
   }
   public static void RequestCoinFromWallet(Vector3 targetCanvasWorldPosition, int count, int totalValue, System.Action<int> OnArrive, System.Action OnAllArrive)
   {
      TargetSettings targetSettingsStart = new TargetSettings(Space.Screen, null, Const.CurrencyType.Coin.IconPosition());
      TargetSettings targetSettingsEnd = new TargetSettings(Space.Screen, null, targetCanvasWorldPosition);
      ValueSettings valueSettings = new ValueSettings(ValueType.TotalValue, totalValue);
      UIManager.THIS.coinEmitter.Emit(count, valueSettings, targetSettingsStart, targetSettingsEnd, UIManager.THIS.motionData_PiggyFill, OnArrive, OnAllArrive);
   }
   public static void EmitLevelShopCoin(Vector3 canvasWorldPosition, int count, int totalValue)
   {
      TargetSettings targetSettingsStart = new TargetSettings(Space.Screen, null, canvasWorldPosition);
      ValueSettings valueSettings = new ValueSettings(ValueType.TotalValue, totalValue);
      UIManager.THIS.coinEmitter.Emit(count, valueSettings, targetSettingsStart, null, UIManager.THIS.motionData_Shop, null, null);
   }
}
