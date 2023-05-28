using DG.Tweening;
using Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Line : MonoBehaviour
    {
        [System.NonSerialized] private List<Segment> segments = new();
        public void Accept(Segment segment)
        {
            segments.Add(segment);

            Transform segmentT = segment.transform;
            segmentT.parent = this.transform;
            segmentT.DOKill();
            segmentT.DOLocalMove(new Vector3(-5.0f + 2.5f * (segments.Count - 1), 0.0f, 0.0f), GameManager.THIS.Constants.segmentAcceptDuration).SetEase(GameManager.THIS.Constants.segmentAcceptEase)
                 .onComplete += () =>
                 {

                 };
        }
    }
}