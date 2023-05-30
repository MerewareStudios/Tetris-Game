using DG.Tweening;
using Internal.Core;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using static UnityEngine.Rendering.HableCurve;


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
                    place.Current.CheckSteady(place);
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

                totalLevel += place.Current.level;

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

            Pawn pawn = Spawner.THIS.SpawnPawn(null, spawnPlace.transform.position, totalLevel);
            pawn.MarkSteadyColor();
            spawnPlace.AcceptImmidiate(pawn);


            pawn.transform.DOKill();
            pawn.transform.localScale = Vector3.zero;
            pawn.transform.DOScale(Vector3.one, 0.2f).SetDelay(duration).SetEase(Ease.OutBack, 2.0f);
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
                    //place.Current.MarkMoverColor();
                }
            });
        }
    }
}