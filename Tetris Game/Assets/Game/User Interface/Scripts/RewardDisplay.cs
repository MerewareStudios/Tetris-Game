using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardDisplay : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] public RectTransform rectTransform;
    [SerializeField] private Image icon;
    [SerializeField] private Image background;
    [SerializeField] private TextMeshProUGUI bottomInfo;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] public ParticleSystem ps;
    [SerializeField] public Animator animator;

    public void SetSortingBehind()
    {
        canvas.sortingOrder = 6;
    }
}
