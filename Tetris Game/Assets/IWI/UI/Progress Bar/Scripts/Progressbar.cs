using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Progressbar : MonoBehaviour
{
    [SerializeField] private RectTransform fillImage;

    public float Fill
    {
        set => fillImage.DOScale(new Vector3(value, 1.0f, 1.0f), 0.15f).SetEase(Ease.OutBack);
    }

    public bool Visible
    {
        set => this.gameObject.SetActive(value);
    }
}
