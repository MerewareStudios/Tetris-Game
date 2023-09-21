using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game;
using UnityEngine;
using UnityEngine.UI;

public class Powerup : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform pivot;
    [SerializeField] private Button button;
    [SerializeField] private Image icon;
    [System.NonSerialized] private Pawn.Usage currentUsage;


    void Start()
    {
        SetPowerup(Pawn.Usage.MagnetLR);
    }

    public void SetPowerup(Pawn.Usage usage)
    {
        this.currentUsage = usage;
        icon.sprite = Const.THIS.pawnIcons[(int)usage];

        canvas.enabled = true;
        
        pivot.DOKill();
        pivot.localScale = Vector3.one;
        pivot.DOPunchScale(Vector3.one * 0.2f, 0.25f).onComplete = () =>
        {
            
            button.targetGraphic.raycastTarget = true;

        };
    }
    public void OnClick_Use()
    {
        button.targetGraphic.raycastTarget = false;
        Spawner.THIS.InterchangeBlock(Pool.Single_Block, this.currentUsage);

        
        pivot.DOKill();
        pivot.DOScale(Vector3.zero, 0.15f).SetEase(Ease.InBack).onComplete = () =>
        {
            canvas.enabled = false;
        };
    }
}
