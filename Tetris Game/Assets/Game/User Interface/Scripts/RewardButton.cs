using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardButton : MonoBehaviour
{
    [System.NonSerialized] private System.Action OnClickAction;
    [SerializeField] private Button ActionButton;
    [SerializeField] private Image piggyIcon;
    [SerializeField] private TextMeshProUGUI iconText;
    [SerializeField] private TextMeshProUGUI amountText;
    
    public RewardButton OnClick(System.Action OnClick)
    {
        this.OnClickAction = OnClick;
        return this;
    }

    public void OnClick()
    {
        OnClickAction?.Invoke();
    }

    public RewardButton Show(Vector3 start, Vector3 end, float delay)
    {
        this.gameObject.SetActive(true);
        
        iconText.gameObject.SetActive(false);
        amountText.gameObject.SetActive(false);
        
        piggyIcon.rectTransform.localScale = Vector3.one;
        piggyIcon.color = Color.white;
        piggyIcon.enabled = true;

        RectTransform thisTransform = transform as RectTransform;
        thisTransform.DOKill();
        thisTransform.position = start;
        thisTransform.localScale = Vector3.zero;
        thisTransform.DOLocalMove(end, 0.35f).SetEase(Ease.OutBack).SetDelay(delay).SetUpdate(true);
        thisTransform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack).SetDelay(delay).SetUpdate(true).onComplete +=
            () =>
            {
                ActionButton.image.raycastTarget = true;
            };
        return this;
    }
}
