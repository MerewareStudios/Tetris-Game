using DG.Tweening;
using Febucci.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PurchaseOption : MonoBehaviour
{
    [SerializeField] private Image glimmerFrame;
    [SerializeField] private CurrenyButton purchaseButton;
    [SerializeField] public RectTransform animationPivot;
    [SerializeField] private CurrencyDisplay priceCurrencyDisplay;
    [SerializeField] private TextAnimator_TMP titleText;
    [SerializeField] private TextMeshProUGUI detailedInfoText;
    [SerializeField] private Image frameOutline;
    [SerializeField] private Image icon;
    [SerializeField] private GameObject bestBadge;
    [System.NonSerialized] private Sequence _colorSequence;

    public PurchaseOption SetPurchaseText(string text)
    {
        purchaseButton.Set(text, true);
        return this;
    }
    public PurchaseOption SetIcon(Sprite sprite)
    {
        icon.sprite = sprite;
        return this;
    }
    public PurchaseOption SetPrice(string currency, Const.CurrencyType currencyType, bool available)
    {
        priceCurrencyDisplay.Display(currency);
        purchaseButton.ButtonSprite = Const.THIS.buttonTextures[(int)currencyType];
        purchaseButton.Available = available;
        return this;
    }
    public PurchaseOption SetBestBadge(bool value)
    {
        bestBadge.SetActive(value);
        return this;
    }
    public PurchaseOption SetInfo(string title, string details)
    {
        titleText.SetText(title);
        detailedInfoText.SetText(details);
        return this;
    }
    public void PunchColor(Color punchColor, Color defaultColor)
    {
        _colorSequence?.Kill();
        Tween punchColorTween = frameOutline.DOColor(punchColor, 0.1f).SetEase(Ease.Linear);
        Tween defaultColorTween = frameOutline.DOColor(defaultColor, 0.1f).SetEase(Ease.Linear).SetDelay(0.5f);

        _colorSequence = DOTween.Sequence().SetUpdate(true);
        _colorSequence.Append(punchColorTween);
        _colorSequence.Append(defaultColorTween);
    }
    public void Punch(Vector2 anchor)
    {
        animationPivot.DOKill();
        animationPivot.anchoredPosition = Vector2.zero;
        animationPivot.DOPunchAnchorPos(anchor, 0.25f).SetUpdate(true);
    }
    public void PunchScale(float amount, float delay)
    {
        transform.DOKill();
        transform.localScale = Vector3.one;
        transform.DOPunchScale(Vector3.one * amount, 0.35f, 1).SetUpdate(true).SetDelay(delay);
    }

    public void GlimmerByBadge()
    {
        if (bestBadge.activeSelf)
        {
            glimmerFrame.Glimmer(AnimConst.THIS.glimmerSpeedUpgrade);
        }
    }
    public void Glimmer()
    {
        glimmerFrame.Glimmer(AnimConst.THIS.glimmerSpeedUpgrade);
    }
}
