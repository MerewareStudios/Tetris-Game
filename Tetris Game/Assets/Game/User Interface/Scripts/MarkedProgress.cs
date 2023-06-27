using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MarkedProgress : MonoBehaviour
{
    [Range(0.0f, 1.0f)] [SerializeField] private float progress;
    [SerializeField] private Image fill;
    [SerializeField] private Transform startMark;
    [SerializeField] private Transform endMark;
    [SerializeField] private Transform currentMark;

    public float _Progress
    {
        set
        {
            progress = value;
            currentMark.position = Vector3.Lerp(startMark.position, endMark.position, progress);
            fill.fillAmount = progress;
        }
        get => progress;
    }
    
    private void OnValidate()
    {
        _Progress = progress;
    }

}

