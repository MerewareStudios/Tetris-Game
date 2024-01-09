using Internal.Core;
using IWI;
using UnityEngine;
using UnityEngine.UI;

public class Consent : Lazyingleton<Consent>
{
    [TextArea] [SerializeField] private string privacyLink;
    [System.NonSerialized] private System.Action _onDone;
    [SerializeField] private ToggleButton toggleButtonPrivacy;
    // [SerializeField] private ToggleButton toggleButtonAge;
    [SerializeField] private GameObject loadingBar;
    [SerializeField] private GameObject subFrame;
    [SerializeField] private Button doneButton;
    [System.NonSerialized] public int TimeScale = 1;

    public delegate bool GetActiveState();

    
    public bool Loading
    {
        set
        {
            loadingBar.SetActive(value);
            subFrame.SetActive(!value);
            doneButton.gameObject.SetActive(!value);
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

    public Consent SetInitialStates(bool privacyState, bool ageState)
    {
        toggleButtonPrivacy.SetIsOnWithoutNotify(privacyState);
        // toggleButtonAge.SetIsOnWithoutNotify(ageState);
        return this;
    }

    public Consent Open(System.Action onDone)
    {
        this._onDone = onDone;
        Visible = true;

        Loading = false;
        
        bool privacyState;
        bool ageState;
        
        if (AdManager.HasTakenAnyConsent())
        {
            privacyState = AdManager.HasMediationPrivacyConsent();
            ageState = !AdManager.IsMediationAgeRestricted();
        }
        else
        {
            privacyState = true;
            ageState = true;
        }
        
        SetInitialStates(privacyState, ageState);
        return this;
    }
    
    public void OpenOnClick()
    {
        HapticManager.OnClickVibrate();
        this.Open(Close);
    }

    public void Close()
    {
        Visible = false;
    }
    
    public void Done()
    {
        HapticManager.OnClickVibrate();

        AdManager.SetMediationConsentTaken(true);
        AdManager.SetMediationPrivacyConsent(toggleButtonPrivacy.isOn);
        AdManager.SetMediationGDPR(toggleButtonPrivacy.isOn);
        // AdManager.SetMediationGDPR(!toggleButtonAge.isOn);
        _onDone?.Invoke();
    }
    
    public void AcceptPrivacy(bool state)
    {
    }
    
    public void AcceptAge(bool state)
    {
    }

    public void OnClickPrivacyLink()
    {
        Application.OpenURL(privacyLink);
    }
}
