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
    [SerializeField] public ParticleSystem ps;

    public void Set(PiggyMenu.PiggyReward piggyReward, int sortingIndex)
    {
        Const.RewardData rewardData = piggyReward.type.RewardData();
        
        this.icon.sprite = rewardData.iconSprite;
        this.background.sprite = rewardData.backgroundSprite;
        bottomInfo.text = "+" + piggyReward.amount + " " + rewardData.infoPostfix.ToUpper();

        canvas.sortingOrder = 7 + sortingIndex;

        var main = ps.main;
        main.startColor = rewardData.color;
    }

    public void SetSortingBehind()
    {
        canvas.sortingOrder = 7;
    }
}
