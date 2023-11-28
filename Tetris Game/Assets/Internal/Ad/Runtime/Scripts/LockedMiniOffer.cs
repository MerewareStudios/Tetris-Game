using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LockedMiniOffer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI centerText;
    [SerializeField] private Image fill;
    [SerializeField] private MiniOffer miniOffer;
    [System.NonSerialized] private Data _savedData;

    public LockedMiniOffer.Data SavedData
    {
        get => _savedData;
        set
        {
            _savedData = value;
            centerText.text = "LEVEL " + value.unlockedAt;
        }
    }

    public LockedMiniOffer Set(int current)
    {
        fill.fillAmount = current / (float)_savedData.unlockedAt;
        miniOffer.gameObject.SetActive(current == _savedData.unlockedAt);
        this.gameObject.SetActive(current != _savedData.unlockedAt);
        return this;
    }
    public int UnlockedAt
    {
        set => _savedData.unlockedAt = value;
        get => _savedData.unlockedAt;
    }
    
    [System.Serializable]
    public class Data : System.ICloneable
    {
        [SerializeField] public int unlockedAt = 15;
            
        public Data()
        {
                
        }
        public Data(Data data)
        {
            this.unlockedAt = data.unlockedAt;
        }

        public object Clone()
        {
            return new Data(this);
        }
    }
}
