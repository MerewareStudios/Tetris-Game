using Internal.Core;
using UnityEngine;

public class Consent : Lazyingleton<Consent>
{
    [TextArea] [SerializeField] private string privacyLink;
    [System.NonSerialized] private System.Action _onDone;
    [SerializeField] private ToggleButton toggleButtonPrivacy;
    [SerializeField] private ToggleButton toggleButtonAge;

    public Consent SetInitialStates(bool privacyState, bool ageState)
    {
        toggleButtonPrivacy.SetIsOnWithoutNotify(privacyState);
        toggleButtonAge.SetIsOnWithoutNotify(ageState);
        return this;
    }

    public Consent Open(System.Action onDone)
    {
        this._onDone = onDone;
        this.gameObject.SetActive(true);
        
        if (!MaxSdk.IsUserConsentSet())
        {
            MaxSdk.SetHasUserConsent(true);
            MaxSdk.SetIsAgeRestrictedUser(false);
        }
        
        bool privacyState = MaxSdk.HasUserConsent();
        bool ageState = !MaxSdk.IsAgeRestrictedUser();
        SetInitialStates(privacyState, ageState);
        
        return this;
    }
    
    public void Open()
    {
        UIManager.Pause(true);
        this.Open(() =>
        {
            UIManager.Pause(false);
        });
    }
    
    public void Done()
    {
        this.gameObject.SetActive(false);
        _onDone?.Invoke();
    }
    
    public void AcceptPrivacy(bool state)
    {
        MaxSdk.SetHasUserConsent(state);
    }
    
    public void AcceptAge(bool state)
    {
        MaxSdk.SetIsAgeRestrictedUser(!state);
    }

    public void OnClickPrivacyLink()
    {
        Application.OpenURL(privacyLink);
    }
    
    // [System.Serializable]
    // public class Data : ICloneable
    // {
    //     [SerializeField] public bool firstConcentTaken;
    //         
    //     public Data(Data data)
    //     {
    //         firstConcentTaken = data.firstConcentTaken;
    //     }
    //
    //     public object Clone()
    //     {
    //         return new Data(this);
    //     }
    // } 
}
