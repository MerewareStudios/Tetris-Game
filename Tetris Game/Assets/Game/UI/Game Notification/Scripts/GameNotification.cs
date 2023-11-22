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
            transform.DOKill();
            this.gameObject.SetActive(value > 0);
            if (value <= 0)
            {
                return;
            }

            transform.localScale = Vector3.one;
            // transform.DOScale(Vector3.one, 0.25f).SetDelay(0.15f).SetEase(Ease.OutBack).SetUpdate(true);
            
            count.text = value.ToString();
        }
    }

    public void Close()
    {
        transform.DOKill();
        transform.DOScale(Vector3.zero, 0.25f).SetDelay(0.15f).SetEase(Ease.InBack).SetUpdate(true);
    }
}
