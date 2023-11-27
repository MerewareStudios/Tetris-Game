using DG.Tweening;
using Game;
using Game.UI;
using Internal.Core;
using IWI;
using IWI.Tutorial;
using IWI.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Visual.Effects;
using ValueType = IWI.Emitter.Enums.ValueType;

public class UIManager : Singleton<UIManager>
{
   [SerializeField] public TextMeshProUGUI levelText;
   [SerializeField] public TextMeshProUGUI levelTextMenu;
   [SerializeField] public GameObject levelProgressbar;
   [SerializeField] public Image levelProgress;
   [SerializeField] public ParticleSystem piggyPS;
   [Header("Canvases")]
   [SerializeField] private Consent consent;
   [SerializeField] private BlockMenu blockMenu;
   [SerializeField] public WeaponMenu weaponMenu;
   [SerializeField] private UpgradeMenu upgradeMenu;
   [SerializeField] private SlashScreen slashScreen;
   [SerializeField] private MenuNavigator menuNavigator;
   [SerializeField] private PiggyMenu piggyMenu;
   [SerializeField] private Powerup powerup;
   [SerializeField] private AdBreakScreen adBreakScreen;
   [SerializeField] private PowerSelectionScreen powerSelectionScreen;
   [SerializeField] private StatDisplayArranger statDisplayArranger;
   [SerializeField] private OfferScreen offerScreen;
   [Header("Bars")]
   [SerializeField] public Shop shop;
   [Header("UI Emitter")]
   [SerializeField] public UIEmitter coinEmitter;
   [SerializeField] public UIEmitter piggyCoinEmitter;
   [SerializeField] public UIEmitter ticketEmitter;
   [SerializeField] public UIEmitter heartEmitter;
   [SerializeField] public MotionData motionData_Enemy_Burst;
   [SerializeField] public MotionData motionData_LevelReward;
   [SerializeField] public MotionData motionData_PiggyReward;
   [SerializeField] public MotionData motionData_PiggyFill;
   [SerializeField] public MotionData motionData_Shop;
   [SerializeField] public MotionData motionData_UpgradeBurst;
   [SerializeField] public MotionData motionData_BoardBurst;
   [SerializeField] public MotionData motionData_Ticket;
   [Header("Transactors")]
   [SerializeField] public CurrencyTransactor coin;
   [SerializeField] public CurrencyTransactor gem;
   [SerializeField] public CurrencyTransactor ticket;
   [System.NonSerialized] public static System.Action<bool> OnMenuModeChanged;
   [Header("Tutorial")]
   [SerializeField] public SpeechBubble speechBubble;
   [SerializeField] public ComboText comboText;
   [SerializeField] public Finger finger;
   [System.NonSerialized] public static IMenu CurrentMenu = null;
   [System.NonSerialized] public static bool MenuVisible = false;
   [Header("Plus")]
   [SerializeField] private Button plusCoinButton;
   [SerializeField] private Button plusPiggyCoinButton;
   [SerializeField] private Button plusTicketButton;
   [SerializeField] private Button plusHealthButton;

   public void SetLevelProgress(float value, int health)
   {
      levelProgressbar.gameObject.SetActive(true);
      levelProgress.DOKill();
      levelProgress.DOFillAmount(value, 0.2f).SetEase(Ease.OutQuad).onComplete = () =>
      {
         if (health > 0)
         {
            return;
         }
         levelProgressbar.gameObject.SetActive(false);
      };
   }
   
