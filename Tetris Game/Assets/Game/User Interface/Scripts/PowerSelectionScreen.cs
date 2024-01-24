using System.Collections.Generic;
using DG.Tweening;
using Game;
using Internal.Core;
using TMPro;
using UnityEngine;

public class PowerSelectionScreen : Lazyingleton<PowerSelectionScreen>
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private List<PowerSelection> powerSelections;
    [SerializeField] public Pawn.Usage[] powerUps;
    [SerializeField] public int[] unlockIndexes;
    [SerializeField] public ToggleButton toggleButton;
    [SerializeField] public TextMeshProUGUI stashText;
    [SerializeField] public Sprite lockIcon;
    [System.NonSerialized] public int TimeScale = 1;

    void Start()
    {
        for (int i = 0; i < powerSelections.Count; i++)
        {
            powerSelections[i].Set(Select, i);
        }
        SetStashState(Powerup.THIS._Data.use);
    }

    public void SetStashState(bool use)
    {
        toggleButton.SetIsOnWithoutNotify(use);
        stashText.text = use ? "USE" : "STASH";
    }

    public void ToggleStash(bool state)
    {
        HapticManager.OnClickVibrate();

        Powerup.THIS._Data.use = state;
        stashText.text = state ? "USE" : "STASH";
    }

    public void Toggle()
    {
        _ = canvas.enabled ? Close() : Open();
    }
    public bool Open()
    {
        if (canvas.enabled)
        {
            return true;
        }
        canvas.enabled = true;
        this.gameObject.SetActive(true);

        int currentLevel = LevelManager.CurrentLevel;
        for (int i = 0; i < powerSelections.Count; i++)
        {
            bool unlocked = currentLevel >= unlockIndexes[i];
            powerSelections[i].SetIcon(unlocked ? powerUps[i].Icon() : lockIcon);
        }
        
        TimeScale = 0;
        GameManager.UpdateTimeScale(false);
        return true;
    }

    public bool Close()
    {
        if (!canvas.enabled)
        {
            return true;
        }
        canvas.enabled = false;
        this.gameObject.SetActive(false);
        
        TimeScale = 1;
        GameManager.UpdateTimeScale(false);
        return false;
    }
    public void OnClick_Close()
    {
        HapticManager.OnClickVibrate(Audio.Button_Click_Exit);
        Close();
    }
    
    public void Peak(bool state)
    {
        canvasGroup.DOKill();
        canvasGroup.DOFade(state ? 0.0f : 1.0f, 0.1f).SetEase(Ease.InOutSine).SetUpdate(true);
    }

    private void Select(int index)
    {
        HapticManager.OnClickVibrate();

        int currentLevel = LevelManager.CurrentLevel;
        bool unlocked = currentLevel >= unlockIndexes[index];
        if (!unlocked)
        {
            return;
        }
        Powerup.THIS.SetPowerup(powerUps[index], true, false);
        Close();
    }
}
