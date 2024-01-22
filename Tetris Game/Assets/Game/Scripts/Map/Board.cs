using System;
using Internal.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using Lofelt.NiceVibrations;
using Random = UnityEngine.Random;

namespace Game
{
    public class Board : Lazyingleton<Board>
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

        public delegate bool OnMergeInfo();
        [System.NonSerialized] public OnMergeInfo OnMerge;
        [System.NonSerialized] public const float MagnetRadius = 2.5f;
        [System.NonSerialized] public const float BombRadius = 1.5f;
        
        [System.NonSerialized] private Transform _thisTransform;
        [System.NonSerialized] private Vector2Int _size;
        [System.NonSerialized] private Vector3 _thisPosition;
        [System.NonSerialized] private Place[,] _places;
        // [System.NonSerialized] private int _tick = 0;
        [System.NonSerialized] private Tween _delayedHighlightTween = null;
        [System.NonSerialized] private Data _data;
        [SerializeField] public int[] DropPositions;
        [System.NonSerialized] public List<SubModel> LoseSubModels = new();

        public int StackLimit
        {
            get => SavedData.unlimitedStack ? int.MaxValue : SavedData.maxStack;
            set
            {
                _data.maxStack = value;
                UpdateStackStat();
            }
        }

        public void RemoveStackLimit()
        {
            SavedData.unlimitedStack = true;
            UpdateStackStat();
            
        }

        public Vector3 Index2Position(Vector2Int index)
        {
            return _thisPosition + new Vector3(1.0f + index.x, 0.0f, 0.5f + index.y);
        }

        public void RemoveMergeOnboardingCallback()
        {
            Board.THIS.OnMerge -= GameManager.THIS.CheckMergeOnboarding;
        }

        void Awake()
        {
            this._thisTransform = this.transform;
            this._suggestionHighlightTransform = this.suggestionHighlight.transform;
            this._suggestionHighlightMain = this.suggestionHighlight.main;
        }

        void Start()
        {
            if (ONBOARDING.WEAPON_TAB.IsNotComplete())
            {
                Board.THIS.OnMerge += GameManager.THIS.CheckMergeOnboarding;
            }
        }

        public Data SavedData
        {
            set
            {
                _data = value;
                UpdateStackStat();
            }
            get => _data;
        }

        private void UpdateStackStat()
        {
            if (SavedData.unlimitedStack || StackLimit <= SavedData.defaultStack)
            {
                StatDisplayArranger.THIS.Hide(StatDisplay.Type.MaxStack);
                return;
            }
            StatDisplayArranger.THIS.Show(StatDisplay.Type.MaxStack, StackLimit, punch:true);
        }

