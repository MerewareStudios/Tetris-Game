using DG.Tweening;
using Internal.Core;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : Singleton<SettingsManager>
{
    [System.NonSerialized] public float TimeScale = 1.0f;
    [SerializeField] private GameObject background;
    [SerializeField] private GameObject buttonsParent;
    [SerializeField] private Image vibrateIcon;
    [SerializeField] private Image soundIcon;
    [SerializeField] private GameObject concedeButton;
    [SerializeField] private Color enabledColor;
    [SerializeField] private Color disabledColor;
    [SerializeField] private GameObject concedeBlockLabel;

    private bool Visible
    {
        set
        {
            if (value)
            {
                concedeBlockLabel.SetActive(LevelManager.CurrentLevel <= 1);
            }
            buttonsParent.SetActive(value);
            background.SetActive(value);
            TimeScale = value ? 0.0f : 1.0f;
            GameManager.UpdateTimeScale();
        }
        get => buttonsParent.activeSelf;
    }

    public void Set()
    {
        SetState(vibrateIcon, HapticManager.THIS.SavedData.canVibrate);
        SetState(soundIcon, HapticManager.THIS.CanPlayAudio);
        Visible = false;
    }

    private void SetState(Image image, bool state)
    {
        image.color = state ? enabledColor : disabledColor;
        image.rectTransform.DOKill();
        image.rectTransform.DOScale(Vector3.one * (state ? 0.85f : 0.75f), 0.1f).SetUpdate(true);
    }
    
    public void OnClick_Settings()
    {
        bool nextState = !Visible;
        if (nextState)
        {
            concedeButton.SetActive(GameManager.PLAYING);
        }
        Visible = nextState;
        HapticManager.OnClickVibrate();
    }
    public void OnClick_Vibrate()
    {
        HapticManager.THIS.SavedData.canVibrate = !HapticManager.THIS.SavedData.canVibrate;
        SetState(vibrateIcon, HapticManager.THIS.SavedData.canVibrate);
        HapticManager.OnClickVibrate();
    }
    public void OnClick_Sound()
    {
        HapticManager.THIS.CanPlayAudio = !HapticManager.THIS.CanPlayAudio;
        SetState(soundIcon, HapticManager.THIS.CanPlayAudio);
        HapticManager.OnClickVibrate();
    }
    public void OnClick_Privacy()
    {
        HapticManager.OnClickVibrate();
        Consent.THIS.OpenFromSettings();
        OnClick_Settings();
    }
    public void OnClick_Concede()
    {
        HapticManager.OnClickVibrate();
        if (LevelManager.CurrentLevel <= 1)
        {
            return;
        }
        UIManager.THIS.AdLayerClick_Concede();
        OnClick_Settings();
    }
}
