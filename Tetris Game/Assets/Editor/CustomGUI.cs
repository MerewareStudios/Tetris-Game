using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[InitializeOnLoad]
public class CustomGUI : MonoBehaviour
{
    [System.NonSerialized] private static CustomGUIData customGUIData;
    static CustomGUI()
    {
        EditorApplication.hierarchyWindowItemOnGUI += Callback;
        customGUIData = AssetDatabase.LoadAssetAtPath("Assets/Editor/Custom GUI Data.asset", typeof(CustomGUIData)) as CustomGUIData;
    }
    
    private static void Callback(int instanceID, Rect selectionRect)
    {
        if (Selection.activeInstanceID == instanceID)
        {
            if (customGUIData.showDefaultOnSelection)
            {
                return;
            }
        }
    
        if (selectionRect.Contains(Event.current.mousePosition))
        {
            if (customGUIData.showDefaultOnMouseHover)
            {
                return;
            }
        }
        var obj = EditorUtility.InstanceIDToObject(instanceID);
        if (obj != null)
        {
            var prefabType = PrefabUtility.GetPrefabAssetType(obj);
            if (prefabType != PrefabAssetType.NotAPrefab)
            {
                return;
            }
            foreach (var data in customGUIData.tags)
            {
                if (obj.name.Contains(data.character, System.StringComparison.InvariantCultureIgnoreCase))
                {
                    Draw(obj, data, selectionRect);
                    return;
                }
            }
            Draw(obj, customGUIData.defaultProperties, selectionRect);
        }
    }
    
    public static void Draw(Object obj, CustomGUIData.Tag data, Rect selectionRect)
    {
        EditorGUI.DrawRect(selectionRect, data.backgroundColor);
        string name = obj.name;
        if (data.removeCharacter)
        {
            name = obj.name.Trim(data.character);
        }
        if (data.toUpperCase)
        {
            name = name.ToUpper();
        }
        GUIContent content = new GUIContent(name, data.tooltip);
        Color color = data.textColor;
        color.a = ((GameObject)obj).activeSelf ? color.a : (color.a * 0.45f);
        EditorGUI.LabelField(selectionRect, content, new GUIStyle()
        {
            normal = new GUIStyleState() { textColor = color },
            fontStyle = data.fontStyle,
            alignment = data.textAnchor,
            fontSize = data.fontSize
        });
        GUIContent contentIcon = new GUIContent(data.texture, data.tooltip);
        Rect rect = selectionRect;
        //selectionRect.x = selectionRect.width * data.texturePadding;
        rect.width *= data.texturePadding;
        EditorGUI.LabelField(rect, contentIcon, new GUIStyle()
        {
            alignment = TextAnchor.MiddleRight,
        });
    }
}
