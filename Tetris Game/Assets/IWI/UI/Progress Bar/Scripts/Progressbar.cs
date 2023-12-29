using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Progressbar : MonoBehaviour
{
    [SerializeField] private RectTransform fillImage;

    public float Fill
    {
        set => fillImage.localScale = new Vector3(value, 1.0f, 1.0f);
    }

    public bool Visible
    {
        set => this.gameObject.SetActive(value);
    }
}
