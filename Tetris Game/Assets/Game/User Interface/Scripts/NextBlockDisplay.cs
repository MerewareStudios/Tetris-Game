using System.Collections;
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
    private const int MaxLeftOverCount = 25;
    
    public bool Visible
    {
        get => this.gameObject.activeSelf;
        set => this.gameObject.SetActive(value);
    }

    public bool Available
    {
        set
        {
            leftOverCount = MaxLeftOverCount;

            showButton.gameObject.SetActive(!value);
            priceTag.gameObject.SetActive(!value);
            questionMark.gameObject.SetActive(!value);
            blockPanel.gameObject.SetActive(value);
        }
        get => true;
    }
    

    // public void SetNextBlockVisibility(bool visible)
    // {
    //     if(visible)
    //     {
    //         leftOverCount = MaxLeftOverCount;
    //         DisplayNextBlock();
    //     }
    //     nextBlockVisual.SetActive(true);
    //     nextBlockVisual.transform.DOKill();
    //     nextBlockVisual.transform.DOScale(visible ? Vector3.one : Vector3.zero, 0.25f).SetEase(visible ? Ease.OutBack : Ease.InBack).onComplete =
    //         () =>
    //         {
    //             nextBlockVisual.SetActive(visible);
    //         };
    // }

    public void Display(Pool blockType)
    {
        leftOverCount--;
        if (leftOverCount == 0)
        {
            Available = false;
            // SetNextBlockVisibility(false);
            return;
        }
        
        List<Transform> segmentTransforms = blockType.Prefab<Block>().segmentTransforms;
        for (int i = 0; i < segmentTransforms.Count; i++)
        {
            nextBlockPawns[i].SetActive(segmentTransforms[i]);
        }
        progress.DOSizeDelta(new Vector2(100.0f * (leftOverCount / (float)MaxLeftOverCount), 7.24f), 0.2f);
    }
    
}
