using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatDisplay : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Image timerFill;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] public Transform animationPivot;
    [SerializeField] private Transform punchPivot;
    [System.NonSerialized] private int _currentValue = -1;

    public void SetIcon(Sprite sprite)
    {
        image.sprite = sprite;
    }

    private bool SetAmount(int value, bool markSpecial = false)
    {
        if (_currentValue == value)
        {
            return true;
        }

        _currentValue = value;
        // text.color = markSpecial ? Onboarding.THIS.specialStatColor : Onboarding.THIS.normalStatColor;
        text.text = markSpecial ? ("x" + value) : value.ToString();
        return false;
    }

    public void Show(int value, float timePercent, bool punch)
    {
        if (punch)
        {
            Punch();
        }

        timerFill.fillAmount = timePercent;
        
        if (SetAmount(value))
        {
            return;
        }

        this.gameObject.SetActive(true);
        animationPivot.DOKill();
        animationPivot.localScale = Vector3.one;
        animationPivot.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack).SetUpdate(true);
    }
    
    public void UpdatePercent(float timePercent)
    {
        timerFill.fillAmount = timePercent;
    }
    public void UpdateAmount(int amount, float punch, bool markSpecial = false)
    {
        if (SetAmount(amount, markSpecial))
        {
            return;
        }
        
        Punch(punch);

        this.gameObject.SetActive(true);
        animationPivot.DOKill();
        animationPivot.localScale = Vector3.one;
        animationPivot.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack).SetUpdate(true);    
    }
    public void Hide()
    {
        if (!this.gameObject.activeSelf)
        {
            return;
        }
        _currentValue = -1;
        animationPivot.DOKill();
        animationPivot.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).SetUpdate(true).onComplete += () =>
        {
            this.gameObject.SetActive(false);
        };
    }
    public void HideImmediate()
    {
        _currentValue = -1;
        this.gameObject.SetActive(false);
    }
    
    public void Punch(float amount = 0.5f)
    {
        punchPivot.DOKill();
        punchPivot.localScale = Vector3.one;
        punchPivot.DOPunchScale(Vector3.one * amount, 0.25f, 1).SetUpdate(true);
    }

    [Serializable]
    public enum Type
    {
        Health,
        Shield,
        Boost,
        // Firerate,
        // Splitshot,
    }
}