   void Awake()
   {
      Consent.THIS = consent;
      BlockMenu.THIS = blockMenu;
      WeaponMenu.THIS = weaponMenu;
      UpgradeMenu.THIS = upgradeMenu;
      SlashScreen.THIS = slashScreen;
      MenuNavigator.THIS = menuNavigator.Setup();
      PiggyMenu.THIS = piggyMenu;
      Powerup.THIS = powerup;
      AdBreakScreen.THIS = adBreakScreen;
      StatDisplayArranger.THIS = statDisplayArranger;
      PowerSelectionScreen.THIS = powerSelectionScreen;
      OfferScreen.THIS = offerScreen;


      UIEmitter.SpawnFunction = SpawnImageIcon;
      UIEmitter.DespawnFunction = DespawnImageIcon;

      

      Wallet.CurrencyTransactors = new[] { Wallet.COIN, Wallet.PIGGY, Wallet.TICKET };

      Glimmer.OnComplete = glimmer => glimmer.Despawn(Pool.Glimmer);

      AdBreakScreen.onVisibilityChanged = (value) =>
      {
         if (MenuVisible)
         {
            return;
         }
         Wallet.ScaleTransactors(value ? 1.1f : 1.0f, value);
      };
      
      IAPManager.OnPurchaseFinish = OfferScreen.THIS.OnPurchaseComplete;
      IAPManager.OnGetOffers = () => OfferScreen.THIS.offerData;

      
      AdBreakScreen.THIS.OnVisibilityChanged = GameManager.UpdateTimeScale;

      Consent.GetRestartButtonState = () => ONBOARDING.UPGRADE_TAB.IsComplete()
                                            && GameManager.PLAYING
                                            && MaxSdk.IsUserConsentSet();  
      
      OfferScreen.OnGetPrice = IAPManager.THIS.GetPriceDecimal;
      OfferScreen.OnGetPriceSymbol = IAPManager.THIS.GetPriceSymbol;
      OfferScreen.OnPurchaseOffer = IAPManager.THIS.Purchase;
      OfferScreen.THIS.OnVisibilityChanged = (visible, processState) =>
      {
         if (PiggyMenu.THIS.Visible && !AdBreakScreen.THIS.Visible)
         {
             if (visible)
             {
                 PiggyMenu.THIS.Pause();
             }
             else
             {
                 PiggyMenu.THIS.Restart();
             }
         }
         
         if (AdBreakScreen.THIS.Visible)
         {
             if (visible)
             {
                 AdBreakScreen.THIS.ByPassInProgress();
             }
             else
             {
                 if (processState.Equals(OfferScreen.ProcessState.SUCCESS))
                 {
                     AdBreakScreen.THIS.InvokeByPass();
                     return;
                 }
                 
                 AdBreakScreen.THIS.RevokeByPass();
             }
         }


         if (PiggyMenu.THIS.Visible)
         {
             PiggyMenu.THIS.SetMiddleSortingLayer(visible ? 0 : 9);                
         }
         GameManager.UpdateTimeScale();
      };
      OfferScreen.OnReward = (rewards, onFinish) =>
      {
         onFinish += SaveManager.THIS.Save;

         float closeDelay = 0.5f;
         bool forceUpdateMenu = true;
         for (int i = 0; i < rewards.Length; i++)
         {
             OfferScreen.Reward reward = rewards[i];
             UIEmitter emitter = null;
             switch (reward.rewardType)
             {
                 case OfferScreen.RewardType.NoAds:
                     AdManager.Bypass.Ads();
                     continue;
                 case OfferScreen.RewardType.Coin:
                     emitter = UIManager.THIS.coinEmitter;
                     forceUpdateMenu = false;
                     break;
                 case OfferScreen.RewardType.Gem:
                     emitter = UIManager.THIS.piggyCoinEmitter;
                     forceUpdateMenu = false;
                     break;
                 case OfferScreen.RewardType.Ticket:
                     emitter = UIManager.THIS.ticketEmitter;
                     forceUpdateMenu = false;
                     break;
                 case OfferScreen.RewardType.Heart:
                     emitter = UIManager.THIS.heartEmitter;
                     forceUpdateMenu = false;
                     break;
             }
             float duration = UIManagerExtensions.EmitOfferReward(emitter, OfferScreen.THIS.PreviewScreenPosition(i),  Mathf.Min(reward.amount, 15), reward.amount, null);
             closeDelay = Mathf.Max(closeDelay, duration);
         }

         if (forceUpdateMenu)
         {
             onFinish += UIManager.ForceUpdateAvailableMenu;
         }
         DOVirtual.DelayedCall(closeDelay, onFinish.Invoke);
      };

      OfferScreen.THIS.SkipCondition = () => Consent.THIS.Visible
                                            || UIManager.THIS.HoveringMeta
                                            || UIManager.THIS.HoveringStat;

        

      CurrentMenu = null;
   }

