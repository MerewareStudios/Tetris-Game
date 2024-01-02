using DG.Tweening;
using UnityEngine;

public class Progressbar : MonoBehaviour
{
    [SerializeField] private RectTransform fillImage;
    [SerializeField] private Vector2 maxSize;

    public float Fill
    {
        set => fillImage.DOSizeDelta(new Vector2(maxSize.x * value, maxSize.y), 0.25f).SetDelay(0.15f).SetEase(Ease.InOutBack);
    }

    public bool Visible
    {
        set => this.gameObject.SetActive(value);
    }
}
