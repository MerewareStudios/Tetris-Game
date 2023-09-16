using System.Collections;
using System.Collections.Generic;
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

    public void Set(PiggyMenu.PiggyReward piggyReward, int sortingIndex)
    {
        Const.RewardData rewardData = piggyReward.type.RewardData();
        
        this.icon.sprite = rewardData.iconSprite;
        this.background.sprite = rewardData.backgroundSprite;
        bottomInfo.text = string.Format(rewardData.formatText, piggyReward.amount);
        title.text = rewardData.title;

        canvas.sortingOrder = 7 + sortingIndex;

        var main = ps.main;
        main.startColor = rewardData.color;
    }

    public void SetSortingBehind()
    {
        canvas.sortingOrder = 6;
    }
}
