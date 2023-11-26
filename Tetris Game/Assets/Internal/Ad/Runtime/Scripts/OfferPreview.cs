using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OfferPreview : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI multText;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private GameObject plus;
    [SerializeField] private GameObject freeBadge;

    public void Set(PreviewData previewData)
    {
        SetTitle(previewData.title);
        SetIcon(previewData.sprite);
        SetMultText(previewData.mult);
        SetFree(previewData.free);
    }

    public void SetPlusState(bool plusEnabled)
    {
        plus.SetActive(plusEnabled);
    }
    
    private void SetIcon(Sprite sprite)
    {
        icon.sprite = sprite;
    }
    
    private void SetMultText(string str)
    {
        multText.text = str;
    }
    
    private void SetTitle(string str)
    {
        title.text = str;
    }
    private void SetFree(bool free)
    {
        freeBadge.SetActive(free);
    }
    
    [System.Serializable]
    public class PreviewData
    {
        [SerializeField] public Sprite sprite;
        [SerializeField] public bool free;
        [TextArea] [SerializeField] public string title;
        [TextArea] [SerializeField] public string mult;
    }
}
