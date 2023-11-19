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

    public void Set(PreviewData previewData)
    {
        SetTitle(previewData.title);
        SetIcon(previewData.sprite);
        SetMultText(previewData.mult);
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
    
    
    [System.Serializable]
    public class PreviewData
    {
        [SerializeField] public Sprite sprite;
        [TextArea] [SerializeField] public string title;
        [TextArea] [SerializeField] public string mult;
    }
}