        #region Construct - Deconstruct
            public void Construct(Vector2Int size)
            {
                this._size = size;
                DropPositions = new int[size.x];
                ClearDropPositions();
                
                CameraManager.THIS.OrthoSize = Mathf.Max(7.8f, _size.x + 2.0f);

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
                        SpawnPawn(place, pawnPlacement.usage, pawnPlacement.extra == 0 ? pawnPlacement.usage.ExtraValue() : pawnPlacement.extra);
                    }
                }
                
                
                visualFrame.sizeDelta = new Vector2(_size.x * 100.0f + 42.7f, _size.y * 100.0f + 42.7f);
                _thisTransform.localPosition = new Vector3(-_size.x * 0.5f + 0.5f, 0.0f, _size.y * 0.5f + 1.75f);

                ground.localScale = Vector3.one * CameraManager.THIS.OrthoSize * 3.2f;

                
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

                    
                // Spawner.THIS.UpdateFingerDelta(bottomPin.position);


                Vector3 topProjection = Spawner.THIS.HitPoint(new Ray(enemySpawnPin.position, CameraManager.THIS.gameCamera.transform.forward));
                Warzone.THIS.StartLine = topProjection.z - 1.5f;
                Warzone.THIS.EndLine = deadline.position.z;
                Warzone.THIS.SpawnRange = topProjection.x;

                
                Vector2 localPoint = CameraManager.THIS.gameCamera.WorldToScreenPoint(statsPin.position);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(projectionRect, localPoint, CameraManager.THIS.gameCamera, out Vector2 local);
                StatDisplayArranger.THIS.SetLocalY(local.y);
                
                
                Warzone.THIS.airplane.UpdatePositions();
                
                this.enabled = false;
                
                LevelManager.THIS.OnLateLoad();
            }

            #endregion
        #region Highlight - Pawn
            public void DehighlightImmediate()
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
                        // place.SetTargetColorType((place.Index.y >= _size.y - _size.y) ? place.LimitDarkLightDown : place.LimitDarkLightUp);
                        place.SetTargetColorType(place.LimitDarkLightDown);
                    }
                    else
                    {
                        place.SetTargetColorType(place.NormalDarkLight);
                    }
                }
                
                
                if (block && grabbed)
                {
                    List<(Place, bool)> projectedPlaces = new();
                    // bool canProjectFuture = !block.Free2Place;

                    bool canPlaceAll = false;
                    
                    foreach (var pawn in block.Pawns)
                    {
                        (Place place, bool canPlace) = Project(pawn, block.RequiredPlaces);
                        if (!place)
                        {
                            projectedPlaces.Clear();
                            break;
                        }
                        projectedPlaces.Add((place, canPlace));
                    }
                    
                    if (projectedPlaces.Count == block.Pawns.Count)
                    {
                        canPlaceAll = true;
                        foreach (var projectedPlace in projectedPlaces)
                        {
                            projectedPlace.Item1.SetTargetColorType(projectedPlace.Item2 ? Game.Place.PlaceColorType.GREEN : Game.Place.PlaceColorType.RED);

                            if (canPlaceAll)
                            {
                                canPlaceAll = projectedPlace.Item2;
                            }
                        }
                    }

                    if (canPlaceAll)
                    {
                        List<int> lines = new();
                        foreach (var place in projectedPlaces)
                        {
                            int index = place.Item1.Index.y;
                            if (!lines.Contains(index))
                            {
                                lines.Add(index);
                            }
                        }

                        foreach (var verticalLine in lines)
                        {
                            bool allLineFilled = true;
                            for(int horizontal = 0; horizontal < _size.x; horizontal++)
                            {
                                Place place = _places[horizontal, verticalLine];

                                if (place.Occupied || place.TargetColorType.Equals(Game.Place.PlaceColorType.GREEN))
                                {
                                    continue;
                                }
                                allLineFilled = false;
                                break;
                            }


                            if (!allLineFilled)
                            {
                                continue;
                            }
                            
                            for(int horizontal = 0; horizontal < _size.x; horizontal++)
                            {
                                Place place = _places[horizontal, verticalLine];

                                place.SetTargetColorType(Game.Place.PlaceColorType.RAY_LIGHT);
                            }
                        }
                    }

                    // if (canProjectFuture)
                    // {
                    //     canProjectFuture = canPlaceAll;
                    // }

                    // canProjectFuture = false;

                    // if (canProjectFuture && projectedPlaces.Count == block.Pawns.Count)
                    // {
                    //     int minShift = _size.y;
                    //     
                    //     for (int i = 0; i < projectedPlaces.Count; i++)
                    //     {
                    //         Vector2Int currentIndex = projectedPlaces[i].Item1.Index;
                    //         int currentShift = 0;
                    //         for (int v = currentIndex.y; v >= 0; v--)
                    //         {
                    //             if (_places[currentIndex.x, v].Current)
                    //             {
                    //                 break;
                    //             }
                    //             
                    //             currentShift++;
                    //         }
                    //
                    //         currentShift--;
                    //         minShift = Mathf.Min(minShift, currentShift);
                    //     }
                    //
                    //     if (minShift > 0)
                    //     {
                    //         for (int i = 0; i < projectedPlaces.Count; i++)
                    //         {
                    //             Vector2Int shiftedIndex = projectedPlaces[i].Item1.Index - new Vector2Int(0, minShift);
                    //             Game.Place.PlaceColorType type = _places[shiftedIndex.x, shiftedIndex.y].TargetColorType.Equals(Game.Place.PlaceColorType.GREEN)
                    //                     ? Game.Place.PlaceColorType.GREEN
                    //                     : _places[shiftedIndex.x, shiftedIndex.y].RayDarkLight;
                    //             _places[shiftedIndex.x, shiftedIndex.y].SetTargetColorType(type);
                    //         }
                    //     }
                    // }
                }
                
                foreach (var place in _places)
                {
                    place.FinalizeState();
                }
            }

            
            
            // public GhostPawn AddGhostPawn(Vector3 position)
            // {
            //     GhostPawn ghostPawn = Pool.Ghost_Pawn.Spawn<GhostPawn>();
            //     
            //     ghostPawn.thisTransform.position = position;
            //     
            //     ghostPawn.meshRenderer.material.DOKill();
            //     ghostPawn.meshRenderer.material.DOColor(Const.THIS.ghostNormal, 0.15f).SetUpdate(true);
            //     
            //     return ghostPawn;
            // }
            // public void RemoveGhostPawn(GhostPawn ghostPawn)
            // {
            //     ghostPawn.meshRenderer.material.DOKill();
            //     ghostPawn.meshRenderer.material.DOColor(Const.THIS.ghostFade, 0.15f).SetUpdate(true).onComplete = () =>
            //     {
            //         ghostPawn.Despawn(Pool.Ghost_Pawn);
            //     };
            // }
        #endregion
        
        public void OnLevelEnd()
        {
            HideSuggestedPlaces();
            foreach (var place in _places)
            {
                place.OnLevelEnd();
            }
        }
        // public void MoveAll(float moveDuration)
        // {
        //     return;
        //     _tick++;
        //
        //     bool moved = false;
        //     foreach (var place in _places)
        //     {
        //         if (!place.Current)
        //         {
        //             continue;
        //         }
        //         // place.Current.MoveForward(place, _tick, moveDuration);
        //         bool movedThis = place.Current.MoveForward(place, _tick, moveDuration);
        //         if (!moved && movedThis)
        //         {
        //             moved = true;
        //         }
        //     }
        //
        //     if (!moved)
        //     {
        //         // Audio.Block_Drag.PlayOneShot();
        //         Map.ResetMergeAudioIndex();
        //     }
        // }
        // public void CheckAll()
        // {
        //     foreach (var place in _places)
        //     {
        //         if (place.Current)
        //         {
        //             place.Current.Check(place);
        //         }
        //     }
        // }

        public List<Place> Index2Place(List<int> indexes)
        {
            List<Place> list = new();

            foreach (var index in indexes)
            {
                list.Add(LinearIndex2Place(index));
            }

            return list;
        }

        private void HighlightRequired()
        {
            // Highlight(Spawner.THIS.CurrentBlock.RequiredPlaces, Const.THIS.suggestionColorTut);
            Highlight(Spawner.THIS.CurrentBlock.RequiredPlaces, Spawner.THIS.CurrentBlock.blockData.Color);
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
                HighlightRequired();
                _delayedHighlightTween = DOVirtual.DelayedCall(1.5f, HighlightRequired, false).SetLoops(-1);
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
                    // if (place.Current.Mover)
                    // {
                    //     // Pawns are still moving, skip
                    //     return;
                    // }
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
                HapticManager.Vibrate(HapticPatterns.PresetType.Selection);
                // Audio.No_Place.PlayOneShot();
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
                    if (!place.Current.Busy && place.Current.UsageType.Equals(Pawn.Usage.Gift))
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
            // public void MarkDropPointsMover()
            // {
            //     
            //     for (int i = 0; i < DropPositions.Length; i++)
            //     {
            //         int y = DropPositions[i];
            //         if (y >= _size.y)
            //         {
            //             continue;
            //         }
            //
            //         for (int j = y + 1; j < _size.y; j++)
            //         {
            //             Place place = _places[i, j];
            //             if (place.Current && !place.Current.Connected)
            //             {
            //                 place.Current.Mover = true;
            //             }
            //         }
            //     }
            //
            //     ClearDropPositions();
            // }
        #endregion
        
        public int CheckTetris(List<Place> projectedPlaces)
        {
            // Get places where the block is placed
            // foreach (var place in places)
            // {
            //     Debug.LogError("Place Index " + place.Index);
            // }
            
            // Debug.LogError("HORIZONTAL CHECK\n");
            // ********** Horizontal Merge Check
            
            // Get unique vertical (, y) indexes to check so we can check merge horizontally
            List<int> uniqueRows = ExtractUniqueRows(projectedPlaces);

            // foreach (var row in uniqueRows)
            // {
            //     Debug.LogError("Unique Check At Row : " + row);
            // }
            
            // Get the rows where we can merge
            List<int> mergeableRows = GetMergeableRows(uniqueRows);

            // if (mergeableRows.Count == 0)
            // {
            //     Debug.LogError("No Horizontal Merge is Found");
            // }
            // foreach (var mergeableRow in mergeableRows)
            // {
            //     Debug.LogError("Mergeable Horizontal Line At Row : " + mergeableRow);
            // }
            
            // Debug.LogError("VERTICAL CHECK\n");
            // ********** Vertical Merge Check
            
            // Get unique horizontal (x, ) indexes to check so we can check merge vertically
            List<int> uniqueColumns = ExtractUniqueColumns(projectedPlaces);

            // foreach (var column in uniqueColumns)
            // {
            //     Debug.LogError("Unique Check At Column : " + column);
            // }

            List<int> mergeableColumns = GetMergeableColumns(uniqueColumns);
            
            // if (mergeableColumns.Count == 0)
            // {
            //     Debug.LogError("No Vertical Merge is Found");
            // }
            // foreach (var mergeableColumn in mergeableColumns)
            // {
            //     Debug.LogError("Mergeable Vertical Line At Column : " + mergeableColumn);
            // }

            int multiplier = mergeableRows.Count + mergeableColumns.Count;

            // bool canMergeHorizontally = mergeableRows.Count > 0;
            // bool canMergeVertically = mergeableColumns.Count > 0;

            if (multiplier == 0)
            {
                Map.ResetMergeAudioIndex();

                return 0;
            }
            
            // Extract crossing indexes so we can treat them specifically
            // List<Vector2Int> crossingPositions = GetCrossingPositions(mergeableRows, mergeableColumns);
            // foreach (var crossingPosition in crossingPositions)
            // {
            //     Debug.LogError("Crossing merge position at : " + crossingPosition);
            // }

            List<Place> travelledPlaces = new();

            Tween lastTween = null;
            
            foreach (var mergeableColumn in mergeableColumns)
            {
                TravelPawnsVerticallyToClosestIndex(mergeableColumn, mergeableRows, projectedPlaces, ref travelledPlaces, ref lastTween);
            }
            
            foreach (var mergeableRow in mergeableRows)
            {
                TravelPawnsHorizontallyToClosestIndex(mergeableRow, mergeableColumns, projectedPlaces, ref travelledPlaces, ref lastTween);
            }

            FinalizeTravel(travelledPlaces, multiplier);

            if (lastTween != null)
            {
                lastTween.onComplete += () => EmitCommonSpawnEffects(multiplier);
            }
                
            return multiplier;
        }

#region Merge Logic

        private void TravelPawnsVerticallyToClosestIndex(int column, List<int> mergeableRows, List<Place> projectedPlaces, ref List<Place> travelledPlaces, ref Tween lastTween)
        {
            int closest = 0;
            if (mergeableRows.Count == 0)
            {
                closest = PickVerticallyRandomFromProjectedPlaces(column, projectedPlaces);
            }
            
            for (int y = 0; y < _size.y; y++)
            {
                Place place = _places[column, y];
                if (place.Occupied)
                {
                    if (mergeableRows.Count > 0)
                    {
                        closest = GetClosestCrossRowAtVertical(place, mergeableRows);
                    }

                    Place targetPlace = _places[column, closest];

                    if (!travelledPlaces.Contains(targetPlace))
                    {
                        targetPlace.Value = 0;
                        travelledPlaces.Add(targetPlace);
                    }
                    
                    Travel(place, targetPlace, ref lastTween);
                }
            }
        }
        private void TravelPawnsHorizontallyToClosestIndex(int row, List<int> mergeableColumns, List<Place> projectedPlaces, ref List<Place> travelledPlaces, ref Tween lastTween)
        {
            int closest = 0;
            if (mergeableColumns.Count == 0)
            {
                closest = PickHorizontallyRandomFromProjectedPlaces(row, projectedPlaces);
            }
            for (int x = 0; x < _size.x; x++)
            {
                Place place = _places[x, row];
                if (place.Occupied)
                {
                    if (mergeableColumns.Count > 0)
                    {
                        closest = GetClosestCrossColumnAtHorizontal(place, mergeableColumns);
                    }

                    Place targetPlace = _places[closest, row];

                    if (!travelledPlaces.Contains(targetPlace))
                    {
                        targetPlace.Value = 0;
                        travelledPlaces.Add(targetPlace);
                    }
                    Travel(place, targetPlace, ref lastTween);
                }
            }
        }

        private int GetClosestCrossRowAtVertical(Place place, List<int> mergeableRows)
        {
            int closestRow = mergeableRows[0];

            foreach (var mergeableRow in mergeableRows)
            {
                int currentDistance = Mathf.Abs(closestRow - place.Index.y);
                int distance = Mathf.Abs(mergeableRow - place.Index.y);
                if (distance < currentDistance)
                {
                    closestRow = mergeableRow;
                }
            }

            return closestRow;
        }
        
        private int PickVerticallyRandomFromProjectedPlaces(int column, List<Place> projectedPlaces)
        {
            List<int> indexes = new();
            foreach (var projectedPlace in projectedPlaces)
            {
                if (column == projectedPlace.Index.x)
                {
                    indexes.Add(projectedPlace.Index.y);
                }
            }

            return indexes.Random();
        }
        
        private int PickHorizontallyRandomFromProjectedPlaces(int row, List<Place> projectedPlaces)
        {
            List<int> indexes = new();
            foreach (var projectedPlace in projectedPlaces)
            {
                if (row == projectedPlace.Index.y)
                {
                    indexes.Add(projectedPlace.Index.x);
                }
            }

            return indexes.Random();
        }

        private int GetClosestCrossColumnAtHorizontal(Place place, List<int> mergeableColumns)
        {
            int closestColumn = mergeableColumns[0];

            foreach (var mergeableColumn in mergeableColumns)
            {
                int currentDistance = Mathf.Abs(closestColumn - place.Index.x);
                int distance = Mathf.Abs(mergeableColumn - place.Index.x);
                if (distance < currentDistance)
                {
                    closestColumn = mergeableColumn;
                }
            }

            return closestColumn;
        }
        

        private List<Vector2Int> GetCrossingPositions(List<int> mergeableRows, List<int> mergeableColumns)
        {
            List<Vector2Int> crossingIndexes = new();
            
            bool canMergeHorizontally = mergeableRows.Count > 0;
            bool canMergeVertically = mergeableColumns.Count > 0;

            if (canMergeHorizontally && canMergeVertically)
            {
                foreach (var column in mergeableColumns)
                {
                    foreach (var row in mergeableRows)
                    {
                        crossingIndexes.Add(new Vector2Int(column, row));
                    }
                }

                return crossingIndexes;
            }
            
            return crossingIndexes;
        }
        
        
        private List<int> ExtractUniqueRows(List<Place> places)
        {
            List<int> indexes = new();
                
            foreach (var place in places)
            {
                int index = place.Index.y;
                if (!indexes.Contains(index))
                {
                    indexes.Add(index);
                }
            }
                
            return indexes;
        }
        
        private List<int> ExtractUniqueColumns(List<Place> places)
        {
            List<int> indexes = new();
                
            foreach (var place in places)
            {
                int index = place.Index.x;
                if (!indexes.Contains(index))
                {
                    indexes.Add(index);
                }
            }
                
            return indexes;
        }
        

        private bool CanMergeVerticallyAtColumn(int column)
        {
            for (int y = 0; y < _size.y; y++)
            {
                if (!_places[column, y].Occupied)
                {
                    return false;
                }
            }

            return true;
        }
        
        
        private bool CanMergeHorizontallyAtRow(int row)
        {
            for (int x = 0; x < _size.x; x++)
            {
                if (!_places[x, row].Occupied)
                {
                    return false;
                }
            }

            return true;
        }
        
        
        private List<int> GetMergeableRows(List<int> rows)
        {
            List<int> indexes = new();
                
            foreach (var row in rows)
            {
                if (CanMergeHorizontallyAtRow(row))
                {
                    indexes.Add(row);
                }
            }
                
            return indexes;
        }
        
        private List<int> GetMergeableColumns(List<int> columns)
        {
            List<int> indexes = new();
                
            foreach (var column in columns)
            {
                if (CanMergeVerticallyAtColumn(column))
                {
                    indexes.Add(column);
                }
            }
                
            return indexes;
        }
        
#endregion
        

        private void Travel(Place fromPlace, Place toPlace, ref Tween lastTween)
        {
            Pawn pawn = fromPlace.Current;
            
            if (!pawn.Unpack())
            {
                return;
            }
            
            pawn.Available = false;
            pawn.OnMerge();

            toPlace.Value += pawn.Amount;
            
            fromPlace.Current = null;

            
            pawn.PunchScaleModelPivot(AnimConst.THIS.mergedPunchScale, AnimConst.THIS.mergedPunchDuration);
            pawn.thisTransform.parent = null;
            pawn.thisTransform.DOKill();
            
            Tween tween = pawn.thisTransform.DOMove(toPlace.PawnTargetPosition, AnimConst.THIS.mergeTravelDur)
                .SetEase(AnimConst.THIS.mergeTravelEase, AnimConst.THIS.mergeTravelShoot)
                .SetDelay(AnimConst.THIS.mergeTravelDelay);
                        
            tween.onComplete = () =>
            {
                pawn.EmitExplodeEffect();
                Particle.Star.Emit(2, toPlace.Position);
                pawn.Deconstruct();
            };

            lastTween = tween;
        }


        private void FinalizeTravel(List<Place> finalPlaces, int multiplier)
        {
            foreach (var finalPlace in finalPlaces)
            {
                finalPlace.Value = Mathf.Min(finalPlace.Value * multiplier,StackLimit);

                if (finalPlace.Value == 0)
                {
                    continue;
                }
            
                SpawnPawn(finalPlace, Pawn.Usage.Ammo, finalPlace.Value).MakeAvailable();

                finalPlace.Value = 0;
            }
        }
        
        // private void CreatePawnAtHorizontal(int horizontal, int lineIndex, int multiplier, int mergeIndex)
        // {
            // Place spawnPlace = _places[horizontal, lineIndex];
            // int totalAmmo = 0;
            // Tween lastTween = null;
            
            // for (int i = 0; i < _size.x; i++)
            // {
                // Place place = _places[i, lineIndex];
                // Pawn pawn = place.Current;
                // if (!pawn)
                // {
                    // continue;
                // }

                
                // bool canMerge = pawn.Unpack();

                // if (!canMerge)
                // {
                    // continue;
                // }
                
                // totalAmmo += pawn.Amount;

                // pawn.Available = false;
                // pawn.OnMerge();
                
                
                // pawn.PunchScaleModelPivot(AnimConst.THIS.mergedPunchScale, AnimConst.THIS.mergedPunchDuration);
                // pawn.thisTransform.parent = null;
                // pawn.thisTransform.DOKill();
                
                // Tween tween = pawn.thisTransform.DOMove(spawnPlace.PawnTargetPosition, AnimConst.THIS.mergeTravelDur)
                //     .SetEase(AnimConst.THIS.mergeTravelEase, AnimConst.THIS.mergeTravelShoot)
                //     .SetDelay(AnimConst.THIS.mergeTravelDelay);
                
                // tween.onComplete = () =>
                // {
                //     pawn.EmitExplodeEffect();
                //     pawn.Deconstruct();
                //
                //     // if (pawn.RecentBlockData)
                //     // {
                //     //     Particle.Debris.Emit(5, spawnPlace.Position, pawn.RecentBlockData.Color);
                //     // }
                // };
                
                // lastTween = tween;
                
                // place.Current = null;
            // }

            // Audio spawnAudio = Audio.Board_Spawn_Ammo;
            //
            // if (lastTween != null)
            // {
            //     lastTween.onComplete += () =>
            //     {
            //         HapticManager.Vibrate(HapticPatterns.PresetType.HeavyImpact);
            //         CameraManager.THIS.Shake(Random.Range(0.2f, 0.225f) + (0.2f * (multiplier - 1)), 0.5f);
            //         spawnAudio.PlayOneShot();
            //     };
            // }

            // totalAmmo = Mathf.Min(totalAmmo * multiplier,StackLimit);
            // if (totalAmmo == 0)
            // {
            //     return;
            // }

            // Pawn.Usage type = Pawn.Usage.Ammo;
            // int ammo = totalAmmo;

            // switch (mergeIndex)
            // {
            //     case 0:
            //         type = Pawn.Usage.Ammo;
            //         ammo = totalAmmo;
            //         spawnAudio = Audio.Board_Spawn_Ammo;
            //         break;
            //     case 1:
            //         type = Pawn.Usage.Energy;
            //         ammo = 0;
            //         break;
            //     default:
            //         type = Pawn.Usage.Gift;
            //         ammo = 0;
            //         break;
            // }
            
            // SpawnPawn(spawnPlace, type, ammo, true).MakeAvailable();
        // }

        private void EmitCommonSpawnEffects(float multiplier)
        {
            HapticManager.Vibrate(HapticPatterns.PresetType.HeavyImpact);
            CameraManager.THIS.Shake(Random.Range(0.2f, 0.225f) + (0.2f * (multiplier - 1)), 0.5f);
            Audio.Board_Spawn_Ammo.PlayOneShot();
        }

        // private void SpawnPawn(Place place, int totalAmmo, int multiplier)
        // {
        //     
        // }
        

        private Pawn SpawnPawn(Place place, Pawn.Usage usage, int amount)
        {
            Pawn pawn = Spawner.THIS.SpawnPawn(null, place.Position, amount, usage);
            place.Accept(pawn); // TODO
            return pawn;
        }

        // public void MergeLines(List<int> lines)
        // {
        //     for (int i = 0; i < lines.Count; i++)
        //     {
        //         MergeLine(lines[i], i, lines.Count);
        //     }
        // }
        
        // private void MergeLine(int lineIndex, int mergeIndex, int mult)
        // {
        //     int highestTick = int.MinValue;
        //     int horIndex = 0;
        //
        //     for (int i = 0; i < _size.x; i++)
        //     {
        //         int index = i;
        //         Place place = _places[index, lineIndex];
        //
        //         if (!place.Current)
        //         {
        //             continue;
        //         }
        //         if (place.Current.SkipMerge)
        //         {
        //             continue;
        //         }
        //         if (place.Current.Tick > highestTick)
        //         {
        //             highestTick = place.Current.Tick;
        //             horIndex = index;
        //         }
        //         else if(place.Current.Tick == highestTick)
        //         {
        //             if(Helper.IsPossible(0.5f))
        //             {
        //                 horIndex = index;
        //             }
        //         }
        //     }
        //     CreatePawnAtHorizontal(horIndex, lineIndex, mult, mergeIndex);
        // }

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
                    
                    if (place.Occupied)
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
            SpawnPawn(randomPlace, Pawn.Usage.Bomb, extra);
        }
