using DG.Tweening;
using Internal.Core;
using UnityEngine;

namespace Game
{
    public class Place : MonoBehaviour
    {
        [SerializeField] public Transform segmentParent;
        [SerializeField] private MeshRenderer placementSprite;
        // [SerializeField] private MeshRenderer igniteSprite;
        [System.NonSerialized] public Vector2Int index;
        [System.NonSerialized] private PlaceType _placeType = PlaceType.FREE;
        // [System.NonSerialized] private bool _supplier = false;
        public Pawn Current { get; set; }
        public bool Occupied => Current;
        public bool IsBorderPlace => index.y == Board.THIS.Size.y - 1;
        
        public int LinearIndex => index.x * Board.THIS.Size.y + index.y;


        // public bool Supplier
        // {
        //     get => _supplier;
        //     set
        //     {
        //         _supplier = value;
        //         igniteSprite.enabled = value;
        //         // SetPlaceType(PlaceType.EMPTY, true);
        //         this._placeType = PlaceType.EMPTY;
        //
        //         Color color = value ? Const.THIS.shooterPlaceColor : Const.THIS.placeColors[(int)PlaceType.EMPTY];
        //         placementSprite.SetColor(GameManager.MPB_PLACEMENT, GameManager.BaseColor, color);
        //     }
        // }

        public void Construct()
        {
            this._placeType = PlaceType.FREE;
            
            Color color = Const.THIS.placeColors[(int)PlaceType.EMPTY];
            placementSprite.SetColor(GameManager.MPB_PLACEMENT, GameManager.BaseColor, color);

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
                Current.DeconstructAnimated(true);
                Current = null;
            }
        }
        public void OnFail()
        {
            if (Occupied)
            {
                Current.DeconstructAnimated(false);
                Current = null;
            }
        }
        public void SetPlaceType(PlaceType placeType, bool force = false)
        {
            if (!force && this._placeType.Equals(placeType))
            {
                return;
            }

            this._placeType = placeType;

            Color color = Const.THIS.placeColors[(int)placeType];
            // if (placeType.Equals(PlaceType.EMPTY) && Supplier)
            // {
            //     color = Const.THIS.shooterPlaceColor;
            // }

            DoColor(color);
        }

        private void DoColor(Color color)
        {
            placementSprite.DOKill();
            placementSprite.GetPropertyBlock(GameManager.MPB_PLACEMENT, 0);
            Color startColor = GameManager.MPB_PLACEMENT.GetColor(GameManager.BaseColor);
            placementSprite.DoColor(GameManager.MPB_PLACEMENT, GameManager.BaseColor, startColor, color, 0.125f, Ease.OutSine);
        }

        public void Accept(Pawn pawn, float duration, System.Action OnComplete = null)
        {
            this.Current = pawn;

            pawn.transform.parent = segmentParent;
            pawn.Move(segmentParent.position, duration, AnimConst.THIS.moveEase, () =>
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