using UnityEngine;
using UnityEngine.UI;

public class CircularProgress : MonoBehaviour
{
    [SerializeField] private Image image;

    public float Fill
    {
        set => image.fillAmount = value;
    }
}
