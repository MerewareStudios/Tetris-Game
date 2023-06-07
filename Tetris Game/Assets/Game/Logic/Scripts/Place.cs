using DG.Tweening;
using Game;
using Internal.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Game
{
    public class Place : MonoBehaviour
    {
        [SerializeField] public Transform segmentParent;
        [SerializeField] public SpriteRenderer SpriteRenderer;
        [System.NonSerialized] public Vector2Int index;
        [System.NonSerialized] private bool puffed = true;
        [System.NonSerialized] private Color targetColor;
        public Pawn Current { get; set; }
        public bool Occupied { get{ return Current != null; } }

        public bool Fade
        {
            set
            {
                if (puffed == value)
                {
                    return;
                }
                puffed = value;
                
                targetColor.a = value ? 0.25f : 1.0f;
                DoColor();
            }
        }

        public void Construct()
        {
            targetColor = GameManager.THIS.Constants.placeColorDefault;
            targetColor.a = 0.25f;
            SpriteRenderer.color = targetColor;
        }
        public void Deconstruct()
        {
            if (Current != null)
            {
                Current.Deconstruct();
            }
        }
        
        public void MarkFree()
        {
            SetTargetColorRGB(GameManager.THIS.Constants.placeColorHighlight);
            DoColor();
        }
        public void MarkOccupied()
        {
            SetTargetColorRGB(GameManager.THIS.Constants.placeColorDeny);
            DoColor();
        }
        public void MarkDefault()
        {
            SetTargetColorRGB(GameManager.THIS.Constants.placeColorDefault);
            DoColor();
        }

        private void SetTargetColorRGB(Color color)
        {
            targetColor.r = color.r;
            targetColor.g = color.g;
            targetColor.b = color.b;
        }

        private void DoColor()
        {
            SpriteRenderer.DOKill();
            SpriteRenderer.DOColor(targetColor, 0.1f);
        }

        void Start()
        {
            MarkDefault();
        }
        public void Accept(Pawn pawn, float duration, System.Action OnAccept = null)
        {
            pawn.transform.parent = segmentParent;
            pawn.Move(segmentParent.position, duration, Ease.Linear, () =>
            {
                this.Current = pawn;
                MarkDefault();
                OnAccept?.Invoke();
                pawn.CheckSteady(this, false);
            });
        }
        public void AcceptImmidiate(Pawn pawn)
        {
            pawn.Move(segmentParent, segmentParent.position);
            this.Current = pawn;
        }

    }
}