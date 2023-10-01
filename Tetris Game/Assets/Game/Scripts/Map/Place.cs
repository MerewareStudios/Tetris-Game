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
        [System.NonSerialized] private PlaceColorType _placeColorType = PlaceColorType.GREEN;
        [System.NonSerialized] private Tween _colorTween;
        [System.NonSerialized] public Pawn Current;
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
            this._placeColorType = PlaceColorType.GREEN;
            gridTile.material.SetColor(GameManager.BaseColor, Const.THIS.gridTileColors[LinearIndex % 2]);
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
        public void SetPlaceType(PlaceColorType placeColorType, Block block = null)
        {
            if (this._placeColorType.Equals(placeColorType))
            {
                return;
            }

            this._placeColorType = placeColorType;
            Color targetColor = Const.THIS.placeColorsDouble[(int)placeColorType * 2 + (LinearIndex % 2)];
            if (Spawner.THIS.FitColorPass &&block && placeColorType.Equals(PlaceColorType.NORMAL_LIMIT))
            {
                if (Index.y >= Board.THIS.Size.y - block.blockData.FitHeight)
                {
                    targetColor = targetColor.AddHueAddValue(0.00125f, 0.08f);
                }
            }
            DoColor(targetColor);
        }
        
        public void SetPlaceTypeImmediate(PlaceColorType placeColorType)
        {
            if (this._placeColorType.Equals(placeColorType))
            {
                return;
            }
            this._placeColorType = placeColorType;
            gridTile.material.SetColor(GameManager.BaseColor, Const.THIS.placeColorsDouble[(int)placeColorType * 2 + (LinearIndex % 2)]);
        }

        private void DoColor(Color color)
        {
            gridTile.DOKill();
            _colorTween?.Kill();
            _colorTween = gridTile.material.DOColor(color, 0.1f).SetEase(Ease.OutSine);
        }

        public void Accept(Pawn pawn, float duration, System.Action onComplete = null)
        {
            this.Current = pawn;

            pawn.transform.parent = segmentParent;
            pawn.Move(segmentParent.position, duration, AnimConst.THIS.moveEase, () =>
            {
                onComplete?.Invoke();
            });
        }
        public void AcceptNow(Pawn pawn)
        {
            this.Current = pawn;
            pawn.Set(segmentParent, segmentParent.position);
        }

        public enum PlaceColorType
        {
            GREEN,
            RED,
            NORMAL,
            NORMAL_LIMIT,
        }
    }
}