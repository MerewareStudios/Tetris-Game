using DG.Tweening;
using Internal.Core;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : Singleton<SettingsManager>
{
    [SerializeField] private GameObject buttonsParent;
    [SerializeField] private Image vibrateIcon;
    [SerializeField] private Image soundIcon;
    [SerializeField] private GameObject concedeButton;
    [SerializeField] private Color enabledColor;
    [SerializeField] private Color disabledColor;

    public void Set()
    {
        SetState(vibrateIcon, HapticManager.THIS.SavedData.canVibrate);
        SetState(soundIcon, HapticManager.THIS.SavedData.canPlayAudio);
        buttonsParent.SetActive(false);
    }

    private void SetState(Image image, bool state)
    {
        image.color = state ? enabledColor : disabledColor;
        image.rectTransform.DOKill();
        image.rectTransform.DOScale(Vector3.one * (state ? 0.85f : 0.75f), 0.1f).SetUpdate(true);
    }
    
    public void OnClick_Settings()
    {
        bool nextState = !buttonsParent.activeSelf;
        if (nextState)
        {
            concedeButton.SetActive(GameManager.PLAYING);
        }
        buttonsParent.gameObject.SetActive(nextState);
        HapticManager.OnClickVibrate();
    }
    public void OnClick_Vibrate()
    {
        HapticManager.THIS.SavedData.canVibrate = !HapticManager.THIS.SavedData.canVibrate;
        Debug.Log(HapticManager.THIS.SavedData.canVibrate);
        SetState(vibrateIcon, HapticManager.THIS.SavedData.canVibrate);
        HapticManager.OnClickVibrate();
    }
    public void OnClick_Sound()
    {
        HapticManager.THIS.SavedData.canPlayAudio = !HapticManager.THIS.SavedData.canPlayAudio;
        SetState(soundIcon, HapticManager.THIS.SavedData.canPlayAudio);
        HapticManager.OnClickVibrate();
    }
    public void OnClick_Privacy()
    {
        Consent.THIS.OpenOnClick();
        OnClick_Settings();
        HapticManager.OnClickVibrate();
    }
    public void OnClick_Concede()
    {
        UIManager.THIS.AdLayerClick_Concede();
        OnClick_Settings();
        HapticManager.OnClickVibrate();
    }
}
