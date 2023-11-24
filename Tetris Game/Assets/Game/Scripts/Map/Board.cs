using Internal.Core;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Random = UnityEngine.Random;

namespace Game
{
    public class Board : Singleton<Board>
    {
        [SerializeField] private Vector3 indexOffset;
        
        [SerializeField] public RectTransform visualFrame;
        [SerializeField] public Transform ground;
        [Header("Ground")]
        [SerializeField] public Transform playerPivot;
        [SerializeField] public RectTransform deadline;
        [Header("Pins")]
        [SerializeField] public RectTransform bottomPin;
        [SerializeField] public RectTransform statsPin;
        [SerializeField] public RectTransform spawnerPin;
        [SerializeField] public RectTransform enemySpawnPin;
        [SerializeField] private ParticleSystem suggestionHighlight;
        [System.NonSerialized] private ParticleSystem.MainModule _suggestionHighlightMain;
        [System.NonSerialized] private Transform _suggestionHighlightTransform;
        [Header("Rect")]
        [SerializeField] public RectTransform projectionRect;

        [System.NonSerialized] public System.Action OnMerge;
        [System.NonSerialized] public const float MagnetRadius = 2.5f;
        [System.NonSerialized] public const float BombRadius = 1.5f;
        
        [System.NonSerialized] private Transform _thisTransform;
        [System.NonSerialized] private Vector2Int _size;
        [System.NonSerialized] private Vector3 _thisPosition;
        [System.NonSerialized] private Place[,] _places;
        [System.NonSerialized] private int _tick = 0;
        [System.NonSerialized] private Tween _delayedHighlightTween = null;
        [System.NonSerialized] private Data _data;
        [SerializeField] public int[] DropPositions;
        [System.NonSerialized] public bool BoostingStack = false;
        [System.NonSerialized] public List<SubModel> LoseSubModels = new();

        public int StackLimit => _Data.maxStack + (BoostingStack ? 1 : 0);
        
        void Awake()
        {
            this._thisTransform = this.transform;
            this._suggestionHighlightTransform = this.suggestionHighlight.transform;
            this._suggestionHighlightMain = this.suggestionHighlight.main;
        }

        public Data _Data
        {
            set
            {
                _data = value;
            }
            get => _data;
        }

        #region Construct - Deconstruct
            public void Construct(Vector2Int size)
            {
                this._size = size;
                DropPositions = new int[size.x];
                ClearDropPositions();
                
                CameraManager.THIS.OrtoSize = Mathf.Max(7.63f, _size.x + 1.82f);

                if (_places != null)
                {
                    foreach (var place in _places)
                    {
                        place.Despawn(0);
                    }
                }
                
                _places = new Place[_size.x, _size.y];
                for (int i = 0; i < _size.x; i++)
                {
                    for(int j = 0; j < _size.y; j++)
                    {
                        Place place = Pool.Place.Spawn<Place>(_thisTransform);
                        place.LocalPosition = new Vector3(i, 0.0f, -j);
                        _places[i, j] = place;
                        place.Index = new Vector2Int(i, j);
                        place.Construct();
                    }
                }


                PawnPlacement[] pawnPlacements = LevelManager.PawnPlacements();

                if (pawnPlacements != null) 
                {
                    foreach (var pawnPlacement in pawnPlacements)
                    {
                        Place place = _places[pawnPlacement.index.x, pawnPlacement.index.y];
                        SpawnPawn(place, pawnPlacement.usage, pawnPlacement.extra == 0 ? pawnPlacement.usage.ExtraValue() : pawnPlacement.extra, false);
                    }
                }
                
                
                visualFrame.sizeDelta = new Vector2(_size.x * 100.0f + 42.7f, _size.y * 100.0f + 42.7f);
                _thisTransform.localPosition = new Vector3(-_size.x * 0.5f + 0.5f, 0.0f, _size.y * 0.5f + 1.75f);

                float groundScale = (25.0f + (_size.x - 6) * 2.5f);
                ground.localScale = Vector3.one * Mathf.Max(groundScale, 25.0f);

                
                this.enabled = true;
                projectionRect.ForceUpdateRectTransforms();
                projectionRect.gameObject.SetActive(false);
                projectionRect.gameObject.SetActive(true);
            }

            public void Deconstruct()
            {
                DehighlightImmediate();
                foreach (var place in _places)
                {
                    place.Deconstruct();
                }
                foreach (var subModel in LoseSubModels)
                {
                    subModel.DeconstructImmediate();
                }
                LoseSubModels.Clear();
            }

            public void OnVictory()
            {
                Call<Place>(_places, (place, x, y) =>
                {
                    if (place.Current)
                    {
                        place.Current.RewardForSubModel();
                    }
                });
            }

