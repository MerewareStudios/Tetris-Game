using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Tab : MonoBehaviour
{
    [SerializeField] private RectTransform animationPivot;
    [SerializeField] private RectTransform imagePivot;

    public Tab Show()
    {
        animationPivot.DOKill();
        animationPivot.DOLocalMove(Vector3.down * 25.0f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
        
        imagePivot.DOKill();
        imagePivot.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
        return this;
    }
    
    public Tab Hide()
    {
        animationPivot.DOKill();
        animationPivot.DOLocalMove(Vector3.down * 75.0f, 0.2f).SetEase(Ease.OutSine).SetUpdate(true);
        
        imagePivot.DOKill();
        imagePivot.DOScale(Vector3.one * 0.75f, 0.2f).SetEase(Ease.OutSine).SetUpdate(true);
        return this;
    }
}
