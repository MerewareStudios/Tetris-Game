using DG.Tweening;
using Game;
using Internal.Core;
using UnityEngine;
using UnityEngine.UI;

public class Powerup : Lazyingleton<Powerup>
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform pivot;
    [SerializeField] private Button button;
    [SerializeField] private Image icon;
    [System.NonSerialized] private Pawn.Usage _currentUsage;

    public bool Available => button.targetGraphic.raycastTarget;

    public void SetPowerup(Pawn.Usage usage)
    {
        this.gameObject.SetActive(true);
        canvas.enabled = true;
        
        this._currentUsage = usage;
        icon.sprite = Const.THIS.pawnIcons[(int)usage];

        
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
        Spawner.THIS.InterchangeBlock(Pool.Single_Block, this._currentUsage);

        
        pivot.DOKill();
        pivot.DOScale(Vector3.zero, 0.15f).SetEase(Ease.InBack).onComplete = () =>
        {
            canvas.enabled = false;
            this.gameObject.SetActive(false);
        };
    }

    public void Deconstruct()
    {
        
    }
}