            private void LateUpdate()
            {
                Spawner.THIS.UpdatePosition(spawnerPin.position);

                    
                float offset = (bottomPin.position - Spawner.THIS.transform.position).z - 1.1f;
                    
                    
                _thisTransform.localPosition += new Vector3(0.0f, 0.0f, -offset);
                _thisPosition = _thisTransform.position;
                    

                Vector3 playerPos = playerPivot.position;
                playerPos.y = 0.0f;
                Warzone.THIS.Player.Position = playerPos;

                    
                Spawner.THIS.UpdateFingerDelta(bottomPin.position);


                Vector3 topProjection = Spawner.THIS.HitPoint(new Ray(enemySpawnPin.position, CameraManager.THIS.gameCamera.transform.forward));
                Warzone.THIS.StartLine = topProjection.z - 1.5f;
                Warzone.THIS.EndLine = deadline.position.z;
                Warzone.THIS.SpawnRange = topProjection.x;

                
                Vector2 localPoint = CameraManager.THIS.gameCamera.WorldToScreenPoint(statsPin.position);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(projectionRect, localPoint, CameraManager.THIS.gameCamera, out Vector2 local);
                StatDisplayArranger.THIS.SetLocalY(local.y);
                
                
                this.enabled = false;
            }

            #endregion
        #region Highlight - Pawn
            private void DehighlightImmediate()
            {
                foreach (var place in _places)
                {
                    place.SetTargetColorType(place.NormalDarkLight);
                    place.FinalizeImmediate();
                }
            }
            public void HighlightPlaces()
            {
                Block block = Spawner.THIS.CurrentBlock;
                bool grabbed = Spawner.THIS.GrabbedBlock;
                foreach (var place in _places)
                {
                    if (block && grabbed && !block.Free2Place)
                    {
                        place.SetTargetColorType((place.Index.y >= _size.y - block.blockData.FitHeight) ? place.LimitDarkLightDown : place.LimitDarkLightUp);
                    }
                    else
                    {
                        place.SetTargetColorType(place.NormalDarkLight);
                    }
                }
                
                
                if (block && grabbed)
                {
                    List<Vector2Int> projectedPlaces = new();
                    bool canProjectFuture = !block.Free2Place;
                    foreach (var pawn in block.Pawns)
                    {
                        (Place place, bool canPlace) = Project(pawn, block.RequiredPlaces);
                        if (!place)
                        {
                            continue;
                        }
                        
                        place.SetTargetColorType(canPlace ? Game.Place.PlaceColorType.GREEN : Game.Place.PlaceColorType.RED);

                        if (canProjectFuture)
                        {
                            if (canPlace && place)
                            {
                                projectedPlaces.Add(place.Index);
                            }
                            else
                            {
                                canProjectFuture = false;
                            }
                        }
                    }

                    if (canProjectFuture && projectedPlaces.Count == block.Pawns.Count)
                    {
                        int minShift = _size.y;
                        
                        for (int i = 0; i < projectedPlaces.Count; i++)
                        {
                            Vector2Int currentIndex = projectedPlaces[i];
                            int currentShift = 0;
                            for (int v = currentIndex.y; v >= 0; v--)
                            {
                                if (_places[currentIndex.x, v].Current)
                                {
                                    break;
                                }
                                
                                currentShift++;
                            }

                            currentShift--;
                            minShift = Mathf.Min(minShift, currentShift);
                        }

                        if (minShift > 0)
                        {
                            for (int i = 0; i < projectedPlaces.Count; i++)
                            {
                                Vector2Int shiftedIndex = projectedPlaces[i] - new Vector2Int(0, minShift);
                                Game.Place.PlaceColorType type = _places[shiftedIndex.x, shiftedIndex.y].TargetColorType.Equals(Game.Place.PlaceColorType.GREEN)
                                        ? Game.Place.PlaceColorType.GREEN
                                        : _places[shiftedIndex.x, shiftedIndex.y].RayDarkLight;
                                _places[shiftedIndex.x, shiftedIndex.y].SetTargetColorType(type);
                            }
                        }
                    }
                }
                
                foreach (var place in _places)
                {
                    place.FinalizeState();
                }
            }
            
            public GhostPawn AddGhostPawn(Vector3 position)
            {
                GhostPawn ghostPawn = Pool.Ghost_Pawn.Spawn<GhostPawn>();
                
                ghostPawn.thisTransform.position = position;
                
                ghostPawn.meshRenderer.material.DOKill();
                ghostPawn.meshRenderer.material.DOColor(Const.THIS.ghostNormal, 0.15f);
                
                return ghostPawn;
            }
            public void RemoveGhostPawn(GhostPawn ghostPawn)
            {
                ghostPawn.meshRenderer.material.DOKill();
                ghostPawn.meshRenderer.material.DOColor(Const.THIS.ghostFade, 0.15f).onComplete = () =>
                {
                    ghostPawn.Despawn(Pool.Ghost_Pawn);
                };
            }
        #endregion
        