   private Image SpawnImageIcon()
   {
      return Pool.Image.Spawn<Image>();
   }
   
   private void DespawnImageIcon(Image image)
   {
      image.Despawn(Pool.Image);
   }

#region Ad Layer Clicks
   public void AdLayerClick_OpenShop()
   {
      AdManager.THIS.TryInterstitial(shop.OnClick_Open);
   }
   public void AdLayerClick_OpenPiggyBank()
   {
      AdManager.THIS.TryInterstitial(() => PiggyMenu.THIS.Open(0.225f));
   }
#endregion
#region Offer
   public void ShowOffer_RemoveAds_Banner()
   {
      AdManager.Offers.RemoveAds();
      AnalyticsManager.OfferShown(OfferScreen.OfferType.REMOVEADS, ActivityType.BANNER);
   }
   public void ShowOffer_RemoveAds_AdBreakByPass()
   {
      AdManager.Offers.RemoveAds();
      AnalyticsManager.OfferShown(OfferScreen.OfferType.REMOVEADS, ActivityType.ADBREAKBYPASS);
   }
   public void ShowOffer_TicketPlus_AdBreakByPass()
   {
      AdManager.Offers.TicketPack();
      AnalyticsManager.OfferShown(OfferScreen.OfferType.TICKETPACK, ActivityType.ADBREAKBYPASS);
   }
   public void ShowOffer_CoinPlus()
   {
      AdManager.Offers.CoinPack();
      AnalyticsManager.OfferShown(OfferScreen.OfferType.COINPACK, CurrentActivityScreen);
   }
   public void ShowOffer_PiggyCoinPlus()
   {
      AdManager.Offers.PiggyPack();
      AnalyticsManager.OfferShown(OfferScreen.OfferType.GEMPACK, CurrentActivityScreen);
   }
   public void ShowOffer_TicketPlus()
   {
      AdManager.Offers.TicketPack();
      AnalyticsManager.OfferShown(OfferScreen.OfferType.TICKETPACK, CurrentActivityScreen);
   }
   public void ShowOffer_HeartPlus()
   {
      AdManager.Offers.HealthPack();
      AnalyticsManager.OfferShown(OfferScreen.OfferType.HEALTHPACK, CurrentActivityScreen);
   }
#endregion
#if UNITY_EDITOR
   private void Update()
   {
      if (Input.GetKeyDown(KeyCode.Q))
      {
         UIManager.THIS.comboText.Show(2);
      }
      if (Input.GetKeyDown(KeyCode.P))
      {
         PiggyMenu.THIS.Open();
      }
      if (Input.GetKeyDown(KeyCode.R))
      {
         MenuNavigator.THIS.Open();
      }
      if (Input.GetKeyDown(KeyCode.M))
      {
         Warzone.THIS.Player._CurrentHealth += 1;
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
         if (ONBOARDING.USE_POWERUP.IsNotComplete())
         {
            Powerup.THIS.Enabled = true;
            Onboarding.TalkAboutPowerUp();
         }
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
      if (Input.GetKeyDown(KeyCode.H))
      {
         UIManagerExtensions.HeartToPlayer(Vector3.zero,  1, 1);
      }
      if (Input.GetKeyDown(KeyCode.I))
      {
         Warzone.THIS.Player.Gun.Boost();

      }
      if (Input.GetKeyDown(KeyCode.Alpha1))
      {
         AdManager.Offers.Offer1();
      }
      if (Input.GetKeyDown(KeyCode.Alpha2))
      {
         AdManager.Offers.Offer2();
      }
      if (Input.GetKeyDown(KeyCode.Alpha3))
      {
         AdManager.Offers.Offer3();
      }
      if (Input.GetKeyDown(KeyCode.Alpha4))
      {
         AdManager.Offers.Offer4();
      }
      if (Input.GetKeyDown(KeyCode.Alpha5))
      {
         AdManager.Offers.Offer5();
      }
   }
#endif
   
   public bool HoveringMeta
   {
      get
      {
         if (!UIManager.THIS.coinEmitter.Idle)
         {
            return true;
         }
         if (!UIManager.THIS.piggyCoinEmitter.Idle)
         {
            return true;
         }
         if (!UIManager.THIS.ticketEmitter.Idle)
         {
            return true;
         }

         return false;
      }
   }
   public bool HoveringStat
   {
      get
      {
         if (!UIManager.THIS.heartEmitter.Idle)
         {
            return true;
         }

         return false;
      }
   }

   public static void UpdateNotifications()
   {
      if (!UIManager.THIS.coinEmitter.Idle)
      {
         return;
      }
      if (!UIManager.THIS.piggyCoinEmitter.Idle)
      {
         return;
      }
      MenuNavigator.THIS.UpdateNotifyMenus(UIManager.CurrentMenu != null);
      MenuNavigator.THIS.UpdateTotalNotify();
   }
   
   public static void ForceUpdateAvailableMenu()
   {
      if (UIManager.THIS.HoveringMeta)
      {
         return;
      }
      if (UIManager.CurrentMenu == null)
      {
         return;
      }
      UIManager.CurrentMenu.Show();
   }

   public static void MenuMode(bool value)
   {
      MenuVisible = value;
      CameraManager.THIS.gameCamera.enabled = !value;
      SaveManager.THIS.Save();
      
      OnMenuModeChanged?.Invoke(value);
   }

   public bool PlusButtonsState
   {
      set
      {
         UIManager.THIS.plusCoinButton.gameObject.SetActive(value);
         UIManager.THIS.plusPiggyCoinButton.gameObject.SetActive(value);
         UIManager.THIS.plusTicketButton.gameObject.SetActive(value);
         UIManager.THIS.plusHealthButton.gameObject.SetActive(value);
      }
   }

   public ActivityType CurrentActivityScreen
   {
      get
      {
         if (PiggyMenu.THIS.Visible)
         {
            return ActivityType.PIGGYMENU;
         }
         if (BlockMenu.THIS.Visible)
         {
            return ActivityType.BLOCKMENU;
         }
         if (WeaponMenu.THIS.Visible)
         {
            return ActivityType.WEAPONMENU;
         }
         if (UpgradeMenu.THIS.Visible)
         {
            return ActivityType.UPGRADEMENU;
         }
         if (AdBreakScreen.THIS.Visible)
         {
            return ActivityType.ADBREAK;
         }
         return ActivityType.GAME;
      }
   }
   public enum ActivityType
   {
      GAME,
      PIGGYMENU,
      BLOCKMENU,
      WEAPONMENU,
      UPGRADEMENU,
      BANNER,
      ADBREAK,
      ADBREAKBYPASS,
   }
}

public static class UIManagerExtensions
{
   public static void Distort(Vector3 worldPosition, float scale, float power, float duration, Ease ease)
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

