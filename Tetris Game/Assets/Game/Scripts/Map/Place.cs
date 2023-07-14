using DG.Tweening;
using Game;
using Internal.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game
{
    public class Place : MonoBehaviour
    {
        [SerializeField] public Transform segmentParent;
        [SerializeField] private SpriteRenderer placementSprite;
        [SerializeField] private SpriteRenderer igniteSprite;
        [System.NonSerialized] public Vector2Int index;
        [System.NonSerialized] private PlaceType _placeType = PlaceType.FREE;
        [System.NonSerialized] private bool _supplier = false;
        public Pawn Current { get; set; }
        public bool Occupied => Current;
        public bool IsBorderPlace => index.y == Board.THIS.Size.y - 1;


        public bool Supplier
        {
            get => _supplier;
            set
            {
                _supplier = value;
                igniteSprite.enabled = value;
                SetColor(PlaceType.EMPTY, true);
            }
        }

        public void Construct()
        {
            this._placeType = PlaceType.FREE;
            placementSprite.color = Const.THIS.placeColors[(int)PlaceType.EMPTY];
        }
        public void Deconstruct()
        {
            if (Occupied)
            {
                Current.Deconstruct();
                Current = null;
            }
        }
        
        public void OnVictory()
        {
            if (Occupied)
            {
                Current.OnVictory();
                Current = null;
            }
        }

        public void SetColor(PlaceType placeType, bool force = false)
        {
            if (!force && this._placeType.Equals(placeType))
            {
                return;
            }

            this._placeType = placeType;

            Color color = Const.THIS.placeColors[(int)placeType];
            if (placeType.Equals(PlaceType.EMPTY) && Supplier)
            {
                color = Const.THIS.mergerPlaceColor;
            }

            DoColor(color);
        }

        private void DoColor(Color color)
        {
            placementSprite.DOKill();
            placementSprite.DOColor(color, 0.125f);
        }

        public void Accept(Pawn pawn, float duration, System.Action OnComplete = null)
        {
            this.Current = pawn;

            pawn.transform.parent = segmentParent;
            pawn.Move(segmentParent.position, duration, Ease.Linear, () =>
            {
                OnComplete?.Invoke();
            });
        }
        public void AcceptNow(Pawn pawn)
        {
            this.Current = pawn;
            pawn.Set(segmentParent, segmentParent.position);
        }

        public enum PlaceType
        {
            FREE,
            OCCUPIED,
            EMPTY,
        }
    }
}