        public void OnLevelEnd()
        {
            HideSuggestedPlaces();
            foreach (var place in _places)
            {
                place.OnLevelEnd();
            }
        }
        public void MoveAll(float moveDuration)
        {
            _tick++;

            foreach (var place in _places)
            {
                if (!place.Current)
                {
                    continue;
                }
                place.Current.MoveForward(place, _tick, moveDuration); // 2
            }
        }
        public void CheckAll()
        {
            foreach (var place in _places)
            {
                if (place.Current)
                {
                    place.Current.Check(place);
                }
            }
        }

        public List<Place> Index2Place(List<int> indexes)
        {
            List<Place> list = new();

            foreach (var index in indexes)
            {
                list.Add(LinearIndex2Place(index));
            }

            return list;
        }
        
        public void CheckDeadLock()
        {
            if (_delayedHighlightTween != null)
            {
                // Already running a suggestion loop, skip
                return;   
            }
            if (Map.THIS.MapWaitForCycle)
            {
                // Still waiting for a map cycle after placement, wait to end & skip
                return;
            }
            if (!Spawner.THIS.CurrentBlock)
            {
                // There is no block to suggest place or check deadlock, skip
                return;
            }
            if (Spawner.THIS.CurrentBlock.Free2Place)
            {
                if (ONBOARDING.PLACE_POWERUP.IsNotComplete())
                {
                    ONBOARDING.PLACE_POWERUP.SetComplete();
                    Onboarding.TalkAboutFreePlacement();
                }
                // There is no block to suggest place or check deadlock, skip
                return;
            }
            if (Spawner.THIS.CurrentBlock.RequiredPlaces != null && Spawner.THIS.CurrentBlock.RequiredPlaces.Count > 0)
            {
                Highlight(Spawner.THIS.CurrentBlock.RequiredPlaces, Const.THIS.suggestionColorTut);
                _delayedHighlightTween = DOVirtual.DelayedCall(1.5f, () =>
                {
                    Highlight(Spawner.THIS.CurrentBlock.RequiredPlaces, Const.THIS.suggestionColorTut);
                }, false).SetLoops(-1);
                // Running a suggestion loop via suggested location by level design, skip
                return;
            }
           
            for (int i = 0; i < _size.x; i++)
            {
                for (int j = 0; j < _size.y; j++)
                {
                    Place place = _places[i, j];
                    if (!place.Current)
                    {
                        continue;
                    }
                    if (place.Current.Mover)
                    {
                        // Pawns are still moving, skip
                        return;
                    }
                }
            }

            List<List<Place>> allPlaces = DetectFit(Spawner.THIS.CurrentBlock);

            if (allPlaces.Count > 0)
            {
                List<Place> randomPlaces = allPlaces.Random();
                _delayedHighlightTween = DOVirtual.DelayedCall(10.0f, () =>
                {
                    Highlight(randomPlaces, Const.THIS.suggestionColor);
                    _delayedHighlightTween?.Kill();
                    _delayedHighlightTween = null;
                }, false);
                // Found a fit, suggest/highlight it, skip
                return;
            }
            if (ONBOARDING.USE_POWERUP.IsNotComplete())
            {
                if (!UIManager.THIS.finger.Visible)
                {
                    Powerup.THIS.Enabled = true;
                    Onboarding.TalkAboutPowerUp();
                }
            }
            else
            {
                Powerup.THIS.PunchFrame(0.2f);
            }
        }

        public Place LinearIndex2Place(int index)
        {
            Vector2Int ind = index.ToIndex(_size.y);
            return _places[ind.x, ind.y];
        }
        private void Call<T>(T[,] array, System.Action<T, int, int> action)
        {
            for (int i = 0; i < _size.x; i++)
            {
                for (int j = 0; j < _size.y; j++)
                {
                    action.Invoke(array[i, j], i, j);
                }
            }
        }

        private Place GetPlace(Vector2Int index) => _places[index.x, index.y];

        
        public float UsePowerups()
        {
            for (int j = 0; j < _size.y; j++)
            {
                for (int i = 0; i < _size.x; i++)
                {
                    if (!_places[i, j].Occupied)
                    {
                        continue;
                    }

                    Place place = _places[i, j];
                    
                    if (!place.Current.Busy && place.Current.UsageType.Equals(Pawn.Usage.Magnet))
                    {
                        CreatePawnAtCircular(i, j);
                        return -1.0f;
                    }
                    if (place.Current.UsageType.Equals(Pawn.Usage.Bomb))
                    {
                        float rest = _places[i, j].Current.SubModel.OnTick();

                        if (rest <= 0.0f)
                        {
                             _places[i, j].Current.Explode(new Vector2Int(i, j));
                             return 0.35f;
                        }
                    }
                    if (!place.Current.Busy && !place.Current.Mover && place.Current.UsageType.Equals(Pawn.Usage.Gift))
                    {
                        place.Current.Unpack();
                        return 0.35f;
                    }
                }
            }

            return -1.0f;
        }

