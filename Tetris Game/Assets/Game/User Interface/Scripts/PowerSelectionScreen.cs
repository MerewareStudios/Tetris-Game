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
    [SerializeField] public ToggleButton toggleButton;
    [SerializeField] public TextMeshProUGUI stashText;
    [System.NonSerialized] public float TimeScale = 1.0f;

    void Start()
    {
        for (int i = 0; i < powerSelections.Count; i++)
        {
            powerSelections[i].Set(Select, powerUps[i].Icon(), i);
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
        
        TimeScale = 0.0f;
        GameManager.UpdateTimeScale();
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
        
        TimeScale = 1.0f;
        GameManager.UpdateTimeScale();
        return false;
    }

    public void Peak(bool state)
    {
        canvasGroup.DOKill();
        canvasGroup.DOFade(state ? 0.0f : 1.0f, 0.1f).SetEase(Ease.InOutSine).SetUpdate(true);
    }

    public void Select(PowerSelection powerSelection)
    {
        Powerup.THIS.SetPowerup(powerUps[powerSelection.PowerIndex], true, false);
        Close();
    }
}
