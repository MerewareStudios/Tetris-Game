using System;
using System;
using System;
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
        [System.NonSerialized] private Place[,] places;
        [System.NonSerialized] private int _tick = 0;
        [System.NonSerialized] public System.Action OnMerge;

        [System.NonSerialized] private Data _data;

        public Data _Data
        {
            set
            {
                _data = value;
                MaxStack = _data.maxStack;
            }
            get => _data;
        }
        
        public int MaxStack
        {
            set
            {
                _Data.maxStack = value;
                if (_Data.maxStack != _Data.defaultStack)
                {
                    StatDisplayArranger.THIS.Show(StatDisplay.Type.MaxStack, _data.maxStack);
                }
                else
                {
                    StatDisplayArranger.THIS.Hide(StatDisplay.Type.MaxStack);
                }
            }
            get => _Data.maxStack;
        }
        
        public int SupplyLine
        {
            set
            {
                int newValue = Mathf.Clamp(value, 0, Size.x * Size.y);
                if (_Data.supplyLine == newValue)
                {
                    return;
                }
                _Data.supplyLine = newValue;
                MarkSupplier(_Data.supplyLine);
            }
            get => _Data.supplyLine;
        }
        
        public void Construct()
        {
            places = new Place[Size.x, Size.y];
            for (int i = 0; i < Size.x; i++)
            {
                for(int j = 0; j < Size.y; j++)
                {
                    Place place = Pool.Place.Spawn<Place>(this.transform);
                    place.Construct();
                    place.transform.localPosition = new Vector3(i, 0.0f, -j);
                    places[i, j] = place;
                    place.index = new Vector2Int(i, j);
                }
            }

            // SupplyLine = _Data.supplyLine;
            MarkSupplier(_Data.supplyLine);

        }
        public void Deconstruct()
        {
            Call<Place>(places, (place) =>
            {
                place.Deconstruct();
            });
        }
        public void OnVictory()
        {
            Call<Place>(places, (place) =>
            {
                place.OnVictory();
            });
        }
        public void OnFail()
        {
            Call<Place>(places, (place) =>
            {
                place.OnFail();
            });
        }
        public void MoveAll(float moveDuration)
        {
            _tick++;

            int steadyPawnCount = 0;

            Call<Place>(places, (place) =>
            {
                if (!place.Current) return;
                
                bool mover = place.Current.MoveForward(place, _tick, moveDuration);

                if (!mover)
                {
                    steadyPawnCount++;
                }
            });

            if (steadyPawnCount > _Data.pawnSat)
            {
                UIManager.THIS.loanBar.MakeAvailable();
            }
            // _ = (steadyPawnCount > _Data.pawnSat) ? UIManager.THIS.loanBar.MakeAvailable() : UIManager.THIS.loanBar.MakeUnavailable();
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
        public void Dehighlight()
        {
            Call<Place>(places, (place) => 
                {
                    place.SetPlaceType(Game.Place.PlaceType.EMPTY);
                });
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

                    if (places[i, j].Current.UsageType.Equals(Pawn.Usage.HorMerge))
                    {
                        forceMerger = true;
                        break;
                    }

                    if (places[i, j].Current.MOVER)
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

        

        private void SpawnMergedPawn(Place place, int level, int mergeIndexPoint)
        {
            Vector3 mergedPawnPosition = place.transform.position;
            // Particle.Portal_Blue.Play(mergedPawnPosition + Vector3.up * 0.25f, Quaternion.Euler(90.0f, 0.0f, 0.0f), Vector3.one);
            if (level <= 0)
            {
                return;
            }
            Pawn mergedPawn = Spawner.THIS.SpawnPawn(null, mergedPawnPosition, level, Pawn.Usage.ShooterIdle);
            
            place.AcceptNow(mergedPawn);

            mergedPawn.MarkMergerColor();
            mergedPawn.AnimatedShow(0.35f, () =>
            {

                Particle.Merge_Circle.Play(mergedPawnPosition  + new Vector3(0.0f, 0.85f, 0.0f), scale : Vector3.one * 0.5f);
            }, () => 
                { 
                    mergedPawn.OnMerge();
                    UIManagerExtensions.EmitVibeText(mergedPawnPosition, "+" + (level - mergeIndexPoint));
                });
            
            UIManager.THIS.shopBar.Amount += level * 0.075f;

            UIManagerExtensions.Distort(mergedPawnPosition, 0.1f);

            // UIManager.THIS.ft_Level.FlyWorld("X1", mergedPawnPosition + new Vector3(0.0f, 0.5f, 0.0f), 0.0f);
        }

        public void MergeLines(List<int> lines, float duration)
        {
            OnMerge?.Invoke();
            
            for (int i = 0; i < lines.Count; i++)
            {
                MergeLine(lines[i], lines.Count, duration);
            }
            
            void MergeLine(int lineIndex, int multiplier, float duration)
            {
                List<Pawn> pawns = new();

                int totalPoint = 0;
                int highestTick = int.MinValue;
                int mergeIndex = 0;
                int mergeIndexPoint = 0;
                float delay = 0.0f;

                for (int i = 0; i < Size.x; i++)
                {
                    int index = i;
                    Place place = places[index, lineIndex];

                    if (!place.Current)
                    {
                        continue;
                    }
                
                    pawns.Add(place.Current);

                    int point = 0;
                    if (place.Current.Unbox(delay += 0.025f))
                    {
                        point = place.Current.Amount == 1 ? multiplier : place.Current.Amount;
                        totalPoint += point;
                        
                        // Particle.Merge_1.Play(place.Current.transform.position, scale : Vector3.one * 0.35f);
                    }

                    if (place.Current.Tick > highestTick)
                    {
                        highestTick = place.Current.Tick;
                        mergeIndex = index;
                        mergeIndexPoint = point;
                    }
                    else if(place.Current.Tick == highestTick)
                    {
                        Helper.IsPossible(0.5f,() =>
                                                {
                                                    mergeIndex = index;
                                                    mergeIndexPoint = point;
                                                }
                        );
                    }
                    place.Current = null;
                }

                Place spawnPlace = places[mergeIndex, lineIndex];
                foreach (var pawn in pawns)
                {
                    // Color color = multiplier == 1 ? Const.THIS.singleColor : Const.THIS.comboColor;
                    Particle.Square.Emit(1, pawn.transform.position, rotation: Quaternion.Euler(90.0f, 0.0f, 0.0f));
                
                    pawn.PunchScale(-0.1f, 0.2f);

                    pawn.transform.DOMove(spawnPlace.segmentParent.position, duration).SetDelay(0.15f)
                        .onComplete += () =>
                    {
                        pawn.Deconstruct();
                    };
                }

                totalPoint = Mathf.Clamp(totalPoint, 0, _Data.maxStack);
                SpawnMergedPawn(spawnPlace, totalPoint, mergeIndexPoint);
            }
        }

        public void MarkAllMover(int startLine)
        {
            Call<Place>(places, (place, horizontalIndex, verticalIndex) =>
            {
                if (place.Current && !place.Current.Connected && verticalIndex >= startLine)
                {
                    place.Current.MOVER = true;
                }
            });
        }
        
        public void MarkMovers(int x, int y)
        {
            Call<Place>(places, (place, horizontalIndex, verticalIndex) =>
            {
        
                if (place.Current && !place.Current.Connected && horizontalIndex == x && verticalIndex >= y)
                {
                    place.Current.MOVER = true;
                }
            });
        }

        public int ConsumeBullet(int splitCount)
        {
            int totalAmmo = 0;
            CallRow<Place>(places, 0, (place, horizontalIndex) =>
            {
                if (splitCount > 0 && place.Current && !place.Current.MOVER && place.Current.UsageType.Equals(Pawn.Usage.Shooter))
                {
                    Pawn currentPawn = place.Current;
                    int ammo = 1;
                    currentPawn.Amount -= ammo;
                    if (currentPawn.Amount > 0)
                    {
                        currentPawn.PunchScaleBullet(-0.2f);
                    }
                    else
                    {
                        place.Current = null;
                        
                        currentPawn.Hide(currentPawn.Deconstruct);
                        
                        MarkMovers(place.index.x, place.index.y);
                    }
                
                    totalAmmo += ammo;
                    splitCount--;
                }
            });

            return totalAmmo;
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
        
        public void MarkSupplier(int lastIndex)
        {
            Call<Place>(places, (place, horizontalIndex, verticalIndex) =>
            {
                int linearIndex = verticalIndex * Size.x + horizontalIndex;
                place.Supplier = linearIndex < lastIndex;
            });
        }
        
        public void Place(Block block)
        {
            block.PlacedOnGrid = true;
            List<Pawn> temporary = new List<Pawn>(block.Pawns);
            foreach (Pawn pawn in temporary)
            {
                Pawn tempPawn = pawn;
                Place place = GetPlace(tempPawn);
                tempPawn.MOVER = true;
                tempPawn.BUSY = true;

                tempPawn.Tick = Board.THIS._tick;
                place.Accept(tempPawn, 0.1f, () =>
                {
                    tempPawn.BUSY = false;
                    place.Current.Check(place);
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
            Vector3 posDif = position - transform.position + indexOffset;
            Vector2 posFin = new Vector2(posDif.x, -posDif.z);
            Vector2Int? index = null;
            if (posFin.x >= 0.0f && posFin.x < Size.x && posFin.y >= 0.0f && posFin.y < Size.y)
            {
                index = new Vector2Int((int)posFin.x, (int)posFin.y);
            }
            return index;
        }
        private (Place, bool) Project(Pawn pawn)
        {
            Vector2Int? index = Pos2Index(pawn.transform.position);
            if (index == null)
            {
                return (null, false);
            }
            Vector2Int ind = (Vector2Int)index;
            Place place = GetPlace(ind);
            
            if (Size.y - pawn.ParentBlock.width > ind.y && !pawn.CanPlaceAnywhere)
            {
                return (place, false);
            }
            if (place.Occupied)
            {
                return (place, false);
            }
            if (HasForwardPawnAtColumn(ind) && !pawn.CanPlaceAnywhere)
            {
                return (place, false);
            }
            return (place, true);
        }
        public Place GetForwardPlace(Place place)
        {
            Place forwardPlace = null;

            Vector2Int index = place.index;
            index.y--;
            if (index.y >= 0)
            {
                forwardPlace = GetPlace(index);
            }
            return forwardPlace;
        }
        public void HighlightPawnOnGrid(Block block)
        {
            foreach (var pawn in block.Pawns)
            {
                (Place place, bool canPlace) = Project(pawn);
                if (place != null)
                {
                    place.SetPlaceType(canPlace ? Game.Place.PlaceType.FREE : Game.Place.PlaceType.OCCUPIED);
                }
            }
        }
        private bool CanPlacePawnOnGrid(Pawn pawn)
        {
            (Place place, bool canPlace) = Project(pawn);
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
                if (!CanPlacePawnOnGrid(pawn))
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

                Particle.Green_Zone.Emit(1, place.transform.position, Quaternion.Euler(90.0f, 0.0f, 0.0f));
            });
        }
        
        
        [System.Serializable]
        public class Data : ICloneable
        {
            [SerializeField] public int defaultStack = 6;
            [SerializeField] public int maxStack = 6;
            [SerializeField] public int defaultSupplyLine = 6;
            [SerializeField] public int supplyLine = 6;
            [SerializeField] public int pawnSat = 75;
            
            public Data()
            {
                
            }
            public Data(Data data)
            {
                this.defaultStack = data.defaultStack;
                this.maxStack = data.maxStack;
                this.defaultSupplyLine = data.defaultSupplyLine;
                this.supplyLine = data.supplyLine;
                this.pawnSat = data.pawnSat;
            }

            public object Clone()
            {
                return new Data(this);
            }
        }
        
    }
}