        #region Drop
            private void ClearDropPositions()
            {
                for (int i = 0; i < DropPositions.Length; i++)
                {
                    DropPositions[i] = _size.y;
                }
            }
            private void AddDropPosition(Vector2Int point)
            {
                int current = DropPositions[point.x];
                if (point.y < current)
                {
                    DropPositions[point.x] = point.y;
                }
            }
            public bool HasDrop()
            {
                for (int i = 0; i < DropPositions.Length; i++)
                {
                    if (DropPositions[i] < _size.y)
                    {
                        return true;
                    }
                }

                return false;
            }
            public void MarkDropPointsMover()
            {
                
                for (int i = 0; i < DropPositions.Length; i++)
                {
                    int y = DropPositions[i];
                    if (y >= _size.y)
                    {
                        continue;
                    }

                    for (int j = y + 1; j < _size.y; j++)
                    {
                        Place place = _places[i, j];
                        if (place.Current && !place.Current.Connected)
                        {
                            place.Current.Mover = true;
                            place.Current.JumpUp(0.2f, 0.3f, (j - y) * 0.075f + 0.25f);
                        }
                    }
                }

                ClearDropPositions();
            }
        #endregion
        
        public List<int> CheckTetris()
        {
            List<int> tetrisLines = new();
            for (int j = 0; j < _size.y; j++)
            {
                bool tetris = true;
                for (int i = 0; i < _size.x; i++)
                {
                    Place place = _places[i, j];
                    if (!place.Occupied)
                    {
                        tetris = false;
                        continue;
                    }
                    if (place.Current.Mover)
                    {
                        tetris = false;
                        continue;
                    }
                    if (place.Current.UsageType.Equals(Pawn.Usage.Magnet) || place.Current.UsageType.Equals(Pawn.Usage.Gift))
                    {
                        tetris = false;
                        continue;
                    }
                }
                if (tetris)
                {
                    tetrisLines.Add(j);
                }
            }
            return tetrisLines;
        }

        private Pawn SpawnPawn(Place place, Pawn.Usage usage, int amount, bool mover)
        {
            Pawn pawn = Spawner.THIS.SpawnPawn(null, place.Position, amount, usage);
            pawn.Mover = mover;
            place.Accept(pawn);
            return pawn;
        }

        public void MergeLines(List<int> lines)
        {
            void MergeLine(int lineIndex, int mergeIndex)
            {
                int highestTick = int.MinValue;
                int horIndex = 0;

                for (int i = 0; i < _size.x; i++)
                {
                    int index = i;
                    Place place = _places[index, lineIndex];

                    if (!place.Current)
                    {
                        continue;
                    }
                    if (place.Current.Tick > highestTick)
                    {
                        highestTick = place.Current.Tick;
                        horIndex = index;
                    }
                    else if(place.Current.Tick == highestTick)
                    {
                        if(Helper.IsPossible(0.5f))
                        {
                            horIndex = index;
                        }
                    }
                }
                CreatePawnAtHorizontal(horIndex, lineIndex, lines.Count, mergeIndex);
            }
            
            // OnMerge?.Invoke(lines.Count);
            OnMerge?.Invoke();

            for (int i = 0; i < lines.Count; i++)
            {
                MergeLine(lines[i], i);
            }
        }

        public void SpawnTrapBomb(int extra)
        {
            int startHeight = Mathf.Min(3, _size.y);

            List<Place> randomPlaces = new List<Place>();

            for (int y = startHeight; y >= 0; y--)
            {
                for (int x = 0; x < _size.x; x++)
                {
                    Vector2Int index = new Vector2Int(x, y);
                    Place place = _places[index.x, index.y];
                    
                    if (place.Occupied || ExpectedMoverComing(index))
                    {
                        continue;
                    }
                    
                    randomPlaces.Add(place);
                }
            }

            if (randomPlaces.Count == 0)
            {
                return;
            }

            Place randomPlace = randomPlaces.Random();
            Particle.Lightning.Play(randomPlace.PlacePosition - CameraManager.THIS.gameCamera.transform.forward);
            SpawnPawn(randomPlace, Pawn.Usage.Bomb, extra, false);
        }
        
