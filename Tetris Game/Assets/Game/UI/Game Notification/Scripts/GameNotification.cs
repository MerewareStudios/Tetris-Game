using DG.Tweening;
using TMPro;
using UnityEngine;

public class GameNotification : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI count;
    [System.NonSerialized] private int _current = 0;

    public int Count
    {
        set
        {
            if (_current == value)
            {
                return;
            }

            _current = value;
            

            if (_current == 0)
            {
                if (!gameObject.activeSelf)
                {
                    return;
                }

                transform.DOKill();
                transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack).SetDelay(0.15f).SetUpdate(true)
                    .onComplete = () =>
                {
                    gameObject.SetActive(false);
                };
                return;
            }

            transform.DOKill();

            if (this.gameObject.activeSelf)
            {
                transform.localScale = Vector3.one;
                transform.DOPunchScale(new Vector3(0.15f, 0.15f, 0.15f), 0.35f).OnStart(() =>
                {
                    count.text = value.ToString();
                }).SetUpdate(true).SetDelay(0.15f);
                return;
            }
            this.gameObject.SetActive(true);

            transform.localScale = Vector3.zero;
            transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack).SetUpdate(true);

            count.text = value.ToString();
        }
    }
    public int CountImmediate
    {
        set
        {
            if (_current == value)
            {
                return;
            }

            _current = value;
            count.text = value.ToString();
            
            transform.DOKill();
            transform.localScale = _current > 0 ? Vector3.one : Vector3.zero;
            gameObject.SetActive(_current > 0);
        }
    }
}
