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
        }
    }

    public void DisplayDamage(int value, float scale = 1.0f)
    {
        damageText.text = value.ToString();
        
        damageText.rectTransform.DOKill();
        damageText.rectTransform.localScale = Vector3.one * scale;
        damageText.rectTransform.DOScale(Vector3.one * 0.4f, 0.25f).SetEase(Ease.Linear);

        damageText.color = Color.white;
        damageText.DOKill();
        damageText.DOColor(new Color(1.0f, 1.0f, 1.0f, 0.0f), 0.15f).SetDelay(0.1f).SetEase(Ease.OutSine);
    }
}