#if CREATIVE
        private int posIndex = 0;
#endif  
        public void DestroyWithProjectile(ParticleSystem ps, Vector3 startPosition)
        {
            ps.Clear();
            Vector2Int pos = new Vector2Int(Random.Range(0, _size.x), Random.Range(0, _size.y));
#if CREATIVE
            pos = Const.THIS.creativeSettings.poses[posIndex++];
#endif
            Vector3 targetPosition = _places[pos.x, pos.y].PlacePosition;
            targetPosition.y += 0.5f;
            
            ps.Play();
            Transform psTransform = ps.transform;
            psTransform.position = startPosition;
            psTransform.DOJump(targetPosition, 2.0f, 1, 1.0f).onComplete = () =>
            {
                ps.Stop();
                Particle.Missile_Explosion.Play(targetPosition);
                Audio.Bomb_Explode.PlayOneShot();

                ExplodePawnsCircular(pos, Board.BombRadius);
                // MarkDropPointsMover();
                CameraManager.THIS.Shake(Random.Range(0.4f, 0.45f), 0.5f);
            };
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
                    
                    tween.onComplete = () =>
                    {
                        pawn.EmitExplodeEffect();
                        pawn.Deconstruct();
                        // if (pawn.RecentBlockData)
                        // {
                        //     Particle.Debris.Emit(5, spawnPlace.Position, pawn.RecentBlockData.Color);
                        // }
                        

                    };

                    lastTween = tween;

                    place.Current = null;
                }
            }

            if (lastTween != null)
            {
                lastTween.onComplete += () =>
                {
                    CameraManager.THIS.Shake(Random.Range(0.3f, 0.4f), 0.5f);

                    if (totalAmmo > 0)
                    {
                        Audio.Board_Spawn_Ammo.PlayOneShot();
                        // Audio.Board_Post_Merge.PlayOneShot();
                        
                        HapticManager.Vibrate(HapticPatterns.PresetType.HeavyImpact);
                        CameraManager.THIS.Shake(Random.Range(0.2f, 0.225f), 0.5f);
                        
                        // Particle.Debris.Emit(30, spawnPlace.Position);
                        Particle.Star.Emit(15, spawnPlace.Position);
                    }
                };
            }

            totalAmmo = Mathf.Min(totalAmmo,StackLimit);
            if (totalAmmo == 0)
            {
                return;
            }
            // Audio.Board_Merge_Rising.PlayOneShotPitch(1.0f, 1);
            Audio.Board_Pre_Merge.PlayOneShotPitch(0.75f);

            // Audio.Board_Merge_Cock.PlayOneShot();

            SpawnPawn(spawnPlace, Pawn.Usage.Ammo, totalAmmo).MakeAvailable();
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

                    // pawn.EmitExplodeEffect();
                    pawn.Explode(place.Index);
                    
                    // if (pawn.RecentBlockData)
                    // {
                    //     Particle.Debris.Emit(5, place.Position, pawn.RecentBlockData.Color);
                    // }

                    RemovePawn(place);
                    // Particle.Debris.Emit(30, place.Position);
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

        
        // public void MarkMoverByTetris(List<int> tetrisLines)
        // {
        //     // int min = tetrisLines.Min();
        //     
        //     // Call<Place>(_places, (place, horizontalIndex, verticalIndex) =>
        //     // {
        //     //     if (place.Current && !place.Current.Connected && verticalIndex > min)
        //     //     {
        //     //         place.Current.Mover = true;
        //     //     }
        //     // });
        //     
        //     for (int y = 0; y < tetrisLines.Count; y++)
        //     {
        //         int plusLineIndex = tetrisLines[y] + 1;
        //         if (tetrisLines.Contains(plusLineIndex) || plusLineIndex >= _size.y)
        //         {
        //             continue;
        //         }
        //         
        //         for (int x = 0; x < _size.x; x++)
        //         {
        //             Place place = _places[x, plusLineIndex];
        //             if (place.Occupied && !place.Current.Connected)
        //             {
        //                 place.Current.JumpUp(0.2f, 0.3f, 0.25f);
        //             }
        //         }
        //     }
        // }
       
        
        // public void MarkMovers(int x, int y)
        // {
        //     Call<Place>(_places, (place, horizontalIndex, verticalIndex) =>
        //     {
        //         if (place.Current && !place.Current.Connected && horizontalIndex == x && verticalIndex >= y)
        //         {
        //             place.Current.Mover = true;
        //         }
        //     });
        // }

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
                        && !place.Current.Busy
                        && place.Current.Available)
                    {
                        
                        place.Current.Amount -= 1;

                        
                        place.Current.UseSubModel();
                        if (place.Current.Amount <= 0)
                        {
                            place.Current.DetachSubModelAndDeconstruct();
                            place.Current = null;
                            // MarkMovers(place.Index.x, place.Index.y);
                        }
                        
                
                        ammoGiven += splitCount;
                        return ammoGiven;
                    }
                }
            }
            
            return ammoGiven;
        }

        // private bool ExpectedMoverComing(Vector2Int index)
        // {
        //     if (index.y + 1 >= _size.y)
        //     {
        //         return false;
        //     }
        //     Place place = _places[index.x, index.y + 1];
        //     
        //     return place.Current && place.Current.Mover;
        // }
        
        public void Place(Block block)
        {
            Map.THIS.MapWaitForCycle = true;
            
            // List<Pawn> temporary = new List<Pawn>(block.Pawns);
            List<Place> projectedPlaces = new();

            // block.PlacedOnGrid = true;

            for (int i = 0; i < block.Pawns.Count; i++)
            {
                Pawn pawn = block.Pawns[i];

                Place place = GetPlace(pawn);
                projectedPlaces.Add(place);
                
                pawn.Busy = true;

                place.Accept(pawn, 0.05f, (i == block.Pawns.Count - 1) ? () => Map.THIS.CheckTetris(projectedPlaces) : null);
            }
            block.Detach();
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
            if (posDif.x < 0.0f)
            {
                posDif.x = -4;
            }
            if (-posDif.z < 0.0f)
            {
                posDif.z = 4;
            }
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
            // if (ExpectedMoverComing(indexValue))
            // {
            //     return (place, false);
            // }
            if (pawn.VData.free2Place)
            {
                return (place, true);
            }
            
            // if (_size.y - pawn.ParentBlock.blockData.FitHeight > indexValue.y)
            if (_size.y - _size.y > indexValue.y)
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
                        // totalVertShiftStart = block.blockData.NormalHeight - 1 + (_size.y - block.blockData.FitHeight);
                        totalVertShiftStart = block.blockData.NormalHeight - 1 + (_size.y - _size.y);
                        totalVertShiftEnd = _size.y;
                        break;
                    case 90:
                        zeroShift = new Vector3(1.5f, 0.0f, -1.0f);
                        totalHorShiftStart = 0;
                        totalHorShiftEnd = _size.x - block.blockData.NormalHeight + 1;
                        // totalVertShiftStart = _size.y - block.blockData.FitHeight;
                        totalVertShiftStart = _size.y - _size.y;
                        totalVertShiftEnd = _size.y - block.blockData.NormalWidth + 1;
                        break;
                    case 180:
                        zeroShift = new Vector3(-1.0f, 0.0f, -1.5f);
                        totalHorShiftStart = block.blockData.NormalWidth - 1;
                        totalHorShiftEnd = _size.x;
                        // totalVertShiftStart = _size.y - block.blockData.FitHeight;
                        totalVertShiftStart = _size.y - _size.y;
                        totalVertShiftEnd = _size.y - block.blockData.NormalHeight + 1;
                        break;
                    case 270:
                        zeroShift = new Vector3(-1.5f, 0.0f, 1.0f);
                        totalHorShiftStart = block.blockData.NormalHeight - 1;
                        totalHorShiftEnd = _size.x;
                        // totalVertShiftStart = block.blockData.NormalWidth - 1 + _size.y - block.blockData.FitHeight;
                        totalVertShiftStart = block.blockData.NormalWidth - 1 + _size.y - _size.y;
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
                            
                            if (!place)
                            {
                                found = false;
                                break;
                            }

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
            [SerializeField] public bool unlimitedStack = false;
            [SerializeField] public bool unlimitedPeek = false;
            
            public Data()
            {
                
            }
            public Data(Data data)
            {
                this.defaultStack = data.defaultStack;
                this.maxStack = data.maxStack;
                this.unlimitedStack = data.unlimitedStack;
            }

            public object Clone()
            {
                return new Data(this);
            }
        }
    }
}