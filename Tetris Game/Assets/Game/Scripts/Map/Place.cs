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
        [System.NonSerialized] private PlaceType _placeType = PlaceType.FREE;
        [System.NonSerialized] private bool _merger = false;
        public Pawn Current { get; set; }
        public bool Occupied => Current;

        public bool Merger
        {
            get
            {
                return _merger;
            }
            set
            {
                _merger = value;
                SetColor(PlaceType.EMPTY);
            }
        }

        public void Construct()
        {
            this._placeType = PlaceType.FREE;
            SpriteRenderer.color = Const.THIS.placeColors[(int)PlaceType.EMPTY];
        }
        public void Deconstruct()
        {
            if (Current)
            {
                Current.Deconstruct();
            }
        }

        public void SetColor(PlaceType placeType)
        {
            if (this._placeType.Equals(placeType))
            {
                return;
            }

            this._placeType = placeType;

            Color color = Const.THIS.placeColors[(int)placeType];
            if (placeType.Equals(PlaceType.EMPTY) && Merger)
            {
                color = Const.THIS.mergerColor;
            }

            DoColor(color);
        }

        private void DoColor(Color color)
        {
            SpriteRenderer.DOKill();
            SpriteRenderer.DOColor(color, 0.125f);
        }

        public void Accept(Pawn pawn, float duration, System.Action OnAccept = null)
        {
            pawn.transform.parent = segmentParent;
            pawn.Move(segmentParent.position, duration, Ease.Linear, () =>
            {
                this.Current = pawn;
                SetColor(PlaceType.EMPTY);
                OnAccept?.Invoke();
                pawn.CheckSteady(this, false);
            });
        }
        public void AcceptImmidiate(Pawn pawn)
        {
            pawn.Move(segmentParent, segmentParent.position);
            this.Current = pawn;
        }

        public enum PlaceType
        {
            FREE,
            OCCUPIED,
            EMPTY,
        }
    }
}