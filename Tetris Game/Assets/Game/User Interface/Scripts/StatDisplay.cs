using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
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

    private bool SetValue(int value)
    {
        if (_currentValue == value)
        {
            return true;
        }

        _currentValue = value;
        text.text = value.ToString();
        return false;
    }

    public void Show(int value, float timePercent, bool punch, bool setFront)
    {
        if (punch)
        {
            Punch();
        }

        timerFill.fillAmount = timePercent;
        
        if (SetValue(value))
        {
            return;
        }

        if (setFront && !this.gameObject.activeSelf)
        {
            this.transform.SetAsFirstSibling();
        }
        
        this.gameObject.SetActive(true);
        animationPivot.DOKill();
        animationPivot.localScale = Vector3.one;
        animationPivot.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack);
    }
    
    public void Hide()
    {
        if (!this.gameObject.activeSelf)
        {
            return;
        }
        _currentValue = -1;
        animationPivot.DOKill();
        animationPivot.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).onComplete += () =>
        {
            this.gameObject.SetActive(false);
        };
    }
    public void HideImmediate()
    {
        _currentValue = -1;
        this.gameObject.SetActive(false);
    }
    
    public void Punch()
    {
        punchPivot.DOKill();
        punchPivot.localScale = Vector3.one;
        punchPivot.DOPunchScale(Vector3.one * 0.25f, 0.25f, 1);
    }

    [Serializable]
    public enum Type
    {
        Health,
        // Damage,
        // Splitshot,
        // Firerate,
        Shield,
    }
}