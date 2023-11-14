using System.Collections.Generic;
using DG.Tweening;
using Game;
using Internal.Core;
using UnityEngine;

public class PowerSelectionScreen : Lazyingleton<PowerSelectionScreen>
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private List<PowerSelection> powerSelections;
    [SerializeField] public Pawn.Usage[] powerUps;
    [System.NonSerialized] public float Timescale = 1.0f;

    void Start()
    {
        for (int i = 0; i < powerSelections.Count; i++)
        {
            powerSelections[i].Set(Select, powerUps[i].Icon(), i);
        }
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
        
        Timescale = 0.0f;
        GameManager.UpdateTimeScale();
        return true;
    }

    public bool Close()
    {
        canvas.enabled = false;
        this.gameObject.SetActive(false);
        
        Timescale = 1.0f;
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
        Powerup.THIS.SetPowerup(powerUps[powerSelection.PowerIndex], true);
        Close();
    }
}
