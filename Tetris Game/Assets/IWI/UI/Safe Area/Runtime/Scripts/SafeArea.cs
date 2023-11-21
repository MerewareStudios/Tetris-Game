using UnityEngine;

public class SafeArea : MonoBehaviour
{
    [SerializeField] private bool safeBottom = true;
    [SerializeField] private bool safeTop = true;
    private void Awake()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        var safeArea = Screen.safeArea;

        var minAnchor = safeArea.position;
        var maxAnchor = safeArea.position + safeArea.size;
        minAnchor.x /= Screen.width;
        minAnchor.y /= Screen.height;
        maxAnchor.x /= Screen.width;
        maxAnchor.y /= Screen.height;

        if (safeBottom)
        {
            rectTransform.anchorMin = minAnchor;
        }

        if (safeTop)
        {
            rectTransform.anchorMax = maxAnchor;
        }
    }
}
