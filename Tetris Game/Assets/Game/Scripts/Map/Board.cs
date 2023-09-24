using System.Collections;
using Internal.Core;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Game
{
    public class Board : Singleton<Board>
    {
        [SerializeField] private Vector3 indexOffset;
        [SerializeField] public Vector2Int Size;
        [System.NonSerialized] private Transform _thisTransform;
        [System.NonSerialized] private Vector3 _thisPosition;
        [System.NonSerialized] private Place[,] places;
        [System.NonSerialized] private int _tick = 0;
        [System.NonSerialized] public System.Action OnMerge;
        [System.NonSerialized] private Tween _delayedHighlightTween = null;

        [System.NonSerialized] private Data _data;

        void Awake()
        {
            this._thisTransform = this.transform;
        }

        public Data _Data
        {
            set
            {
                _data = value;
            }
            get => _data;
        }
        
        public void Construct()
        {
            places = new Place[Size.x, Size.y];
            for (int i = 0; i < Size.x; i++)
            {
                for(int j = 0; j < Size.y; j++)
                {
                    Place place = Pool.Place.Spawn<Place>(_thisTransform);
                    place.LocalPosition = new Vector3(i, 0.0f, -j);
                    places[i, j] = place;
                    place.Index = new Vector2Int(i, j);
                    place.Construct();
                }
            }

            _thisTransform.localPosition = new Vector3(-Size.x * 0.5f + 0.5f, 0.0f, Size.y * 0.5f + 1.75f);
            this._thisPosition = _thisTransform.position;
        }

        public void Deconstruct()
        {
            DehighlightImmediate();
            HideSuggestedPlaces();
            
            Call<Place>(places, (place) =>
            {
                place.Deconstruct();
            });
        }
        
        public void OnLevelEnd()
        {
            Call<Place>(places, (place) =>
            {
                place.OnLevelEnd();
            });
        }
        public void MoveAll(float moveDuration)
        {
            _tick++;

            Call<Place>(places, (place) =>
            {
                if (!place.Current) return;
                place.Current.MoveForward(place, _tick, moveDuration);
            });
        }
        public void CheckAll()
        {
            Call<Place>(places, (place) =>
            {
                if (place.Current)
                {
                    place.Current.Check(place);
                }
            });
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

        public void KillDelayedHighlight()
        {
            _delayedHighlightTween?.Kill();
            _delayedHighlightTween = null;
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
            if (!Spawner.THIS._currentBlock)
            {
                // There is no block to suggest place or check deadlock, skip
                return;
            }
            if (Spawner.THIS._currentBlock.Pawns[0].Free2Place)
            {
                HighlightEmptyPlaces();
                // There is no block to suggest place or check deadlock, skip
                return;
            }
            if (Spawner.THIS._currentBlock.RequiredPlaces != null && Spawner.THIS._currentBlock.RequiredPlaces.Count > 0)
            {
                Highlight(Spawner.THIS._currentBlock.RequiredPlaces);
                _delayedHighlightTween = DOVirtual.DelayedCall(1.5f, () =>
                {
                    Highlight(Spawner.THIS._currentBlock.RequiredPlaces);
                    _delayedHighlightTween = null;
                });
                // Running a suggestion loop via suggested location by level design, skip
                return;
            }
           
            for (int i = 0; i < Size.x; i++)
            {
                for (int j = 0; j < Size.y; j++)
                {
                    Place place = places[i, j];
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

            List<List<Place>> allPlaces = DetectFit(Spawner.THIS._currentBlock);

            if (allPlaces.Count > 0)
            {
                List<Place> randomPlaces = allPlaces.Random();
                _delayedHighlightTween = DOVirtual.DelayedCall(4.0f, () =>
                {
                    Highlight(randomPlaces);
                    _delayedHighlightTween = null;
                });
                // Found a fit, suggest/highlight it, skip
                return;
            }
            if (ONBOARDING.LEARNED_POWERUP.IsNotComplete())
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
        public void Dehighlight()
        {
            Call<Place>(places, (place) => 
                {
                    place.SetPlaceType(Game.Place.PlaceColor.NORMAL);
                });
        }
        public void DehighlightImmediate()
        {
            Call<Place>(places, (place) => 
            {
                place.SetPlaceTypeImmediate(Game.Place.PlaceColor.NORMAL);
            });
        }
        public Place LinearIndex2Place(int index)
        {
            Vector2Int ind = index.ToIndex(Size.y);
            return places[ind.x, ind.y];
        }
        private void Call<T>(T[,] array, System.Action<T> action)
        {
            for (int i = 0; i < Size.x; i++)
            {
                for (int j = 0; j < Size.y; j++)
                {
                    action.Invoke(array[i, j]);
                }
            }
        }
        private void Call<T>(T[,] array, System.Action<T, int, int> action)
        {
            for (int i = 0; i < Size.x; i++)
            {
                for (int j = 0; j < Size.y; j++)
                {
                    action.Invoke(array[i, j], i, j);
                }
            }
        }
        private void CallRow<T>(T[,] array, int lineIndex, System.Action<T, int> action)
        {
            for (int i = 0; i < Size.x; i++)
            {
                action.Invoke(array[i, lineIndex], i);
            }
        } 
        private void CallColumn<T>(T[,] array, int columnIndex, System.Action<T, int> action)
        {
            for (int j = 0; j < Size.y; j++)
            {
                action.Invoke(array[columnIndex, j], j);
            }
        }
        public Place GetPlace(Vector2Int index) => places[index.x, index.y];

        
        public List<Vector2Int> UsePowerups()
        {
            List<Vector2Int> points = new();

            for (int j = 0; j < Size.y; j++)
            {
                for (int i = 0; i < Size.x; i++)
                {
                    if (!places[i, j].Occupied)
                    {
                        continue;
                    }

                    if (places[i, j].Current.UsageType.Equals(Pawn.Usage.MagnetLR))
                    {
                        CreatePawnAtHorizontal(i, j);
                        for (int k = 0; k < Size.x; k++)
                        {
                            points.Add(new Vector2Int(k, j));
                        }
                        return points;
                    }
                    if (places[i, j].Current.UsageType.Equals(Pawn.Usage.MagnetUD))
                    {
                        CreatePawnAtVertical(i, j);
                        points.Add(new Vector2Int(i, j));
                        return points;
                    }
                    if (places[i, j].Current.UsageType.Equals(Pawn.Usage.Magnet))
                    {
                        CreatePawnAtCircular(i, j, points);
                        return points;
                    }
                }
            }

            return points;
        }
        
        public List<int> CheckTetris()
        {
            List<int> tetrisLines = new();
            for (int j = 0; j < Size.y; j++)
            {
                bool tetris = true;
                bool forceMerger = false;
                for (int i = 0; i < Size.x; i++)
                {
                    
                    if (!places[i, j].Occupied)
                    {
                        tetris = false;
                        continue;
                    }
                    if (places[i, j].Current.Mover)
                    {
                        tetris = false;
                        continue;
                    }
                    
                }
                if (tetris || forceMerger)
                {
                    tetrisLines.Add(j);
                }
            }
            return tetrisLines;
        }

        private void SpawnMergedPawn(Place place, int level)
        {
            Vector3 mergedPawnPosition = place.Position;
            if (level <= 0)
            {
                return;
            }
            Pawn mergedPawn = Spawner.THIS.SpawnPawn(null, mergedPawnPosition, level, Pawn.Usage.UnpackedAmmo);
            
            place.AcceptNow(mergedPawn);

            mergedPawn.CanTakeAmmo = false;
            mergedPawn.Mover = true;

            
            mergedPawn.AnimatedShow(AnimConst.THIS.MergeShowDelay, AnimConst.THIS.mergedScalePunch, AnimConst.THIS.mergedScaleDuration, 
                () =>
            {
                UIManagerExtensions.Distort(mergedPawnPosition + Vector3.up * 0.45f, 0.0f);
                Particle.Merge_Circle.Play(mergedPawnPosition  + new Vector3(0.0f, 0.85f, 0.0f), scale : Vector3.one * 0.5f);
                mergedPawn.CanTakeAmmo = true;
            });


        }

        public void MergeLines(List<int> lines)
        {
            void MergeLine(int lineIndex, int multiplier)
            {
                int highestTick = int.MinValue;
                int mergeIndex = 0;

                for (int i = 0; i < Size.x; i++)
                {
                    int index = i;
                    Place place = places[index, lineIndex];

                    if (!place.Current)
                    {
                        continue;
                    }
                    if (place.Current.Tick > highestTick)
                    {
                        highestTick = place.Current.Tick;
                        mergeIndex = index;
                    }
                    else if(place.Current.Tick == highestTick)
                    {
                        Helper.IsPossible(0.5f,() => { mergeIndex = index; } );
                    }
                }
                CreatePawnAtHorizontal(mergeIndex, lineIndex);
            }
            
            OnMerge?.Invoke();

            for (int i = 0; i < lines.Count; i++)
            {
                MergeLine(lines[i], lines.Count);
            }
        }

        private void CreatePawnAtHorizontal(int horizontal, int lineIndex)
        {
            Place spawnPlace = places[horizontal, lineIndex];
            int totalPoint = 0;
            Pawn lastPawn = null;
            
            float delay = 0.0f;

            
            for (int i = 0; i < Size.x; i++)
            {
                Place place = places[i, lineIndex];
                Pawn pawn = place.Current;
                if (!pawn)
                {
                    continue;
                }


                lastPawn = pawn;

                totalPoint += pawn.Amount;
                
                pawn.Unpack(delay += 0.025f);
                
                
                pawn.PunchScaleModelPivot(AnimConst.THIS.mergedPunchScale, AnimConst.THIS.mergedPunchDuration);
                pawn.transform.DOMove(spawnPlace.segmentParent.position, AnimConst.THIS.mergeTravelDur).SetEase(AnimConst.THIS.mergeTravelEase, AnimConst.THIS.mergeTravelShoot).SetDelay(AnimConst.THIS.mergeTravelDelay)
                    .onComplete += () =>
                {
                    pawn.Deconstruct();

                    if (lastPawn == pawn)
                    {
                        Pool.Cube_Explosion.Spawn<CubeExplosion>().Explode(spawnPlace.Position + new Vector3(0.0f, 0.6f, 0.0f));
                    }
                };

                place.Current = null;
            }

            totalPoint = Mathf.Clamp(totalPoint, 0, _Data.maxStack);
            SpawnMergedPawn(spawnPlace, totalPoint);
        }
        
        private void CreatePawnAtVertical(int horizontal, int vertical)
        {
            Place spawnPlace = places[horizontal, vertical];
            int totalPoint = 0;
            Pawn lastPawn = null;
            
            float delay = 0.0f;
            
            for (int i = 0; i < Size.y; i++)
            {
                Place place = places[horizontal, i];
                Pawn pawn = place.Current;
                if (!pawn)
                {
                    continue;
                }


                lastPawn = pawn;

                totalPoint += pawn.Amount;
                
                pawn.Unpack(delay += 0.025f);

                pawn.PunchScaleModelPivot(AnimConst.THIS.mergedPunchScale, AnimConst.THIS.mergedPunchDuration);
                pawn.transform.DOMove(spawnPlace.segmentParent.position, AnimConst.THIS.mergeTravelDur).SetEase(AnimConst.THIS.mergeTravelEase, AnimConst.THIS.mergeTravelShoot).SetDelay(AnimConst.THIS.mergeTravelDelay)
                    .onComplete += () =>
                {
                    pawn.Deconstruct();

                    if (lastPawn == pawn)
                    {
                        Pool.Cube_Explosion.Spawn<CubeExplosion>().Explode(spawnPlace.Position + new Vector3(0.0f, 0.6f, 0.0f));
                    }
                };

                place.Current = null;
            }

            totalPoint = Mathf.Clamp(totalPoint, 0, _Data.maxStack);
            SpawnMergedPawn(spawnPlace, totalPoint);
        }

        private void CreatePawnAtCircular(int horizontal, int vertical, List<Vector2Int> points)
        {
            Vector2Int center = new Vector2Int(horizontal, vertical);
            
            Place spawnPlace = places[horizontal, vertical];
            int totalPoint = 0;
            Pawn lastPawn = null;
            
            float delay = 0.0f;
            
            for (int i = 0; i < Size.x; i++)
            {
                for (int j = 0; j < Size.y; j++)
                {
                    Place place = places[i, j];
                    Pawn pawn = place.Current;
                    
                    if (!pawn)
                    {
                        continue;
                    }
                    
                    Vector2Int current = new Vector2Int(i, j);
                    if (Vector2Int.Distance(center, current) > 2.5f)
                    {
                        continue;
                    }

                    bool placed = false;

                    for (int k = 0; k < points.Count; k++)
                    {
                        if (current.x == points[k].x && current.y < points[k].y)
                        {
                            points[k] = current;
                            placed = true;
                            break;
                        }
                    }

                    if (!placed)
                    {
                        points.Add(current);
                    }

                    lastPawn = pawn;

                    totalPoint += pawn.Amount;
                    
                    pawn.Unpack(delay += 0.025f);

                    pawn.PunchScaleModelPivot(AnimConst.THIS.mergedPunchScale, AnimConst.THIS.mergedPunchDuration);
                    pawn.transform.DOMove(spawnPlace.segmentParent.position, AnimConst.THIS.mergeTravelDur).SetEase(AnimConst.THIS.mergeTravelEase, AnimConst.THIS.mergeTravelShoot).SetDelay(AnimConst.THIS.mergeTravelDelay)
                        .onComplete += () =>
                    {
                        pawn.Deconstruct();

                        if (lastPawn == pawn)
                        {
                            Pool.Cube_Explosion.Spawn<CubeExplosion>().Explode(spawnPlace.Position + new Vector3(0.0f, 0.6f, 0.0f));
                        }
                    };

                    place.Current = null;
                }
            }

            totalPoint = Mathf.Clamp(totalPoint, 0, _Data.maxStack);
            SpawnMergedPawn(spawnPlace, totalPoint);
        }
        public void MarkMover(int horizontal)
        {
            Call<Place>(places, (place, horizontalIndex, verticalIndex) =>
            {
                if (place.Current && !place.Current.Connected && verticalIndex >= horizontal)
                {
                    place.Current.Mover = true;
                    place.Current.JumpUp(0.2f, 0.3f, (verticalIndex - horizontal) * 0.075f + 0.25f);
                }
            });
        }
        public void MarkMover(List<Vector2Int> moverPoints)
        {
            foreach (var point in moverPoints)
            {
                for (int j = point.y; j < Size.y; j++)
                {
                    Place place = places[point.x, j];
                    if (place.Current && !place.Current.Connected)
                    {
                        place.Current.Mover = true;
                        place.Current.JumpUp(0.2f, 0.3f, (j - point.y) * 0.075f + 0.25f);
                    }
                }
            }
        }
        
        public void MarkMovers(int x, int y)
        {
            Call<Place>(places, (place, horizontalIndex, verticalIndex) =>
            {
                if (place.Current && !place.Current.Connected && horizontalIndex == x && verticalIndex >= y)
                {
                    place.Current.Mover = true;
                    place.Current.JumpUp(0.2f, 0.3f, (verticalIndex - y - 1) * 0.075f);
                }
            });
        }

        public int ConsumeBullet(int splitCount)
        {
            int ammoGiven = 0;
            
            for (int j = 0; j < Size.y; j++)
            {
                for (int i = 0; i < Size.x; i++)
                {
                    if (splitCount <= 0)
                    {
                        return ammoGiven;
                    }
                    Place place = places[i, j];
                    
                    if (place.Current && !place.Current.Mover && !place.Current.Busy && place.Current.UsageType.Equals(Pawn.Usage.UnpackedAmmo) && place.Current.CanTakeAmmo)
                    {
                        Pawn currentPawn = place.Current;
                        int ammo = Mathf.Min(currentPawn.Amount, splitCount);
                        currentPawn.Amount -= ammo;
                        if (currentPawn.Amount > 0)
                        {
                            currentPawn.PunchScaleModelPivot(-0.4f);
                            Particle.Square_Bullet.Emit(1, currentPawn.transform.position, rotation: Quaternion.Euler(90.0f, 0.0f, 0.0f));
                        }
                        else
                        {
                            place.Current = null;
                            currentPawn.Hide(currentPawn.Deconstruct);
                            MarkMovers(place.Index.x, place.Index.y);
                        }
                
                        ammoGiven += ammo;
                        splitCount -= ammo;
                    }
                }
            }
            
            return ammoGiven;
        }
        public bool HasForwardPawnAtColumn(Vector2Int index)
        {
            
            for (int j = 0; j < Size.y; j++)
            {
                Place place = places[index.x, j];
                if (index.y <= j && place.Current != null)
                {
                    return true;
                }
            }
            return false;
        }

        private bool ExpectedMoverComing(Vector2Int index)
        {
            if (index.y + 1 >= Size.y)
            {
                return false;
            }
            Place place = places[index.x, index.y + 1];
            
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
                currentPawn.Mover = true;
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
            Vector2Int? index = Pos2Index(pawn.transform.position);
            if (index != null)
            {
                return GetPlace((Vector2Int)index);
            }
            return null;
        }
        private Vector2Int? Pos2Index(Vector3 position)
        {
            Vector3 posDif = (position + Spawner.THIS.distanceOfBlockCast) - _thisPosition + indexOffset;
            Vector2 posFin = new Vector2(posDif.x, -posDif.z);
            Vector2Int? index = null;
            if (posFin.x >= 0.0f && posFin.x < Size.x && posFin.y >= 0.0f && posFin.y < Size.y)
            {
                index = new Vector2Int((int)posFin.x, (int)posFin.y);
            }
            return index;
        }
        private (Place, bool) Project(Pawn pawn, List<Place> requiredPlaces)
        {
            Vector2Int? index = Pos2Index(pawn.transform.position);
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
            if (pawn.Free2Place)
            {
                return (place, true);
            }
            
            if (Size.y - pawn.ParentBlock.blockData.FitHeight > indexValue.y)
            {
                return (place, false);
            }
            if (requiredPlaces is { Count: > 0 } && !requiredPlaces.Contains(place))
            {
                return (place, false);
            }
            
            return (place, true);
        }
        public Place IsEmpty(Vector3 position)
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
        public void HighlightPawnOnGrid(Block block)
        {
            Board.THIS.Dehighlight();
            foreach (var pawn in block.Pawns)
            {
                (Place place, bool canPlace) = Project(pawn, block.RequiredPlaces);
                if (place)
                {
                    place.SetPlaceType(canPlace ? Game.Place.PlaceColor.GREEN : Game.Place.PlaceColor.RED);
                }
            }
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

        public void ShowAvailablePlaces()
        {
            Call<Place>(places, (place, horizontalIndex, verticalIndex) =>
            {
                if (place.Current) return;

                Particle.Blue_Zone.Emit(1, place.Position, Quaternion.Euler(90.0f, 0.0f, 0.0f));
            });
        }
        
        public void ShowTicketMergePlaces()
        {
            CallRow<Place>(places, 0, (place, horizontalIndex) =>
            {
                Particle.Yellow_Zone.Emit(1, place.Position, Quaternion.Euler(90.0f, 0.0f, 0.0f));
            });
        }
        
        public void Highlight(List<Place> places)
        {
            foreach (var place in places)
            {
                Particle.Blue_Zone.Emit(1, place.PlacePosition + new Vector3(0.0f, 0.01f, 0.0f), Quaternion.Euler(90.0f, 0.0f, 0.0f));
            }
        }
        public void HighlightEmptyPlaces()
        {
            foreach (var place in places)
            {
                if (place.Current)
                {
                    continue;
                }
                Particle.Blue_Zone.Emit(1, place.PlacePosition + new Vector3(0.0f, 0.01f, 0.0f), Quaternion.Euler(90.0f, 0.0f, 0.0f));
            }
        }

        public void HideSuggestedPlaces()
        {
            Particle.Blue_Zone.StopAndClear();
            Particle.Yellow_Zone.StopAndClear();
            KillDelayedHighlight();
        }
        
        [System.Serializable]
        public class SuggestedBlock
        {
            [SerializeField] public Pool type;
            [SerializeField] public List<int> requiredPlaces;
            [SerializeField] public BlockRot blockRot;
            [SerializeField] public bool canRotate = true;
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
            [SerializeField] public int pawnSat = 75;
            
            public Data()
            {
                
            }
            public Data(Data data)
            {
                this.defaultStack = data.defaultStack;
                this.maxStack = data.maxStack;
                this.defaultSupplyLine = data.defaultSupplyLine;
                this.pawnSat = data.pawnSat;
            }

            public object Clone()
            {
                return new Data(this);
            }
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
                        totalHorShiftEnd = Size.x - block.blockData.NormalWidth + 1;
                        totalVertShiftStart = block.blockData.NormalHeight - 1 + (Size.y - block.blockData.FitHeight);
                        totalVertShiftEnd = Size.y;
                        break;
                    case 90:
                        zeroShift = new Vector3(1.5f, 0.0f, -1.0f);
                        totalHorShiftStart = 0;
                        totalHorShiftEnd = Size.x - block.blockData.NormalHeight + 1;
                        totalVertShiftStart = Size.y - block.blockData.FitHeight;
                        totalVertShiftEnd = Size.y - block.blockData.NormalWidth + 1;
                        break;
                    case 180:
                        zeroShift = new Vector3(-1.0f, 0.0f, -1.5f);
                        totalHorShiftStart = block.blockData.NormalWidth - 1;
                        totalHorShiftEnd = Size.x;
                        totalVertShiftStart = Size.y - block.blockData.FitHeight;
                        totalVertShiftEnd = Size.y - block.blockData.NormalHeight + 1;
                        break;
                    case 270:
                        zeroShift = new Vector3(-1.5f, 0.0f, 1.0f);
                        totalHorShiftStart = block.blockData.NormalHeight - 1;
                        totalHorShiftEnd = Size.x;
                        totalVertShiftStart = block.blockData.NormalWidth - 1 + Size.y - block.blockData.FitHeight;
                        totalVertShiftEnd = Size.y;
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

                            Place place = IsEmpty(finalPos);
                            
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
    }
}