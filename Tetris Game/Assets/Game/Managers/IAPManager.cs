using System;
using System.Globalization;
using System.Linq;
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

    public delegate OfferScreen.OfferData[] GetOfferFunction();
    
    public static System.Action<string, bool> OnPurchaseFinish;
    public static GetOfferFunction OnGetOffers;
    
    private string _localCurrencySymbol = null;


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

        OfferScreen.OfferData[] offerData = OnGetOffers.Invoke();
        foreach (var offer in offerData)
        {
            builder.AddProduct(offer.iapID, offer.productType);
        }
        UnityPurchasing.Initialize(this, builder);
    }

    public void Purchase(string purchaseID)
    {
        _storeController.InitiatePurchase(purchaseID);
    }

    public string GetPriceSymbol(string iapID)
    {
        if (!string.IsNullOrEmpty(_localCurrencySymbol))
        {
            return _localCurrencySymbol;
        }
        if (_productCollection == null)
        {
            return "Retrieving price...";
        }
        Product product = _productCollection.WithID(iapID);
        return GetCurrencySymbol(product.metadata.isoCurrencyCode);
    }
    public decimal GetPriceDecimal(string iapID)
    {
        if (_productCollection == null)
        {
            return 0;
        }
#if UNITY_EDITOR
        return 0.99m;
        
#else
        Product product = _productCollection.WithID(iapID);
        return product.metadata.localizedPrice;
#endif
    }

    private string GetCurrencySymbol(string isoCode)
    {
        #if UNITY_EDITOR
            isoCode = "USD";
        #endif
        
        _localCurrencySymbol = CultureInfo
            .GetCultures(CultureTypes.AllCultures)
            .Where(c => !c.IsNeutralCulture)
            .Select(culture => {
                try
                {
                    return new RegionInfo(culture.Name);
                }
                catch
                {
                    return null;
                }
            })
            .Where(ri => ri!=null && ri.ISOCurrencySymbol == isoCode)
            .Select(ri => ri.CurrencySymbol)
            .FirstOrDefault();

        return _localCurrencySymbol ?? isoCode;
    }


    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
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
        OnPurchaseFinish?.Invoke(product.definition.id, true);
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        
    }
    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        OnPurchaseFinish?.Invoke(product.definition.id, false);
    }
}
