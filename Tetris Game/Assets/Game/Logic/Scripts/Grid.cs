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
        [SerializeField] public Line line;
        [SerializeField] public int TickIndex = 0;
        [SerializeField] private Vector2Int size;

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

                    placeDic.Add(place.transform, place);
                }
            }
        }

        public void Tick()
        {
            Debug.LogWarning("Tick");
            TickIndex++;

            Call<Place>(places, (place) =>
            {
                if (place.currentSegment != null)
                {
                    place.currentSegment.UpdateParentBlockStats(TickIndex);
                }
            });
            Call<Place>(places, (place) =>
            {
                if (place.currentSegment != null)
                {
                    place.currentSegment.MoveForward();
                }
            });
        }
        public void Dehighlight()
        {
            Call<Place>(places, (place) => 
                {
                    place.Highlight = false;
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
        public List<int> CheckTetris()
        {
            List<int> tetrisLines = new();
            for (int j = 0; j < size.y; j++)
            {
                bool tetris = true;
                for (int i = 0; i < size.x; i++)
                {
                    if(!places[i, j].Occupied || places[i, j].currentSegment.parentBlock != null)
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

        public void MergeLine(int lineIndex)
        {
            List<int> indexes = new();
            List<Segment> segments = new();
            int highestTick = 1;

            int totalLevel = 0;

            for (int i = 0; i < size.x; i++)
            {
                Place place = places[i, lineIndex];
                segments.Add(place.currentSegment);

                totalLevel += place.currentSegment.level;

                if (place.currentSegment.tick == highestTick)
                {
                    indexes.Add(i);
                }
                else if(place.currentSegment.tick > highestTick)
                {
                    highestTick = place.currentSegment.tick;
                    indexes.Clear();
                    indexes.Add(i);
                }
                place.Disjoint();
            }

            Place spawnPlace = places[indexes.Random(), lineIndex];
            foreach (var segment in segments)
            {
                segment.SetMergeColor();
                segment.transform.DOMove(spawnPlace.segmentParent.position, 0.2f).SetDelay(0.25f)
                    .onComplete += () =>
                    {
                        segment.Despawn();
                    };
            }

            //Segment segmentSpawned = SpawnSegment(spawnPlace, Pool.Segment___Level_2);
            Segment segmentSpawned = SpawnSegment(spawnPlace, totalLevel);

            segmentSpawned.transform.DOKill();
            segmentSpawned.transform.localScale = Vector3.zero;
            segmentSpawned.transform.DOScale(Vector3.one, 0.25f).SetDelay(0.5f).SetEase(Ease.OutBack);
        }

        public void MergeLines(List<int> lines)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                MergeLine(lines[i]);
            }
        }

        public void MoveFromLine(int startLine, int freeForwardCount)
        {
            Call<Place>(places, (place, horizonalIndex, verticalIndex) =>
            {
                if (place.currentSegment != null && place.currentSegment.parentBlock == null && verticalIndex >= startLine)
                {
                    place.currentSegment.AddFreeMove(freeForwardCount);
                }
            });
        }

        private Segment SpawnSegment(Place place, int level)
        {
            Segment segment = Pool.Segment___Level_1.Spawn<Segment>(this.transform);
            segment.SetLevel(level);
            segment.currentPlace = place;
            segment.Mover = true;
            place.AcceptImmidiate(segment);

            return segment;
        }
    }
}