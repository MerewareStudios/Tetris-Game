using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CircularProgress : MonoBehaviour
{
    [SerializeField] private Image image;

    public float Fill
    {
        set => image.fillAmount = value;
    }
    
    public float FillAnimated
    {
        set => image.DOFillAmount(value, 0.2f).SetEase(Ease.Linear);
    }

    public void Kill()
    {
        image.DOKill();
    }
}
