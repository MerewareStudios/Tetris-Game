using DG.Tweening;
using Internal.Core;
using UnityEngine;

namespace Game
{
    public class Place : MonoBehaviour
    {
        [SerializeField] public Transform segmentParent;
        [SerializeField] private Renderer gridTile;
        [System.NonSerialized] private Transform _thisTransform;
        [System.NonSerialized] public Vector2Int Index;
        [System.NonSerialized] private PlaceColor _placeColor = PlaceColor.GREEN;
        [System.NonSerialized] private Tween _colorTween;
        public Pawn Current { get; set; }
        public bool Occupied => Current;
        public bool IsBorderPlace => Index.y == Board.THIS.Size.y - 1;
        public Vector3 PlacePosition => gridTile.transform.position;
        
        public int LinearIndex => Index.x * Board.THIS.Size.y + Index.y;
        public Vector3 Position => _thisTransform.position;

        public Vector3 LocalPosition
        {
            set => _thisTransform.localPosition = value;
            get => _thisTransform.localPosition;
        }
        
        void Awake()
        {
            this._thisTransform = this.transform;
        }


        public void Construct()
        {
            this._placeColor = PlaceColor.GREEN;
            gridTile.SetColor(GameManager.MPB_GRID_TILE, GameManager.BaseColor, Const.THIS.gridTileColors[LinearIndex % 2]);
        }
        public void Deconstruct()
        {
            if (Occupied)
            {
                Current.Deconstruct();
                Current = null;
            }
        }
        
        public void OnLevelEnd()
        {
            if (Occupied)
            {
                Current.OnLevelEnd();
            }
        }
        // public void OnFail()
        // {
        //     if (Occupied)
        //     {
        //         Current.Deconstruct();
        //         Current = null;
        //     }
        // }
        public void SetPlaceType(PlaceColor placeColor)
        {
            if (this._placeColor.Equals(placeColor))
            {
                return;
            }

            this._placeColor = placeColor;
            DoColor(Const.THIS.placeColorsDouble[(int)placeColor * 2 + (LinearIndex % 2)]);
        }
        
        public void SetPlaceTypeImmediate(PlaceColor placeColor)
        {
            if (this._placeColor.Equals(placeColor))
            {
                return;
            }
            this._placeColor = placeColor;
            gridTile.SetColor(GameManager.MPB_GRID_TILE, GameManager.BaseColor, Const.THIS.placeColorsDouble[(int)placeColor * 2 + (LinearIndex % 2)]);
        }

        private void DoColor(Color color)
        {
            gridTile.DOKill();
            gridTile.GetPropertyBlock(GameManager.MPB_GRID_TILE, 0);
            Color startColor = GameManager.MPB_GRID_TILE.GetColor(GameManager.BaseColor);
            
            _colorTween?.Kill();
            _colorTween = gridTile.DoColor(GameManager.MPB_GRID_TILE, GameManager.BaseColor, startColor, color, 0.1f, Ease.OutSine);
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

        public enum PlaceColor
        {
            GREEN,
            RED,
            NORMAL,
        }
    }
}