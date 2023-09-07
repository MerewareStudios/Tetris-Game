using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Glimmer : MonoBehaviour
{
    public static System.Action<Glimmer> OnComplete;
    [SerializeField] private RectTransform _rectTransform;
    
    public void Show(Image image, RectTransform parent, float speed, Ease ease)
    {
        this._rectTransform.DOKill();
        
        this._rectTransform.SetParent(parent);
        this._rectTransform.SetAsLastSibling();
        this._rectTransform.localPosition = Vector3.zero;
        this._rectTransform.localScale = new Vector3(2.5f, 8.0f, 1.0f);
        this._rectTransform.localEulerAngles = new Vector3(0.0f, 0.0f, -45.0f);

        Vector2 size = parent.rect.size * 0.8f;
        this._rectTransform.anchoredPosition = new Vector2(-size.x, size.y);
        this._rectTransform.DOAnchorPos(new Vector2(size.x, -size.y), speed).SetSpeedBased(true).SetEase(ease).SetUpdate(true)
            .onComplete = () => OnComplete.Invoke(this);
    }
}
