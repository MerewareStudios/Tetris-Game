using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[ExecuteAlways]
public class CircleLayoutGroup : MonoBehaviour
{
    [SerializeField] private float distance = 1.0f;
    [SerializeField] private Vector3 startDirection = new Vector3(0.0f, 1.0f, 0.0f);
    [SerializeField] private Vector2 size = new Vector2(100, 100);
    void Update()
    {
        int activeChildCount = 0;

        foreach (Transform t in transform)
        {
            activeChildCount += t.gameObject.activeSelf ? 1 : 0;
        }
        List<Vector3> points = GetPoints( transform.position, activeChildCount, startDirection, distance);
        for (int i = 0; i < points.Count; i++)
        {
            RectTransform currentTransform = transform.GetChild(i) as RectTransform;
            currentTransform.position = points[i];
            currentTransform.sizeDelta = size;
        }
    }

    public static List<Vector3> GetPoints(Vector3 center, int elementCount, Vector3 dir, float radius)
    {
        List<Vector3> points = new();
        
        float angleAdded = 360.0f / elementCount;

        for (int i = 0; i < elementCount; i++)
        {
            Vector3 direction = Quaternion.Euler(0.0f, 0.0f, angleAdded * i) * dir;
            points.Add( center + direction * radius);
        }

        return points;
    }
}
