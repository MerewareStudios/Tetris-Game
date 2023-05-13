using DG.Tweening;
using Internal.Core;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;


namespace Game
{
    public class Grid : MonoBehaviour
    {
        [SerializeField] private Transform nodeParent;
        [System.NonSerialized] private Pawn[,] pawns;
        [System.NonSerialized] public Vector3 offset;
        public delegate bool CheckIndexFunction(int x, int y);

        public void Generate(int width, int height, float space)
        {
            pawns = new Pawn[width, height];

            for (int w = 0; w < width; w++)
            {
                for (int h = 0; h < height; h++)
                {
                    GameObject gridNode = Pool.Grid_Node.Spawn(this.nodeParent);
                    Transform nodeTransform = gridNode.transform;

                    nodeTransform.localPosition = new Vector3(w * space, 0.0f, h * space);
                }
            }

            offset = new Vector3((width - 1) * space * -0.5f, 0.0f, 0.0f);
            transform.localPosition = offset;
        }

        
       
        private void CheckRows()
        {
            for (int h = 0; h < pawns.GetLength(1); h++)
            {
                bool lineFull = true;
                for (int w = 0; w < pawns.GetLength(0); w++)
                {
                    if (pawns[w, h] == null)
                    {
                        lineFull = false;
                        break;
                    }
                }
                if (lineFull)
                {
                    StartCoroutine(Tetris(h));
                    return;
                }
            }
        }

        public void Submit(Block block)
        {
            foreach (var pawn in block.pawns)
            {
                Vector2Int index = Pos2Index(pawn.transform.position);

                pawns[index.x, index.y] = pawn;
            }
            block.Deconstruct(this.transform);
            CheckRows();
        }
        private IEnumerator Tetris(int lineIndex)
        {
            for (int w = 0; w < pawns.GetLength(0); w++)
            {
                Pawn pawn = pawns[w, lineIndex];
                pawn.Light();

                int endLine = FindFurthestStoppingPoint(w, lineIndex);


                pawns[w, lineIndex] = null;
                Pawn stationaryPawn = pawns[w, endLine];
                //pawns[w, endLine] = pawn;

                Vector3 targetPos = pawn.transform.position + Vector3.forward * (endLine - lineIndex);

                pawn.transform.DOMove(targetPos, 0.25f).SetEase(Ease.Linear)
                    .onComplete = () =>
                        {
                            Pawn newPawn = Merge(stationaryPawn, pawn);
                            //pawns[w, endLine] = newPawn;
                        };
            }
            yield return new WaitForSeconds(0.5f);

            Fall(lineIndex);
        }

        private Pawn Merge(Pawn stationaryPawn, Pawn movingPawn)
        {
            if (stationaryPawn == null)
            {
                return movingPawn;

            }
            int mergeLevel = stationaryPawn.level + movingPawn.level;

            //stationaryPawn.Despawn();
            movingPawn.Despawn();

            Pool pawnType = GameManager.THIS.Constants.pawns[mergeLevel - 1];
            return pawnType.Spawn<Pawn>();
        }


        private void PrintPawns()
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int h = 0; h < pawns.GetLength(1); h++)
            {
                for (int w = 0; w < pawns.GetLength(0); w++)
                {
                    stringBuilder.Append(pawns[w, h] == null ? "e " : "p ");
                }
                stringBuilder.Append("\n");
            }
            stringBuilder.Log();
        }

        private int FindFurthestStoppingPoint(int w, int y)
        {
            int startLine = y + 1;
            for (int i = startLine; i < pawns.GetLength(1); i++)
            {
                if (pawns[w, i] != null)
                {
                    return i;
                }
            }
            return pawns.GetLength(1) - 1;
        }
        private void Fall(int startLine)
        {
            //PrintPawns();

            int startIndex = startLine - 1;

            for (int i = startIndex; i >= 0; i--)
            {
                for (int w = 0; w < pawns.GetLength(0); w++)
                {
                    if (pawns[w, i] != null)
                    {
                        Pawn pawn = pawns[w, i];
                        pawns[w, i] = null;

                        int endIndex = i + 1;

                        Vector3 targetPos = pawn.transform.position + Vector3.forward;

                        pawn.transform.DOMove(targetPos, 0.25f).SetEase(Ease.Linear)
                        .onComplete = () =>
                        {

                        };

                        pawns[w, endIndex] = pawn;
                    }

                }
            }
        }

      
        public bool CheckMotion(CheckIndexFunction rule, Block block)
        {
            foreach (var pawn in block.pawns)
            {
                Vector2Int index = Pos2Index(pawn.transform.position);

                if (!rule.Invoke(index.x, index.y))
                {
                    return false;
                }
            }
            return true;
        }

        private Vector2Int Pos2Index(Vector3 position)
        {
            Vector3 gridPos = position - offset;
            Vector2Int index = new Vector2Int((int)gridPos.x, (int)gridPos.z);
            return index;
        }

        public bool CanMoveForward(int x, int y)
        {
            if (y >= pawns.GetLength(1)-1)
            {
                return false;
            }
            if (y < 0)
            {
                return true;
            }
            if (pawns[x, y + 1] != null)
            {
                return false;
            }
            return true;
        }
        public bool CanMoveLeft(int x, int y)
        {
            if (x <= 0)
            {
                return false;
            }
            if (y < 0)
            {
                return true;
            }
            if (pawns[x - 1, y] != null)
            {
                return false;
            }
            return true;
        }
        public bool CanMoveRight(int x, int y)
        {
            if (x >= pawns.GetLength(0) - 1)
            {
                return false;
            }
            if (y < 0)
            {
                return true;
            }
            if (pawns[x + 1, y] != null)
            {
                return false;
            }
            return true;
        }
    }
}