        public void DestroyWithProjectile(ParticleSystem ps, Vector3 startPosition)
        {
            ps.Clear();
            Vector2Int pos = new Vector2Int(Random.Range(0, _size.x), Random.Range(0, _size.y));
            Vector3 targetPosition = _places[pos.x, pos.y].PlacePosition;
            targetPosition.y += 0.5f;
            
            ps.Play();
            Transform psTransform = ps.transform;
            psTransform.position = startPosition;
            psTransform.DOJump(targetPosition, 2.0f, 1, 1.0f).onComplete = () =>
            {
                ps.Stop();
                Particle.Missile_Explosion.Play(targetPosition);
                ExplodePawnsCircular(pos, Board.BombRadius);
                MarkDropPointsMover();
            };
        }

        private void CreatePawnAtHorizontal(int horizontal, int lineIndex, int multiplier, int mergeIndex)
        {
            Place spawnPlace = _places[horizontal, lineIndex];
            int totalAmmo = 0;
            Tween lastTween = null;
            
            for (int i = 0; i < _size.x; i++)
            {
                Place place = _places[i, lineIndex];
                Pawn pawn = place.Current;
                if (!pawn)
                {
                    continue;
                }

                
                bool canMerge = pawn.Unpack();

                if (!canMerge)
                {
                    continue;
                }
                
                totalAmmo += pawn.Amount;

                pawn.Available = false;
                
                pawn.PunchScaleModelPivot(AnimConst.THIS.mergedPunchScale, AnimConst.THIS.mergedPunchDuration);
                pawn.thisTransform.parent = null;
                pawn.thisTransform.DOKill();
                
                Tween tween = pawn.thisTransform.DOMove(spawnPlace.PawnTargetPosition, AnimConst.THIS.mergeTravelDur)
                    .SetEase(AnimConst.THIS.mergeTravelEase, AnimConst.THIS.mergeTravelShoot)
                    .SetDelay(AnimConst.THIS.mergeTravelDelay);
                
                tween.onComplete = pawn.Deconstruct;
                
                lastTween = tween;
                
                place.Current = null;
            }

            if (lastTween != null)
            {
                lastTween.onComplete += () =>
                {
                    CameraManager.THIS.Shake(0.2f + (0.1f * (multiplier - 1)), 0.5f);
                    Particle.Debris.Emit(30, spawnPlace.Position);
                    // Pool.Cube_Explosion.Spawn<CubeExplosion>().Explode(spawnPlace.Position + new Vector3(0.0f, 0.6f, 0.0f));
                };
            }

            totalAmmo = Mathf.Min(totalAmmo * multiplier,StackLimit);
            if (totalAmmo == 0)
            {
                return;
            }

            Pawn.Usage type = Pawn.Usage.Ammo;
            int ammo = totalAmmo;

            switch (mergeIndex)
            {
                case 0:
                    type = Pawn.Usage.Ammo;
                    ammo = totalAmmo;
                    break;
                case 1:
                    type = Pawn.Usage.Energy;
                    ammo = 0;
                    break;
                default:
                    type = Pawn.Usage.Gift;
                    ammo = 0;
                    break;
            }
            
            
            SpawnPawn(spawnPlace, type, ammo, true).MakeAvailable();
        }
        
        private void CreatePawnAtCircular(int horizontal, int vertical)
        {
            Vector2Int center = new Vector2Int(horizontal, vertical);
            
            Place spawnPlace = _places[horizontal, vertical];
            int totalAmmo = 0;
            Tween lastTween = null;
            
            for (int i = 0; i < _size.x; i++)
            {
                for (int j = 0; j < _size.y; j++)
                {
                    Place place = _places[i, j];
                    Pawn pawn = place.Current;
                    
                    Vector2Int current = new Vector2Int(i, j);
                    
                    if (Vector2Int.Distance(center, current) > MagnetRadius)
                    {
                        continue;
                    }
                    
                    AddDropPosition(current);
                    
                    if (!pawn)
                    {
                        continue;
                    }
                    

                    if (pawn.UsageType.Equals(Pawn.Usage.Magnet) && current != new Vector2Int(horizontal, vertical))
                    {
                        continue;
                    }
                    
                    totalAmmo += pawn.Amount;
                    
                    bool canMerge = pawn.Unpack();

                    if (!canMerge)
                    {
                        continue;
                    }
                    
                    pawn.Available = false;
                    
                    pawn.PunchScaleModelPivot(AnimConst.THIS.mergedPunchScale, AnimConst.THIS.mergedPunchDuration);
                    pawn.thisTransform.parent = null;
                    pawn.thisTransform.DOKill();
                    
                    Tween tween = pawn.thisTransform
                        .DOMove(spawnPlace.PawnTargetPosition, AnimConst.THIS.mergeTravelDur)
                        .SetEase(AnimConst.THIS.mergeTravelEase, AnimConst.THIS.mergeTravelShoot)
                        .SetDelay(AnimConst.THIS.mergeTravelDelay);
                    
                    tween.onComplete = pawn.Deconstruct;

                    lastTween = tween;

                    place.Current = null;
                }
            }
            
            if (lastTween != null)
            {
                lastTween.onComplete += () =>
                {
                    CameraManager.THIS.Shake(0.2f, 0.5f);

                    if (totalAmmo > 0)
                    {
                        Particle.Debris.Emit(30, spawnPlace.Position);
                        // Pool.Cube_Explosion.Spawn<CubeExplosion>().Explode(spawnPlace.Position + new Vector3(0.0f, 0.6f, 0.0f));
                    }
                };
            }

            totalAmmo = Mathf.Min(totalAmmo,StackLimit);
            if (totalAmmo == 0)
            {
                return;
            }
            SpawnPawn(spawnPlace, Pawn.Usage.Ammo, totalAmmo, true).MakeAvailable();
        }

