using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Progressbar : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    public float Fill
    {
        set => fillImage.rectTransform.localScale = new Vector3(value, 1.0f, 1.0f);
    }

    public bool Visible
    {
        set => this.gameObject.SetActive(value);
    }
}
