using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class HealthCanvas : MonoBehaviour
{
    [SerializeField] public Canvas canvas;
    [SerializeField] private RectTransform healthRT;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI damageText;
    
    public int Health
    {
        set
        {
            healthText.text = value <= 0 ? "" : value.ToString();

            // healthRT.DOKill();
            // healthRT.localPosition = Vector3.zero;
            // healthRT.DOPunchAnchorPos(new Vector2(0.0f, 10.0f), 0.75f, 1);
        }
    }

    public void DisplayDamage(int value)
    {
        damageText.text = value.ToString();
        
        damageText.rectTransform.DOKill();
        damageText.rectTransform.localScale = Vector3.one;
        damageText.rectTransform.DOScale(Vector3.one * 0.4f, 0.25f).SetEase(Ease.Linear);
        // damageText.rectTransform.anchoredPosition = new Vector2(0.0f, 0.0f);
        // damageText.rectTransform.DOAnchorPosY(-35.0f, 0.4f).SetRelative(true).SetEase(Ease.OutSine);

        damageText.color = Color.white;
        damageText.DOKill();
        damageText.DOColor(new Color(1.0f, 1.0f, 1.0f, 0.0f), 0.15f).SetDelay(0.1f).SetEase(Ease.OutSine);
    }
}
