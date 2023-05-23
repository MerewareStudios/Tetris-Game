using Internal.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Place : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;

    void Start()
    {
        Highlight = false;
    }

    public bool Highlight
    {
        set 
        { 
            Color color = value ? GameManager.THIS.Constants.placeColorHighlight : GameManager.THIS.Constants.placeColorDefault;
            meshRenderer.SetColor(GameManager.MPB_PLACE, "_BaseColor", color);    
        }
    }
}
