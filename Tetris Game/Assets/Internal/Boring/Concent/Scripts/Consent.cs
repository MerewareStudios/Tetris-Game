using Internal.Core;
using UnityEngine;
using UnityEngine.UI;

public class Consent : Lazyingleton<Consent>
{
    [TextArea] [SerializeField] private string privacyLink;
    [System.NonSerialized] private System.Action _onDone;
    [SerializeField] private ToggleButton toggleButtonPrivacy;
    [SerializeField] private ToggleButton toggleButtonAge;
    [SerializeField] private GameObject loadingBar;
    [SerializeField] private GameObject subFrame;
    [SerializeField] private Button doneButton;
    [SerializeField] private Button restartButton;
    [System.NonSerialized] public int TimeScale = 1;

    public delegate bool GetActiveState();
    public static GetActiveState GetRestartButtonState;

    
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
        toggleButtonAge.SetIsOnWithoutNotify(ageState);
        return this;
    }

    public Consent Open(System.Action onDone)
    {
        this._onDone = onDone;
        Visible = true;

        Loading = false;
        
        restartButton.gameObject.SetActive(GetRestartButtonState.Invoke());
        
        bool privacyState;
        bool ageState;
        
        if (MaxSdk.IsUserConsentSet())
        {
            privacyState = MaxSdk.HasUserConsent();
            ageState = !MaxSdk.IsAgeRestrictedUser();
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
        MaxSdk.SetHasUserConsent(toggleButtonPrivacy.isOn);
        MaxSdk.SetIsAgeRestrictedUser(!toggleButtonAge.isOn);
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
