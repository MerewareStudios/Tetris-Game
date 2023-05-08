using CW.Common;
using Internal.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PoolManager))]
[CanEditMultipleObjects]
public class PoolManagerGUI : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button(new GUIContent("REFRESH", "Convert to hard coded indexes.")) == true)
        {
            AutoGenerate.GeneratePool();
        }
        DrawDefaultInspector();
    }
}
