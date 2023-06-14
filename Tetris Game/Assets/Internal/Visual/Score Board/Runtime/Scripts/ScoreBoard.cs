using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Internal.Core;
using TMPro;
using UnityEngine;

public class ScoreBoard : Singleton<ScoreBoard>
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private RectTransform animationPivot;

    public delegate int Load();
    public static Load OnLoad;
    public delegate void Save(int amount);
    public static Save OnSave;

    public int Score
    {
        get => OnLoad.Invoke();
        set
        {
            OnSave.Invoke(value);

            scoreText.text = value.ToString();
            
            Punch(0.25f);
        }
    }

    private void Punch(float amount)
    {
        animationPivot.DOKill();
        animationPivot.localScale = Vector3.one;
        animationPivot.DOPunchScale(Vector3.one * amount, 0.25f);
    }
}
