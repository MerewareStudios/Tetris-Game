using DG.Tweening;
using Internal.Core;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;


namespace Game
{
    public class Grid : MonoBehaviour
    {
        [System.NonSerialized] public Place[,] places;
        [System.NonSerialized] public Dictionary<Transform, Place> placeDic = new();
        [System.NonSerialized] public int TickIndex = 0;
        [SerializeField] public Vector2Int size;
        [System.NonSerialized] public bool[] frontBlockers;

        public void Construct()
        {
            places = new Place[size.x, size.y];
            for (int i = 0; i < size.x; i++)
            {
                for(int j = 0; j < size.y; j++)
                {
                    Place place = Pool.Place.Spawn<Place>(this.transform);
                    place.transform.localPosition = new Vector3(i, 0.0f, -j);
                    places[i, j] = place;
                    place.index = new Vector2Int(i, j);
                    placeDic.Add(place.transform, place);
                }
            }
            frontBlockers = new bool[size.x];
            frontBlockers.Fill(false);
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
            return frontBlockers[frontIndex];
        }
        public void SetFrontFree(int frontIndex, bool state)
        {
            frontBlockers[frontIndex] = state;
        }
        public void SetAllFrontFree(bool state)
        {
            for (int i = 0; i < frontBlockers.Length; i++)
            {
                frontBlockers[i] = state;
            }
        }
        public void Move(float moveDuration)
        {
            TickIndex++;

            Call<Place>(places, (place) =>
            {
                if (place.Current)
                {
                    place.Current.MoveForward(place, TickIndex, moveDuration);
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
                    place.MarkDefault();
                });
        }
        private void Call<T>(T[,] array, System.Action<T> action)
        {
            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.y; j++)
                {
                    action.Invoke(array[i, j]);
                }
            }
        }
        private void Call<T>(T[,] array, System.Action<T, int, int> action)
        {
            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.y; j++)
                {
                    action.Invoke(array[i, j], i, j);
                }
            }
        }
        private void CallRow<T>(T[,] array, int lineIndex, System.Action<T, int> action)
        {
            for (int i = 0; i < size.x; i++)
            {
                action.Invoke(array[i, lineIndex], i);
            }
        } 
        private void CallColumn<T>(T[,] array, int columnIndex, System.Action<T, int> action)
        {
            for (int j = 0; j < size.y; j++)
            {
                action.Invoke(array[columnIndex, j], j);
            }
        }
        public Place GetPlace(int x, int y) => places[x, y];
        public Place GetPlace(Vector2Int index) => places[index.x, index.y];

        public List<int> CheckTetris()
        {
            List<int> tetrisLines = new();
            for (int j = 0; j < size.y; j++)
            {
                bool tetris = true;
                for (int i = 0; i < size.x; i++)
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

        public void MergeLine(int lineIndex, float duration)
        {
            List<int> indexes = new();
            List<Pawn> segments = new();
            int highestTick = -1;

            int totalLevel = 0;
            

            for (int i = 0; i < size.x; i++)
            {
                Place place = places[i, lineIndex];
                segments.Add(place.Current);

                totalLevel += place.Current.Level;

                if (place.Current.movedAtTick == highestTick)
                {
                    indexes.Add(i);
                }
                else if(place.Current.movedAtTick > highestTick)
                {
                    highestTick = place.Current.movedAtTick;
                    indexes.Clear();
                    indexes.Add(i);
                }
                place.Current = null;
            }

            Place spawnPlace = places[indexes.Random(), lineIndex];
            foreach (var segment in segments)
            {
                segment.transform.DOMove(spawnPlace.segmentParent.position, duration).SetDelay(0.0f)
                    .onComplete += () =>
                    {
                        segment.Despawn();
                    };
            }
           
            Pawn newPawn = Spawner.THIS.SpawnPawn(null, spawnPlace.transform.position, totalLevel);
            newPawn.MarkSteadyColor();
            spawnPlace.AcceptImmidiate(newPawn);

            newPawn.AnimatedShow(duration, () => newPawn.CanShoot = true);
        }

        public void MergeLines(List<int> lines, float duration)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                MergeLine(lines[i], duration);
            }
        }

        public void MarkNewMovers(int startLine, int freeForwardCount)
        {
            SetAllFrontFree(true);

            Map.THIS.FreeMoveIndex = startLine;
            Call<Place>(places, (place, horizonalIndex, verticalIndex) =>
            {

                if (place.Current != null && !place.Current.Connected && verticalIndex >= startLine)
                {
                    place.Current.MoveUntilForward = true;
                }
            });
        }

        public void Shoot()
        {
            Pawn enemyPawn = Map.THIS.line.pawnBig;
            int totalDamageTaken = 0;
            CallRow<Place>(places, 0, (place, verticalIndex) =>
            {
                if (enemyPawn.Level > 0 && place.Current != null && place.Current.CanShoot && place.Current.Level > 1)
                {
                    Pawn currentPawn = place.Current;
                    if (enemyPawn.Level > 0)
                    {
                        int damage = 1;
                        currentPawn.Level -= damage;
                        currentPawn.PunchScale(-0.2f);
                       
                        enemyPawn.Level-=damage;
                        enemyPawn.PunchScale(-0.2f);
                        
                        totalDamageTaken += damage;
                    }
                }
            });
            if (enemyPawn.Level <= 0)
            {
                LevelManager.THIS.OnWictory();
            }

            if (totalDamageTaken == 0)
            {
                enemyPawn.Level++;
                enemyPawn.PunchScale(0.2f);
            }
            
        }
        public bool HasForwardPawnAtColumn(Vector2Int index)
        {
            
            for (int j = 0; j < size.y; j++)
            {
                Place place = places[index.x, j];
                if (index.y <= j && place.Current != null)
                {
                    return true;
                }
            }
            return false;
        }
        public void HighlightPrediction(Place place)
        {
            // Place prevPlace = null;
            for (int j = size.y-1; j >= 0; j--)
            {
                Place checkPlace = places[place.index.x, j];
                if (!checkPlace.Occupied)
                {
                    checkPlace.MarkFree();
                    // prevPlace = checkPlace;
                }
                else
                {
                    break;
                }
            }

            // if (prevPlace != null)
            // {
            //     prevPlace.MarkFree();
            // }
        }
    }
}