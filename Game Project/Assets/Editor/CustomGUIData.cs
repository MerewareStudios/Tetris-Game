using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Custom GUI Data", menuName = "Internal/Unity/GUI/Custom GUI Data", order = 0)]
public class CustomGUIData : ScriptableObject
{
    [SerializeField] public bool showDefaultOnSelection = true;
    [SerializeField] public bool showDefaultOnMouseHover = true;
    [SerializeField] public Tag defaultProperties;
    [SerializeField] public List<Tag> tags;

    [System.Serializable]
    public class Tag
    {
        [SerializeField] public char character;
        [SerializeField] public string tooltip;
        [SerializeField] public Texture2D texture;
        [Range(0.0f, 1.0f)][SerializeField] public float texturePadding = 0.25f;
        [SerializeField] public bool removeCharacter = true;
        [SerializeField] public Color textColor;
        [SerializeField] public Color backgroundColor;
        [SerializeField] public FontStyle fontStyle;
        [SerializeField] public TextAnchor textAnchor;
        [SerializeField] public int fontSize = 13;
        [SerializeField] public bool toUpperCase;
    }
}
