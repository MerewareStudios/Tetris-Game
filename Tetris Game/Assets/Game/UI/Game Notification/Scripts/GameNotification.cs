using DG.Tweening;
using TMPro;
using UnityEngine;

public class GameNotification : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI count;

    public int Count
    {
        set
        {
            count.text = value.ToString();
            transform.DOKill();
            
            bool visible = value > 0;
            if (visible)
            {
                transform.localScale = Vector3.one;
                transform.DOPunchScale(new Vector3(0.15f, 0.15f, 0.15f), 0.35f).SetDelay(0.15f).SetUpdate(true);
            }
            else
            {
                transform.DOScale(Vector3.zero, 0.25f).SetDelay(0.15f).SetEase(Ease.InBack).SetUpdate(true);
            }
        }
    }
    public void Close()
    {
        transform.DOKill();
        transform.DOScale(Vector3.zero, 0.25f).SetDelay(0.3f).SetEase(Ease.InBack).SetUpdate(true);
    }
}
