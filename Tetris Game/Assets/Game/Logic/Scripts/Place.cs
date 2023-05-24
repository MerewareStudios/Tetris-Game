using DG.Tweening;
using Game;
using Internal.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Place : MonoBehaviour
    {
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] public Transform segmentParent;
        [System.NonSerialized] public Segment currentSegment;

        void Start()
        {
            Highlight = false;
        }

        public bool Highlight
        {
            set 
            { 
                Color color = value ? GameManager.THIS.Constants.placeColorHighlight : GameManager.THIS.Constants.placeColorDefault;
                meshRenderer.SetColor(GameManager.MPB_PLACE, "_BaseColor", color);    
            }
        }
        public bool Deny
        {
            set
            {
                Color color = value ? GameManager.THIS.Constants.placeColorDeny : GameManager.THIS.Constants.placeColorDefault;
                meshRenderer.SetColor(GameManager.MPB_PLACE, "_BaseColor", color);
            }
        }
        public bool Occupied
        {
            get
            {
                return currentSegment != null;
            }
        }
        public void Accept(Segment segment)
        {
            Transform segmentT = segment.transform;
            segmentT.parent = this.segmentParent;
            segmentT.DOKill();
            segmentT.DOLocalMove(Vector3.zero, GameManager.THIS.Constants.segmentAcceptDuration).SetEase(GameManager.THIS.Constants.segmentAcceptEase)
                 .onComplete += () =>
                  {
                      Highlight = false;
                      this.currentSegment = segment;
                      OnAcceptComplete();
                  };
        }
        public void Clear()
        {
            currentSegment = null;
        }
        private void OnAcceptComplete()
        {
            //Map.THIS.AddSegment(this.currentSegment);
        }
    }
}