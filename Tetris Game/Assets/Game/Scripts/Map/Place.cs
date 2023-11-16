using DG.Tweening;
using UnityEngine;

namespace Game
{
    public class Place : MonoBehaviour
    {
        [SerializeField] private Renderer gridTile;
        [SerializeField] private Transform segmentParent;
        [SerializeField] public Transform tileTransform;
        
        [System.NonSerialized] private Transform _thisTransform;
        [System.NonSerialized] private PlaceColorType _colorType = PlaceColorType.GREEN;
        [System.NonSerialized] private Tween _colorTween;
        [System.NonSerialized] private GhostPawn _ghostPawn = null;
        
        [System.NonSerialized] public Vector2Int Index;
        [System.NonSerialized] public PlaceColorType TargetColorType = PlaceColorType.GREEN;
        [System.NonSerialized] private Pawn _current;
        
        public Vector3 PlacePosition => gridTile.transform.position;
        public Vector3 Position => _thisTransform.position;
        
        public PlaceColorType NormalDarkLight => Even ? PlaceColorType.NORMAL_DARK : PlaceColorType.NORMAL_LIGHT;
        public PlaceColorType LimitDarkLightUp => Even ? PlaceColorType.LIMIT_DARK_UP : PlaceColorType.LIMIT_LIGHT_UP;
        public PlaceColorType LimitDarkLightDown => Even ? PlaceColorType.LIMIT_DARK_DOWN : PlaceColorType.LIMIT_LIGHT_DOWN;
        public PlaceColorType RayDarkLight => Even ? PlaceColorType.RAY_DARK : PlaceColorType.RAY_LIGHT;
        
        public bool Occupied => Current;
        public bool Fast => _colorType.Equals(PlaceColorType.RED) || _colorType.Equals(PlaceColorType.GREEN);
        public bool Even => (Index.x + Index.y) % 2 == 0;
        
        public Vector3 LocalPosition
        {
            set => _thisTransform.localPosition = value;
            get => _thisTransform.localPosition;
        }

        public Pawn Current
        {
            get => this._current;
            set
            {
#if UNITY_EDITOR
                if (value && this._current)
                {
                    Debug.LogError("Current is not null"); // 6
                }
#endif
                this._current = value;
                if (!value)
                {
                    return;
                }
                this._current.thisTransform.parent = segmentParent;
                
#if UNITY_EDITOR
                if (this.segmentParent.childCount > 1)
                {
                    Debug.LogError("Too many child", this.gameObject);
                    Debug.Break();
                }
#endif
            }
        }
        
        public Vector3 PawnTargetPosition => segmentParent.position;
        
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

            if (_ghostPawn)
            {
                _ghostPawn.Despawn(Pool.Ghost_Pawn);
                _ghostPawn = null;
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
            this.TargetColorType = placeColorType;
        }
        public void FinalizeState()
        {
            if (TargetColorType.Equals(_colorType))
            {
                return;
            }
            
            _colorType = TargetColorType;
            
            int enumIndex = (int)_colorType;

            bool targetState = Const.THIS.ghostPawnStateDouble[enumIndex];
            DoGhostPawn(targetState);
            
            Color targetColor = Const.THIS.placeColorsDouble[enumIndex];
            DoColor(targetColor);
                
            Vector3 targetPos = Const.THIS.placePosDouble[enumIndex];
            tileTransform.localPosition = targetPos;
        }
        public void FinalizeImmediate()
        {
            if (TargetColorType.Equals(_colorType))
            {
                return;
            }
            _colorType = TargetColorType;
            Color targetColor = Const.THIS.placeColorsDouble[(int)_colorType];
            gridTile.material.SetColor(GameManager.BaseColor, targetColor);
            
            Vector3 targetPos = Const.THIS.placePosDouble[(int)_colorType];
            // tileTransform.DOKill();
            tileTransform.localPosition = targetPos;
        }

        private void DoColor(Color color)
        {
            _colorTween?.Kill();
            _colorTween = gridTile.material.DOColor(color, Fast ? 0.1f : 0.2f).SetEase(Ease.OutQuad);
        }

        private void DoGhostPawn(bool add)
        {
            if (add)
            {
                if (_ghostPawn)
                {
                    return;
                }
                _ghostPawn = Board.THIS.AddGhostPawn(PawnTargetPosition);
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

        public void Accept(Pawn pawn, float duration = 0.0f, System.Action onComplete = null)
        {
            Current = pawn; // 4

            if (duration <= 0.0f)
            {
                pawn.thisTransform.position = segmentParent.position;
                onComplete?.Invoke();
                return;
            }

            pawn.Move(PawnTargetPosition, duration, AnimConst.THIS.moveEase, () =>
            {
                onComplete?.Invoke();
            });
        }

        public enum PlaceColorType
        {
            GREEN,
            RED,
            NORMAL_DARK,
            NORMAL_LIGHT,
            LIMIT_DARK_UP,
            LIMIT_LIGHT_UP,
            LIMIT_DARK_DOWN,
            LIMIT_LIGHT_DOWN,
            RAY_DARK,
            RAY_LIGHT,
        }
    }
}