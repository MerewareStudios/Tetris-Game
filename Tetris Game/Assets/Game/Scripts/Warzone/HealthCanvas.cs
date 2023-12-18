using DG.Tweening;
using TMPro;
using UnityEngine;

public class HealthCanvas : MonoBehaviour
{
    [SerializeField] public Canvas canvas;
    [SerializeField] private RectTransform healthRT;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI damageText;

    [System.NonSerialized] private int _lastFrame = -1;
    [System.NonSerialized] private int _lastValue = 0;
    
    public int Health
    {
        set
        {
            healthText.text = value.ToString();
            
            healthRT.DOKill();
            healthRT.localScale = Vector3.one;
            // if (value == 0)
            // {
            //     healthRT.DOScale(Vector3.zero, 0.175f).SetEase(Ease.InBack);
            // }
            // else
            // {
                healthRT.DOPunchScale(Vector3.one * 0.85f, 0.2f);
            // }
        }
    }

    public void Hide()
    {
        healthRT.DOScale(Vector3.zero, 0.1f).SetEase(Ease.Linear).SetDelay(0.2f);
    }

    public void DisplayDamage(int value, float scale = 1.0f)
    {
        if (Time.frameCount - _lastFrame <= 10)
        {
            value += _lastValue;
        }
        
        damageText.text = value.ToString();
        
        damageText.rectTransform.DOKill();
        damageText.rectTransform.localScale = Vector3.one * scale;
        damageText.rectTransform.DOScale(Vector3.one * 0.4f, 0.25f).SetEase(Ease.Linear);
        
      

        damageText.color = Color.white;
        damageText.DOKill();
        damageText.DOColor(new Color(1.0f, 1.0f, 1.0f, 0.0f), 0.15f).SetDelay(0.1f).SetEase(Ease.OutSine);

        _lastValue = value;
        _lastFrame = Time.frameCount;
    }
}