        private Place GetSidePlace(int x, int y)
        {
            Place GetRightPlace()
            {
                return _places[x + 1, y];
            }
            Place GetLeftPlace()
            {
                return _places[x - 1, y];
            }
            
            if (x == 0)
            {
                return GetRightPlace();
            }

            if (x == _size.x - 1)
            {
                return GetLeftPlace();
            }

            return Random.Range(0.0f, 1.0f) < 0.5f ? GetLeftPlace() : GetRightPlace();
        }
        

        public void ExplodePawnsCircular(Vector2Int center, float radius)
        {
            for (int i = 0; i < _size.x; i++)
            {
                for (int j = 0; j < _size.y; j++)
                {
                    Place place = _places[i, j];
                    Pawn pawn = place.Current;
                    
                    Vector2Int current = new Vector2Int(i, j);
                    if (Vector2Int.Distance(center, current) > radius)
                    {
                        continue;
                    }
                    
                    AddDropPosition(current);
                    
                    if (!pawn)
                    {
                        continue;
                    }

                    pawn.Explode(place.Index);
                    RemovePawn(place);

                    Particle.Debris.Emit(30, place.Position);
                    // Pool.Cube_Explosion.Spawn<CubeExplosion>().Explode(place.Position + new Vector3(0.0f, 0.6f, 0.0f));
                }
            }
        }

        private void RemovePawn(Place place)
        {
            if (!place.Occupied)
            {
                return;
            }

            if (place.Current.ParentBlock)
            {
                place.Current.ParentBlock.DetachPawn(place.Current);
            }
            
            place.Current.Deconstruct();
            place.Current = null;
        }

        
        public void MarkMover(int horizontal)
        {
            Call<Place>(_places, (place, horizontalIndex, verticalIndex) =>
            {
                if (place.Current && !place.Current.Connected && verticalIndex > horizontal)
                {
                    place.Current.Mover = true;
                    place.Current.JumpUp(0.2f, 0.3f, (verticalIndex - horizontal) * 0.075f + 0.25f);
                }
            });
        }
       
        
        public void MarkMovers(int x, int y)
        {
            Call<Place>(_places, (place, horizontalIndex, verticalIndex) =>
            {
                if (place.Current && !place.Current.Connected && horizontalIndex == x && verticalIndex >= y)
                {
                    place.Current.Mover = true;
                    place.Current.JumpUp(0.2f, 0.3f, (verticalIndex - y - 1) * 0.075f);
                }
            });
        }

        public int TakeBullet(int splitCount)
        {
            int ammoGiven = 0;
            
            for (int j = 0; j < _size.y; j++)
            {
                for (int i = 0; i < _size.x; i++)
                {
                    Place place = _places[i, j];
                    
                    if (place.Current
                        && place.Current.UsageType.Equals(Pawn.Usage.Ammo)
                        && place.Current.Amount > 0
                        && !place.Current.Mover
                        && !place.Current.Busy
                        && place.Current.Available)
                    {
                        
                        place.Current.Amount -= 1;

                        
                        place.Current.OnUse();
                        if (place.Current.Amount <= 0)
                        {
                            place.Current.DetachSubModelAndDeconstruct();
                            place.Current = null;
                            MarkMovers(place.Index.x, place.Index.y);
                        }
                        
                
                        ammoGiven += splitCount;
                        return ammoGiven;
                    }
                }
            }
            
            return ammoGiven;
        }

        private bool ExpectedMoverComing(Vector2Int index)
        {
            if (index.y + 1 >= _size.y)
            {
                return false;
            }
            Place place = _places[index.x, index.y + 1];
            
            return place.Current && place.Current.Mover;
        }
        
