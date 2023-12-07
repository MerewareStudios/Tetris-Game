using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using DG.Tweening;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class BlockVisualGrid : MonoBehaviour
{
    [SerializeField] private GameObject[] blocks;
    [SerializeField] private Image[] bullets;
    [SerializeField] private string ammoText;

    public void Display(List<Transform> segments)
    {
        for (int i = 0; i < segments.Count; i++)
        {
            blocks[i].SetActive(segments[i]);
            if (blocks[i].activeSelf)
            {
                blocks[i].transform.DOKill();
                blocks[i].transform.localScale = Vector3.one;
                blocks[i].transform.DOPunchScale(Vector3.one * 0.2f, 0.25f, 1).SetUpdate(true);
            }
        }
    }

    private string GetAmmoImage(int count)
    {
        StringBuilder stringBuilder = new();
        for (int i = 0; i < count; i++)
        {
            stringBuilder.Append(ammoText);
        }

        return stringBuilder.ToString();
    }
    
}