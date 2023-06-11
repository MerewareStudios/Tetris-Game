using DG.Tweening;
using Internal.Core;
using System.Collections.Generic;
using UnityEngine;


namespace Game
{
    public class Grid : MonoBehaviour
    {
        [SerializeField] public Vector2Int Size;
        
        [System.NonSerialized] private Place[,] places;
        [System.NonSerialized] private int _tickIndex = 0;
        [System.NonSerialized] private bool[] _frontBlockers;

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
            Map.THIS.grid.MarkMerger(0);
            
            _frontBlockers = new bool[Size.x];
            _frontBlockers.Fill(false);
        }
        public void Deconstruct()
        {
            Call<Place>(places, (place) =>
            {
                place.Deconstruct();
            });
        }
        public bool IsFrontFree(int frontIndex)
        {
            return _frontBlockers[frontIndex];
        }
        public void SetFrontFree(int frontIndex, bool state)
        {
            _frontBlockers[frontIndex] = state;
        }
        public void SetAllFrontFree(bool state)
        {
            for (int i = 0; i < _frontBlockers.Length; i++)
            {
                _frontBlockers[i] = state;
            }
        }
        public void Move(float moveDuration)
        {
            _tickIndex++;

            Call<Place>(places, (place) =>
            {
                if (place.Current)
                {
                    place.Current.MoveForward(place, _tickIndex, moveDuration);
                }
            });
        }
        public void CheckSteady()
        {
            Call<Place>(places, (place) =>
            {
                if (place.Current)
                {
                    place.Current.CheckSteady(place, true);
                }
            });
        }
        public void Dehighlight()
        {
            Call<Place>(places, (place) => 
                {
                    place.SetColor(Place.PlaceType.EMPTY);
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
        public Place GetPlace(int x, int y) => places[x, y];
        public Place GetPlace(Vector2Int index) => places[index.x, index.y];

        public List<int> CheckTetris()
        {
            List<int> tetrisLines = new();
            for (int j = 0; j < Size.y; j++)
            {
                bool tetris = true;
                for (int i = 0; i < Size.x; i++)
                {
                    if(!places[i, j].Occupied || places[i, j].Current.Connected || places[i, j].Current.MoveUntilForward)
                    {
                        tetris = false;
                        break;
                    }
                }
                if (tetris)
                {
                    tetrisLines.Add(j);
                }
            }
            return tetrisLines;
        }

        public int MergeLine(int lineIndex, float duration, int multiplier)
        {
            List<int> indexes = new();
            List<Pawn> pawns = new();
            int highestTick = -1;

            int totalLevel = 0;
            

            for (int i = 0; i < Size.x; i++)
            {
                Place place = places[i, lineIndex];
                
                
                pawns.Add(place.Current);

                

                int additive = place.Current.Level;
                if (additive == 1)
                {
                    additive *= multiplier;
                }
                totalLevel += additive;

                if (place.Current.MovedAtTick == highestTick)
                {
                    indexes.Add(i);
                }
                else if(place.Current.MovedAtTick > highestTick)
                {
                    highestTick = place.Current.MovedAtTick;
                    indexes.Clear();
                    indexes.Add(i);
                }
                place.Current = null;
            }

            Place spawnPlace = places[indexes.Random(), lineIndex];
            foreach (var pawn in pawns)
            {
                Color color = multiplier == 1 ? Const.THIS.singleColor : Const.THIS.comboColor;
                Particle.Square.Emit(1, color, pawn.transform.position, rotation: Quaternion.Euler(90.0f, 0.0f, 0.0f));
                
                pawn.transform.DOMove(spawnPlace.segmentParent.position, duration).SetDelay(0.15f)
                    .onComplete += () =>
                    {
                        pawn.Deconstruct();
                    };
            }
            
           
            Pawn newPawn = Spawner.THIS.SpawnPawn(null, spawnPlace.transform.position, totalLevel);
            newPawn.MarkMergeColor();
            spawnPlace.AcceptImmidiate(newPawn);

            newPawn.AnimatedShow(0.6f, () => newPawn.CanShoot = true);
            newPawn.Merger = true;
            
            UIManager.THIS.ft_TF2.FlyWorld("+" + totalLevel, newPawn.transform.position + new Vector3(-0.1f, 0.2f, 0.0f), 0.3f);
            Particle.Portal_Blue.Play(newPawn.transform.position + Vector3.up * 0.05f,
                Quaternion.Euler(90.0f, 0.0f, 0.0f), Vector3.one);


            return totalLevel;
        }

        public void MergeLines(List<int> lines, float duration)
        {
            int[] points = new int[lines.Count];
            for (int i = 0; i < lines.Count; i++)
            {
                points[i] = MergeLine(lines[i], duration, lines.Count);
            }

            int totalPoint = 0;           

            foreach (var point in points)
            {
                totalPoint += point;
            }

            if (totalPoint > 0)
            {
                int addition = totalPoint;
                DOVirtual.DelayedCall(0.3f, () =>
                {
                    ScoreBoard.THIS.Score += addition;
                });
            }
        }

        public void MarkNewMovers(int startLine)
        {
            SetAllFrontFree(true);

            Call<Place>(places, (place, horizonalIndex, verticalIndex) =>
            {

                if (place.Current && !place.Current.Connected && verticalIndex >= startLine)
                {
                    place.Current.MoveUntilForward = true;
                }
            });
        }
        
        public void MarkNewMovers(int x, int y)
        {
            _frontBlockers[x] = true;

            Call<Place>(places, (place, horizontalIndex, verticalIndex) =>
            {

                if (place.Current && !place.Current.Connected && horizontalIndex == x && verticalIndex >= y)
                {
                    place.Current.UpcomingMover = true;
                    // UnityEditor.EditorApplication.isPaused = true;
                }
            });
        }

        public void GiveBullet()
        {
            int totalAmmo = 0;
            CallRow<Place>(places, 0, (place, horizontalIndex) =>
            {
                if (place.Current && place.Current.CanShoot && place.Current.Merger)
                {
                    Pawn currentPawn = place.Current;
                    int ammo = 1;
                    currentPawn.Level -= ammo;
                    if (currentPawn.Level > 0)
                    {
                        currentPawn.PunchScale(-0.2f);
                    }
                    else
                    {
                        place.Current = null;
                        
                        currentPawn.Hide(currentPawn.Despawn);
                        
                        MarkNewMovers(place.index.x, place.index.y);
                    }

                    totalAmmo += ammo;
                }
            });

            if (totalAmmo > 0)
            {
                Warzone.THIS.PlayerAttack(totalAmmo);
            }
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
        
        public void MarkMerger(int index)
        {
            Call<Place>(places, (place, horizonalIndex, verticalIndex) =>
            {
                place.Merger = (verticalIndex == index);
            });
        }
    }
}