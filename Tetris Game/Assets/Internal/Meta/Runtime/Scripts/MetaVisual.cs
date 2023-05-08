using DG.Tweening;
using Internal.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MetaVisual : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private RectTransform punchPivot;
    [SerializeField] private Image image;
    [SerializeField] private float defaultAlpha = 0.8f;
    [System.NonSerialized] private int currentAmount;

    public string Key { get; set; }
    public int Amount
    {
        set
        {
            text.text = Key + value;


            PunchUp((value > currentAmount) ? 15.0f : -5.0f);

            currentAmount = value;
        }
    }

    public void PunchUp(float mag)
    {
        punchPivot.DOKill();
        punchPivot.localPosition = Vector3.zero;
        punchPivot.PunchUp(mag, 0.25f);
    }
    public void Reject()
    {
        punchPivot.DOKill();
        punchPivot.localPosition = Vector3.zero;
        punchPivot.PunchRight(5.0f, 0.35f);
    }
    public void SetAlpha(float alpha)
    {
        text.SetAlpha(alpha);
        image.SetAlpha(alpha);
    }

    public void Hide(bool value)
    {
        text.SetAlpha(value ? 0.25f : 1.0f);
        image.SetAlpha(value ? 0.25f : defaultAlpha);
    }
}