        public void Place(Block block)
        {
            block.PlacedOnGrid = true;
            List<Pawn> temporary = new List<Pawn>(block.Pawns);
            foreach (Pawn pawn in temporary)
            {
                Pawn currentPawn = pawn;
                Place place = GetPlace(currentPawn);
                
                currentPawn.Mover = currentPawn.VData.moverOnPlacement;
                currentPawn.Busy = true;
                currentPawn.Tick = _tick;
                
                place.Accept(currentPawn, 0.1f, () =>
                {
                    currentPawn.Busy = false;
                    place.Current.Check(place);
                    Map.THIS.MapWaitForCycle = true;
                });
            }
        }
        private Place GetPlace(Pawn pawn)
        {
            Vector2Int? index = Project2Index(pawn);
            return index != null ? GetPlace(index.Value) : null;
        }
        private Vector2Int? Project2Index(Pawn pawn)
        {
            if (pawn.ParentBlock.IsPivotPawn(pawn))
            {
                pawn.ParentBlock.UnsafePivotIndex = Pos2UnsafeIndex(pawn.thisTransform.position);
            }
            return Unsafe2SafeIndex(pawn.ParentBlock.GetUnsafeIndex(pawn));
        }
        private Vector2Int? Pos2Index(Vector3 position)
        {
            Vector2Int index = Pos2UnsafeIndex(position);
            
            if (IsIndexValid(index))
            {
                return index;
            }
            return null;
        }
        
        private Vector2Int Pos2UnsafeIndex(Vector3 position)
        {
            Vector3 posDif = (position + Spawner.THIS.distanceOfBlockCast) - _thisPosition + indexOffset;
            return new Vector2Int((int)posDif.x, -(int)(posDif.z));
        }
        private Vector2Int? Unsafe2SafeIndex(Vector2Int index)
        {
            if (IsIndexValid(index))
            {
                return index;
            }
            return null;
        }
        private bool IsIndexValid(Vector2Int index) => index.x >= 0 && index.x < _size.x && index.y >= 0 && index.y < _size.y;
        
        private (Place, bool) Project(Pawn pawn, List<Place> requiredPlaces)
        {
            Vector2Int? index = Project2Index(pawn);
            if (index == null)
            {
                return (null, false);
            }
            Vector2Int indexValue = index.Value;
            Place place = GetPlace(indexValue);
            
            if (place.Occupied)
            {
                return (place, false);
            }
            if (ExpectedMoverComing(indexValue))
            {
                return (place, false);
            }
            if (pawn.VData.free2Place)
            {
                return (place, true);
            }
            
            if (_size.y - pawn.ParentBlock.blockData.FitHeight > indexValue.y)
            {
                return (place, false);
            }
            if (requiredPlaces is { Count: > 0 } && !requiredPlaces.Contains(place))
            {
                return (place, false);
            }
            
            return (place, true);
        }
        public Place Project(Vector3 position)
        {
            Vector2Int? index = Pos2Index(position);
            if (index == null)
            {
                return null;
            }
            Vector2Int ind = (Vector2Int)index;
            Place place = GetPlace(ind);
            
            if (place.Occupied)
            {
                return null;
            }
            return place;
        }
        public Place GetForwardPlace(Place place)
        {
            Place forwardPlace = null;

            Vector2Int index = place.Index;
            index.y--;
            if (index.y >= 0)
            {
                forwardPlace = GetPlace(index);
            }
            return forwardPlace;
        }
       
        private bool CanPlacePawnOnGrid(Pawn pawn, List<Place> requiredPlaces)
        {
            (Place place, bool canPlace) = Project(pawn, requiredPlaces);
            if (place == null)
            {
                return false;
            }

            return canPlace;
        }
        public bool CanPlace(Block block)
        {
            foreach (var pawn in block.Pawns)
            {
                if (!CanPlacePawnOnGrid(pawn, block.RequiredPlaces))
                {
                    return false;
                }
            }
            return true;
        }

        private void EmitSuggestionHighlight(int count, Color color, Vector3 position, Quaternion rotation)
        {
            _suggestionHighlightTransform.position = position;
            _suggestionHighlightTransform.localRotation = rotation;
            _suggestionHighlightMain.startColor = color;
            suggestionHighlight.Emit(count);
        }

        public void Highlight(List<Place> places, Color color)
        {
            foreach (var place in places)
            {
                EmitSuggestionHighlight(1, color, place.PlacePosition + new Vector3(0.0f, 0.01f, 0.0f), Quaternion.Euler(90.0f, 0.0f, 0.0f));
            }
        }
        public void HighlightEmptyPlaces(Color color)
        {
            foreach (var place in _places)
            {
                if (place.Current)
                {
                    continue;
                }

                EmitSuggestionHighlight(1, color, place.PlacePosition + new Vector3(0.0f, 0.01f, 0.0f), Quaternion.Euler(90.0f, 0.0f, 0.0f));
            }
        }

