using System.Collections.Generic;
using DG.Tweening;
using Game;
using UnityEngine;
using UnityEngine.UI;

public class NextBlockDisplay : MonoBehaviour
{
    [SerializeField] private GameObject[] nextBlockPawns;
    [SerializeField] private RectTransform progress;
    [SerializeField] private int leftOverCount = 0;
    [SerializeField] private Button showButton;
    [SerializeField] private GameObject priceTag;
    [SerializeField] private GameObject questionMark;
    [SerializeField] private GameObject blockPanel;
    [SerializeField] private GameObject plusButton;
    private const int MaxLeftOverCount = 25;
    
    public bool Visible
    {
        get => this.gameObject.activeSelf;
        set
        {
        #if CREATIVE
            this.gameObject.SetActive(Const.THIS.creativeSettings.nextBlockEnabled && value);
            return;
        #endif
            this.gameObject.SetActive(value);
        }
    }

    public void RemoveNextBlockLimit()
    {
        Board.THIS.SavedData.unlimitedPeek = true;
        Available = true;
    }


    public bool Available
    {
        set
        {
            leftOverCount = MaxLeftOverCount;

            bool unlimited = Board.THIS.SavedData.unlimitedPeek;

            blockPanel.gameObject.SetActive(value || unlimited);
            
            showButton.gameObject.SetActive(!value && !unlimited);
            priceTag.gameObject.SetActive(!value && !unlimited);
            questionMark.gameObject.SetActive(!value && !unlimited);
            progress.gameObject.SetActive(value && !unlimited);
            plusButton.gameObject.SetActive(true && !unlimited);
        }
        get => true;
    }
    

    public void Display(Pool blockType)
    {
        List<Transform> segmentTransforms = blockType.Prefab<Block>().segmentTransforms;
        for (int i = 0; i < segmentTransforms.Count; i++)
        {
            nextBlockPawns[i].SetActive(segmentTransforms[i]);
        }

        if (Board.THIS.SavedData.unlimitedPeek)
        {
            return;
        }
        
        leftOverCount--;
        if (leftOverCount == 0)
        {
            Available = false;
            // SetNextBlockVisibility(false);
            return;
        }
        
        progress.DOSizeDelta(new Vector2(100.0f * (leftOverCount / (float)MaxLeftOverCount), 7.24f), 0.2f);
    }
    
}
