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
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] public Transform segmentParent;
        [System.NonSerialized] public Vector2Int index;
        public Pawn Current { get; set; }
        public bool Occupied { get{ return Current != null; } }
        public void MarkFree()
        {
            meshRenderer.SetColor(GameManager.MPB_PLACE, "_BaseColor", GameManager.THIS.Constants.placeColorHighlight);
        }
        public void MarkOccupied()
        {
            meshRenderer.SetColor(GameManager.MPB_PLACE, "_BaseColor", GameManager.THIS.Constants.placeColorDeny);
        }
        public void MarkDefault()
        {
            meshRenderer.SetColor(GameManager.MPB_PLACE, "_BaseColor", GameManager.THIS.Constants.placeColorDefault);
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
                pawn.CheckSteady(this);
            });
        }
        public void AcceptImmidiate(Pawn pawn)
        {
            pawn.Move(segmentParent, segmentParent.position);
            this.Current = pawn;
        }

    }
}