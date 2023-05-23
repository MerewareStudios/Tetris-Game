using DG.Tweening;
using Internal.Core;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using TMPro;
using UnityEngine;

namespace Game
{
    public class Block : MonoBehaviour
    {
        [SerializeField] private List<Transform> segmentTransforms;
        [System.NonSerialized] private List<Segment> segments = new();
        [SerializeField] private Vector3 spawnerOffset;

        public void Construct()
        {
            this.transform.localPosition = spawnerOffset;
            this.transform.localScale = Vector3.one;
            this.transform.localRotation = Quaternion.identity;

            foreach (var s in segmentTransforms)
            {
                Segment segment = Pool.Segment___Level_1.Spawn<Segment>(this.transform);
                segment.transform.position = s.position;
                segment.transform.localRotation = Quaternion.identity;
                segment.transform.localScale = Vector3.one;

                segments.Add(segment);
            }
        }

        public void Check()
        {
            foreach (var segment in segments)
            {
                segment.Check();
            }
        }

        private void OnDrawGizmos()
        {
            foreach (var s in segmentTransforms)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawCube(s.transform.position, Vector3.one * 0.95f * transform.localScale.x);
            }
        }

        public enum Type
        {
            I,
            J,
            L,
            O,
            S,
            T,
            Z
        }
        public enum Direction
        {
            NONE,
            FORWARD,
            BACKWARD,
            LEFT,
            RIGHT
        }
    }
}
