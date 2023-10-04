using DG.Tweening;
using Internal.Core;
using UnityEngine;

namespace Game
{
    public class Place : MonoBehaviour
    {
        [SerializeField] public Transform segmentParent;
        [SerializeField] public Transform tileTransform;
        [SerializeField] private Renderer gridTile;
        [System.NonSerialized] private Transform _thisTransform;
        [System.NonSerialized] public Vector2Int Index;
        [System.NonSerialized] private PlaceColorType _colorType = PlaceColorType.GREEN;
        [System.NonSerialized] private PlaceColorType _targetColorType = PlaceColorType.GREEN;
        [System.NonSerialized] private Tween _colorTween;
        [System.NonSerialized] public Pawn Current;
        public bool Occupied => Current;
        public Vector3 PlacePosition => gridTile.transform.position;
        [System.NonSerialized] private GameObject _ghostPawn = null;

        public Vector3 Position => _thisTransform.position;
        public PlaceColorType NormalDarkLight => Even ? PlaceColorType.NORMAL_DARK : PlaceColorType.NORMAL_LIGHT;
        public PlaceColorType LimitDarkLight => Even ? PlaceColorType.LIMIT_DARK : PlaceColorType.LIMIT_LIGHT;
        public PlaceColorType RayDarkLight => Even ? PlaceColorType.RAY_DARK : PlaceColorType.RAY_LIGHT;
        public bool Even => (Index.x + Index.y) % 2 == 0;
        
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
            SetTargetColorType(NormalDarkLight);
            FinalizeImmediate();
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
        public void SetTargetColorType(PlaceColorType placeColorType)
        {
            this._targetColorType = placeColorType;
        }
        public void FinalizeState()
        {
            if (_targetColorType.Equals(_colorType))
            {
                return;
            }
            
            _colorType = _targetColorType;
            
            int enumIndex = (int)_colorType;
            
            Color targetColor = Const.THIS.placeColorsDouble[enumIndex];
            DoColor(targetColor);
            
            Vector3 targetPos = Const.THIS.placePosDouble[enumIndex];
            DoPos(targetPos);
            
            bool targetState = Const.THIS.ghostPawnStateDouble[enumIndex];
            DoGhostPawn(targetState);
        }
        public void FinalizeImmediate()
        {
            if (_targetColorType.Equals(_colorType))
            {
                return;
            }
            _colorType = _targetColorType;
            Color targetColor = Const.THIS.placeColorsDouble[(int)_colorType];
            gridTile.material.SetColor(GameManager.BaseColor, targetColor);
            
            Vector3 targetPos = Const.THIS.placePosDouble[(int)_colorType];
            // tileTransform.DOKill();
            tileTransform.localPosition = targetPos;
        }

        private void DoColor(Color color)
        {
            gridTile.DOKill();
            _colorTween?.Kill();
            _colorTween = gridTile.material.DOColor(color, 0.15f).SetEase(Ease.OutQuad);
        }
        private void DoPos(Vector3 pos)
        {
            tileTransform.localPosition = pos;
            // tileTransform.DOKill();
            // tileTransform.DOLocalMove(pos, 0.15f);
        }
        private void DoGhostPawn(bool add)
        {
            if (add)
            {
                _ghostPawn = Board.THIS.AddGhostPawn(segmentParent.position);
            }
            else
            {
                if (!_ghostPawn)
                {
                    return;
                }
                Board.THIS.RemoveGhostPawn(this._ghostPawn);
                _ghostPawn = null;
            }
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
            NORMAL_DARK,
            NORMAL_LIGHT,
            LIMIT_DARK,
            LIMIT_LIGHT,
            RAY_DARK,
            RAY_LIGHT,
        }
    }
}