        public void HideSuggestedPlaces()
        {
            suggestionHighlight.Stop();
            suggestionHighlight.Clear();
            _delayedHighlightTween?.Kill();
            _delayedHighlightTween = null;
        }
        
        private List<List<Place>> DetectFit(Block block)
        {
            List<List<Place>> allPlaces = new();

            List<Vector3> localPawnPositions = block.LocalPawnPositions;
            
            Vector3 zeroShift = Vector3.zero;


            foreach (var angle in block.blockData.checkAngles)
            {
                int totalHorShiftStart = 0;
                int totalHorShiftEnd = 0;
                
                int totalVertShiftStart = 0;
                int totalVertShiftEnd = 0;

                switch (angle)
                {
                    case 0:
                        zeroShift = new Vector3(1.0f, 0.0f, 1.5f);
                        totalHorShiftStart = 0;
                        totalHorShiftEnd = _size.x - block.blockData.NormalWidth + 1;
                        totalVertShiftStart = block.blockData.NormalHeight - 1 + (_size.y - block.blockData.FitHeight);
                        totalVertShiftEnd = _size.y;
                        break;
                    case 90:
                        zeroShift = new Vector3(1.5f, 0.0f, -1.0f);
                        totalHorShiftStart = 0;
                        totalHorShiftEnd = _size.x - block.blockData.NormalHeight + 1;
                        totalVertShiftStart = _size.y - block.blockData.FitHeight;
                        totalVertShiftEnd = _size.y - block.blockData.NormalWidth + 1;
                        break;
                    case 180:
                        zeroShift = new Vector3(-1.0f, 0.0f, -1.5f);
                        totalHorShiftStart = block.blockData.NormalWidth - 1;
                        totalHorShiftEnd = _size.x;
                        totalVertShiftStart = _size.y - block.blockData.FitHeight;
                        totalVertShiftEnd = _size.y - block.blockData.NormalHeight + 1;
                        break;
                    case 270:
                        zeroShift = new Vector3(-1.5f, 0.0f, 1.0f);
                        totalHorShiftStart = block.blockData.NormalHeight - 1;
                        totalHorShiftEnd = _size.x;
                        totalVertShiftStart = block.blockData.NormalWidth - 1 + _size.y - block.blockData.FitHeight;
                        totalVertShiftEnd = _size.y;
                        break;
                }

                for (int j = totalVertShiftStart; j < totalVertShiftEnd; j++)
                {
                    for (int i = totalHorShiftStart; i < totalHorShiftEnd; i++)
                    {
                        bool found = true;

                        List<Place> foundPlaces = new();
                        
                        foreach (var localPawnPosition in localPawnPositions)
                        {
                            Vector3 rotatedPosition = localPawnPosition.RotatePointAroundPivot(Vector3.zero, Quaternion.Euler(0.0f, angle, 0.0f));

                            Vector3 shift = zeroShift + new Vector3(i, 0.0f, -j);

                            Vector3 finalPos = _thisPosition + shift + rotatedPosition;

                            Place place = Project(finalPos);
                            
                            // Helper.Sphere(finalPos + Vector3.up, 0.5f, Color.red, 0.5f);

                            if (!place)
                            {
                                found = false;
                                break;
                            }
                            // Helper.Sphere(place.Position + Vector3.up * 1.5f, 0.15f, Color.blue, 0.5f);

                            foundPlaces.Add(place);
                        }

                        if (found)
                        {
                            allPlaces.Add(foundPlaces);
                        }
                    }
                }
            }
            return allPlaces;
        }
        
        [System.Serializable]
        public class SuggestedBlock
        {
            [SerializeField] public Pool type;
            [SerializeField] public List<int> requiredPlaces;
            [SerializeField] public BlockRot blockRot;
            [SerializeField] public bool canRotate = true;
        }
        [System.Serializable]
        public class PawnPlacement
        {
            [SerializeField] public Pawn.Usage usage;
            [SerializeField] public Vector2Int index;
            [SerializeField] public int extra = 0;
        }
        public enum BlockRot
        {
            UP,
            RIGHT,
            DOWN,
            LEFT
        }
        
        [System.Serializable]
        public class Data : System.ICloneable
        {
            [SerializeField] public int defaultStack = 6;
            [SerializeField] public int maxStack = 6;
            [SerializeField] public int defaultSupplyLine = 6;
            
            public Data()
            {
                
            }
            public Data(Data data)
            {
                this.defaultStack = data.defaultStack;
                this.maxStack = data.maxStack;
                this.defaultSupplyLine = data.defaultSupplyLine;
            }

            public object Clone()
            {
                return new Data(this);
            }
        }
    }
}