      Pool.Distortion.Spawn<Distortion>().Distort(hitPoint, forward, scale, power, duration, ease);
   }
   public static void QuickDistort(Vector3 worldPosition)
   {
      Distort(worldPosition, AnimConst.THIS.distortScale, AnimConst.THIS.distortPower, AnimConst.THIS.distortDuration, AnimConst.THIS.distortEase);
   }
   public static void DistortWarmUp()
   {
      Distort(new Vector3(0.0f, 1.0f, 0.0f), 1.0f, 0.0f, 0.25f, Ease.Linear);
   }
   
   public static void Glimmer(this Image image, float speed)
   {
      Glimmer glimmer = Pool.Glimmer.Spawn<Glimmer>();
      RectTransform rectTransform = image.rectTransform;
      glimmer.Show(image, rectTransform, speed, AnimConst.THIS.glimmerEase);
   }
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
   public static void EmitPiggyRewardCoin(Vector3 canvasWorldPosition, int count, int totalValue, System.Action OnAllArrive)
   {
      TargetSettings targetSettingsStart = new TargetSettings(UIEmitter.Cam.UI, null, canvasWorldPosition);
      ValueSettings valueSettings = new ValueSettings(ValueType.TotalValue, totalValue);
      UIManager.THIS.coinEmitter.Emit(count, valueSettings, targetSettingsStart, null, UIManager.THIS.motionData_PiggyReward, null, OnAllArrive);
   }
   public static void EmitPiggyRewardPiggy(Vector3 canvasWorldPosition, int count, int totalValue, System.Action OnAllArrive)
   {
      TargetSettings targetSettingsStart = new TargetSettings(UIEmitter.Cam.UI, null, canvasWorldPosition);
      ValueSettings valueSettings = new ValueSettings(ValueType.TotalValue, totalValue);
      UIManager.THIS.piggyCoinEmitter.Emit(count, valueSettings, targetSettingsStart, null, UIManager.THIS.motionData_PiggyReward, null, OnAllArrive);
   }
   public static void EmitPiggyRewardTicket(Vector3 canvasWorldPosition, int count, int totalValue, System.Action OnAllArrive)
   {
      TargetSettings targetSettingsStart = new TargetSettings(UIEmitter.Cam.UI, null, canvasWorldPosition);
      ValueSettings valueSettings = new ValueSettings(ValueType.TotalValue, totalValue);
      UIManager.THIS.ticketEmitter.Emit(count, valueSettings, targetSettingsStart, null, UIManager.THIS.motionData_PiggyReward, null, OnAllArrive);
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
   public static void BoardCoinToPlayer(Vector3 worldPosition, int count, int totalValue)
   {
      TargetSettings targetSettingsStart = new TargetSettings(UIEmitter.Cam.Game, null, worldPosition);
      ValueSettings valueSettings = new ValueSettings(ValueType.TotalValue, totalValue);
      UIManager.THIS.coinEmitter.Emit(count, valueSettings, targetSettingsStart, null, UIManager.THIS.motionData_BoardBurst);
   }
   public static void BoardHeartToPlayer(Vector3 worldPosition, int count, int totalValue)
   {
      TargetSettings targetSettingsStart = new TargetSettings(UIEmitter.Cam.Game, null, worldPosition);
      ValueSettings valueSettings = new ValueSettings(ValueType.TotalValue, totalValue);
      UIManager.THIS.heartEmitter.Emit(count, valueSettings, targetSettingsStart, null, UIManager.THIS.motionData_BoardBurst);
   }
   public static void HeartToPlayer(Vector3 worldPosition, int count, int totalValue)
   {
      TargetSettings targetSettingsStart = new TargetSettings(UIEmitter.Cam.Game, null, worldPosition);
      ValueSettings valueSettings = new ValueSettings(ValueType.TotalValue, totalValue);
      UIManager.THIS.heartEmitter.Emit(count, valueSettings, targetSettingsStart, null, UIManager.THIS.motionData_UpgradeBurst);
   }
   public static void RequestTicketFromWallet(Vector3 targetCanvasWorldPosition, int count, int totalValue, System.Action<int> OnArrive, System.Action OnAllArrive, UIEmitter.Cam cam = UIEmitter.Cam.Game)
   {
      TargetSettings targetSettingsStart = new TargetSettings(UIEmitter.Cam.UI, null, Const.CurrencyType.Ticket.IconPosition());
      TargetSettings targetSettingsEnd = new TargetSettings(cam, null, targetCanvasWorldPosition);
      ValueSettings valueSettings = new ValueSettings(ValueType.TotalValue, totalValue);
      UIManager.THIS.ticketEmitter.Emit(count, valueSettings, targetSettingsStart, targetSettingsEnd, UIManager.THIS.motionData_Ticket, OnArrive, OnAllArrive);
   }
   
   public static float EmitOfferReward(UIEmitter uiEmitter, Vector3 canvasWorldPosition, int count, int totalValue, System.Action OnAllArrive)
   {
      TargetSettings targetSettingsStart = new TargetSettings(UIEmitter.Cam.UI, null, canvasWorldPosition);
      ValueSettings valueSettings = new ValueSettings(ValueType.TotalValue, totalValue);
      MotionData motionData = UIManager.THIS.motionData_LevelReward;
      uiEmitter.Emit(count, valueSettings, targetSettingsStart, null, UIManager.THIS.motionData_LevelReward, null, OnAllArrive);
      return motionData.MaxDuration;
   }
}
