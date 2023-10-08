using System;
using Game.UI;
using Internal.Core;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;


public class IAPManager : Singleton<IAPManager>, IDetailedStoreListener
{
    IStoreController _storeController;
    ProductCollection _productCollection;
    private const string BasicChestID = "com.iwi.combatris.basicchest";
    private const string RemoveAdBreakID = "com.iwi.combatris.removeadbreak";
    private const string PiggyCoinPackID = "com.iwi.combatris.piggycoinpack";
    private const string TicketPackID = "com.iwi.combatris.ticketpack";
    private const string PrestigeChestID = "com.iwi.combatris.prestigechest";
    private const string CoinPackID = "com.iwi.combatris.coinpack";
    private const string PrimeChestID = "com.iwi.combatris.primechest";


    void Awake()
    {
        void InitializeGamingServies(Action onSuccess, Action<string> onError)
        {
            try
            {
                var options = new InitializationOptions().SetEnvironmentName("production");

                UnityServices.InitializeAsync(options).ContinueWith(task => onSuccess());
            }
            catch (Exception exception)
            {
                onError(exception.Message);
            }
        }
        
        InitializeGamingServies(
            () =>
            {
                var text = "Congratulations!\nUnity Gaming Services has been successfully initialized.";
                Debug.Log(text);
            },
            message =>
            {
                var text = $"Unity Gaming Services failed to initialize with error: {message}.";
                Debug.LogError(text);
            });
    }
    void Start()
    {
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            var text =
                "Error: Unity Gaming Services not initialized.\n" +
                "To initialize Unity Gaming Services, open the file \"InitializeGamingServices.cs\" " +
                "and uncomment the line \"Initialize(OnSuccess, OnError);\" in the \"Awake\" method.";
            Debug.LogError(text);
            
            return;
        }
        
        Initialize();
    }

   
    public void Initialize()
    {
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProduct(BasicChestID, ProductType.NonConsumable);
        builder.AddProduct(RemoveAdBreakID, ProductType.NonConsumable);
        builder.AddProduct(PiggyCoinPackID, ProductType.NonConsumable);
        builder.AddProduct(TicketPackID , ProductType.NonConsumable);
        builder.AddProduct(PrestigeChestID, ProductType.NonConsumable);
        builder.AddProduct(CoinPackID, ProductType.NonConsumable);
        builder.AddProduct(PrimeChestID, ProductType.NonConsumable);
        UnityPurchasing.Initialize(this, builder);
    }

    public void Purchase(UpgradeMenu.PurchaseType purchaseType)
    {
        _storeController.InitiatePurchase(PurchaseType2ID(purchaseType));
    }
    public string GetLocalPrice(UpgradeMenu.PurchaseType purchaseType)
    {
        if (_productCollection == null)
        {
            return "No Connection";
        }
        return _productCollection.WithID(PurchaseType2ID(purchaseType)).metadata.localizedPriceString;
    }


    private string PurchaseType2ID(UpgradeMenu.PurchaseType purchaseType)
    {
        switch (purchaseType)
        {
            case UpgradeMenu.PurchaseType.RemoveAdBreak:
                return RemoveAdBreakID;
            case UpgradeMenu.PurchaseType.TicketPack:
                return TicketPackID;
            case UpgradeMenu.PurchaseType.CoinPack:
                return CoinPackID;
            case UpgradeMenu.PurchaseType.PiggyCoinPack:
                return PiggyCoinPackID;
            case UpgradeMenu.PurchaseType.BasicChest:
                return BasicChestID;
            case UpgradeMenu.PurchaseType.PrimeChest:
                return PrimeChestID;
            case UpgradeMenu.PurchaseType.PrestigeChest:
                return PrestigeChestID;
        }
        Debug.LogError("Purchase Type Not Found");
        return "";
    }
    
    private UpgradeMenu.PurchaseType ID2PurchaseType(string id)
    {
        switch (id)
        {
            case RemoveAdBreakID:
                return UpgradeMenu.PurchaseType.RemoveAdBreak;
            case TicketPackID:
                return UpgradeMenu.PurchaseType.TicketPack;
            case CoinPackID:
                return UpgradeMenu.PurchaseType.CoinPack;
            case PiggyCoinPackID:
                return UpgradeMenu.PurchaseType.PiggyCoinPack;
            case BasicChestID:
                return UpgradeMenu.PurchaseType.BasicChest;
            case PrimeChestID:
                return UpgradeMenu.PurchaseType.PrimeChest;
            case PrestigeChestID:
                return UpgradeMenu.PurchaseType.PrestigeChest;
        }
        Debug.LogError("Purchase Type Not Found");
        return UpgradeMenu.PurchaseType.Shield;
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("In-App Purchasing successfully initialized");
        _storeController = controller;
        this._productCollection = _storeController.products;
    }
    public void OnInitializeFailed(InitializationFailureReason error)
    {
        OnInitializeFailed(error, null);
    }
    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        var errorMessage = $"Purchasing failed to initialize. Reason: {error}.";

        if (message != null)
        {
            errorMessage += $" More details: {message}";
        }

        Debug.Log(errorMessage);
    }
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        var product = args.purchasedProduct;
        UpgradeMenu.THIS.OnPurchase(ID2PurchaseType(product.definition.id));
        Debug.Log($"Purchase Complete - Product: {product.definition.id}");
        return PurchaseProcessingResult.Complete;
    }
    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.Log($"Purchase failed - Product: '{product.definition.id}', PurchaseFailureReason: {failureReason}");
    }
    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        Debug.Log($"Purchase failed - Product: '{product.definition.id}'," +
                  $" Purchase failure reason: {failureDescription.reason}," +
                  $" Purchase failure details: {failureDescription.message}");
    }
}
