using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;

public class BlockVisualGrid : MonoBehaviour
{
    [SerializeField] private GameObject[] blocks;
    [SerializeField] private TextMeshProUGUI[] texts;
    [SerializeField] private string ammoText;

    public void Display(int[] lookUp)
    {
        for (int i = 0; i < lookUp.Length; i++)
        {
            blocks[i].SetActive(lookUp[i] > 0);
            texts[i].text = GetAmmoImage(lookUp[i]);
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