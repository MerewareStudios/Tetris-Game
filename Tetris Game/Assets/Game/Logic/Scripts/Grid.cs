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

            Call<Place>(places, (place) =>
            {
                if (place.currentSegment != null)
                {
                    place.currentSegment.UpdateParentBlockStats();
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
        public List<int> CheckTetris()
        {
            List<int> tetrisLines = new();
            for (int j = 0; j < size.y; j++)
            {
                bool tetris = true;
                for (int i = 0; i < size.x; i++)
                {
                    if(!places[i, j].Occupied)
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

        public void ClearLine(int lineIndex)
        {
            for (int i = 0; i < size.x; i++)
            {
                Place place = places[i, lineIndex];
                place.Deconstruct(true);
            }
        }

        public void ClearLines(List<int> lines)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                ClearLine(lines[i]);
            }
        }

        public void MoveFromLine(int startLine)
        {

        }
    }
}