#define GDPR_DEBUG

#if GDPR_DEBUG
    using System.Collections.Generic;
#endif

using GoogleMobileAds.Api.Mediation.UnityAds;
using GoogleMobileAds.Ump.Api;
using Internal.Core;
using IWI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Consent : Lazyingleton<Consent>
{
    [TextArea] [SerializeField] private string ageTextFill;
    [SerializeField] private TextMeshProUGUI ageText;
    [SerializeField] private Slider ageSlider;

    private int Age => (int)ageSlider.value;
    public void UpdateAgeText()
    {
        ageText.text = string.Format(ageTextFill, Age);
    }
    
    
    [TextArea] [SerializeField] private string privacyLink;
    [System.NonSerialized] public int TimeScale = 1;
    [System.NonSerialized] public System.Action OnAccept;
    [SerializeField] private GameObject acceptFrame;
    [SerializeField] private GameObject inGameFrame;


    #region AdMob UMP

    public void UpdateGDPR()
    {
        #if GDPR_DEBUG
        ResetConsent();
          var debugSettings = new ConsentDebugSettings
            {
                DebugGeography = DebugGeography.EEA,

                TestDeviceHashedIds =
                new List<string>
                {
                    "8CC2D3236465097BFA36ED41F80E9F96"
                }
            };
        #endif
        
        
        ConsentRequestParameters request = 
            new ConsentRequestParameters
            {
                TagForUnderAgeOfConsent = AdManager.IsUnderAgeForGDPR(),
#if GDPR_DEBUG
                ConsentDebugSettings = debugSettings,
#endif
            };

        ConsentInformation.Update(request, OnAdMobConsentInfoUpdated);
    }
    
#if GDPR_DEBUG
    public void ResetConsent()
    {
        ConsentInformation.Reset();
    }
#endif
    
    private void OnAdMobConsentInfoUpdated(FormError consentError)
    {
        if (consentError != null)
        {
            // Handle the error.
            // Debug.LogError(consentError);
            return;
        }
    

        // If the error is null, the consent information state was updated.
        // You are now ready to check if a form is available.
        ConsentForm.LoadAndShowConsentFormIfRequired((FormError formError) =>
        {
            if (formError != null)
            {
                // Consent gathering failed.
                // Debug.LogError(consentError);
                return;
            }

            // Consent has been gathered.
            
            if (ConsentInformation.CanRequestAds())
            {
                UnityAds.SetConsentMetaData("gdpr.consent", true);

                // MobileAds.Initialize((InitializationStatus initstatus) =>
                // {
                //     // TODO: Request an ad.
                // });
            }
            
        });
    }

    public void OnAdMobUpdatePrivacy()
    {
        ConsentForm.ShowPrivacyOptionsForm((FormError showError) =>
        {
            if (showError != null)
            {
                // Debug.LogError("Error showing privacy options form with error: " + showError.Message);
            }
            // _privacyButton.interactable = ConsentInformation.PrivacyOptionsRequirementStatus == PrivacyOptionsRequirementStatus.Required;
        });
    }

    
    #endregion
    
    
    public bool AcceptState
    {
        set
        {
            acceptFrame.SetActive(value);
            inGameFrame.SetActive(!value);
        }
    }

    public bool Visible
    {
        set
        {
            TimeScale = value ? 0 : 1;
            GameManager.UpdateTimeScale();
            gameObject.SetActive(value);
        }
        get => gameObject.activeSelf;
    }


    public Consent Open()
    {
        Visible = true;
        AcceptState = true;
        UpdateAgeText();
        return this;
    }
    
    public void OpenFromSettings()
    {
        HapticManager.OnClickVibrate();
        Visible = true;
        AcceptState = false;
    }

    public void Close()
    {
        HapticManager.OnClickVibrate();
        Visible = false;
    }
    
    public void OnClick_Accept()
    {
        HapticManager.OnClickVibrate();
        AdManager.PassPrivacyInfo(true, Age);
        OnAccept?.Invoke();
        OnAccept = null;
        Close();
    }
    
    public void OnClickPrivacyLink()
    {
        HapticManager.OnClickVibrate();
        Application.OpenURL(privacyLink);
    }
}
