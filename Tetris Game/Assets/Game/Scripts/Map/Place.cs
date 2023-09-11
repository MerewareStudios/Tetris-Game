using DG.Tweening;
using Internal.Core;
using UnityEngine;

namespace Game
{
    public class Place : MonoBehaviour
    {
        [SerializeField] public Transform segmentParent;
        [SerializeField] private Renderer gridTile;
        [System.NonSerialized] public Vector2Int Index;
        [System.NonSerialized] private PlaceColor _placeColor = PlaceColor.GREEN;
        public Pawn Current { get; set; }
        public bool Occupied => Current;
        public bool IsBorderPlace => Index.y == Board.THIS.Size.y - 1;
        public Vector3 PlacePosition => gridTile.transform.position;
        
        public int LinearIndex => Index.x * Board.THIS.Size.y + Index.y;

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
        public void SetPlaceType(PlaceColor placeColor, bool force = false)
        {
            if (!force && this._placeColor.Equals(placeColor))
            {
                return;
            }

            this._placeColor = placeColor;
            DoColor(Const.THIS.placeColorsDouble[(int)placeColor * 2 + (LinearIndex % 2)]);
        }

        private void DoColor(Color color)
        {
            gridTile.DOKill();
            gridTile.GetPropertyBlock(GameManager.MPB_GRID_TILE, 0);
            Color startColor = GameManager.MPB_GRID_TILE.GetColor(GameManager.BaseColor);
            gridTile.DoColor(GameManager.MPB_GRID_TILE, GameManager.BaseColor, startColor, color, 0.125f, Ease.OutSine);
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