using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Tab : MonoBehaviour
{
    [SerializeField] private RectTransform animationPivot;

    public void Show()
    {
        animationPivot.DOKill();
        animationPivot.DOLocalMove(Vector3.down * 25.0f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
    }
    
    public void Hide()
    {
        animationPivot.DOKill();
        animationPivot.DOLocalMove(Vector3.down * 75.0f, 0.2f).SetEase(Ease.OutSine).SetUpdate(true);
    }
}
