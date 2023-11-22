using TMPro;
using UnityEngine;

public class GameNotification : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI count;

    public int Count
    {
        set
        {
            this.gameObject.SetActive(value > 0);
            if (value <= 0)
            {
                return;
            }

            count.text = value.ToString();
        }
    }
}
