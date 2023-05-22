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

        
       
        private IEnumerator CheckRows()
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
                    if (h == pawns.GetLength(1) - 1)
                    {
                        yield return StartCoroutine(GoToWar(h));
                    }
                    else
                    {
                        yield return StartCoroutine(Tetris(h));
                    }
                    yield break;
                }
            }

            yield return new WaitForEndOfFrame();

            Map.THIS.SpawnRandomBlock();
        }

        public void Submit(Block block)
        {
            foreach (var pawn in block.pawns)
            {
                Vector2Int index = Pos2Index(pawn.transform.position);

                pawns[index.x, index.y] = pawn;
            }
            block.Deconstruct(this.transform);
            Map.THIS.currentBlock = null;
            StartCoroutine(CheckRows());
        }
        private IEnumerator Tetris(int lineIndex)
        {
            for (int w = 0; w < pawns.GetLength(0); w++)
            {
                int column = w;
                Pawn movingPawn = pawns[column, lineIndex];
                movingPawn.Tetris();

                int endLine = FindFurthestStoppingPoint(w, lineIndex);


                pawns[column, lineIndex] = null;
                Pawn stationaryPawn = pawns[column, endLine];

                Vector3 targetPos = movingPawn.transform.position + Vector3.forward * (endLine - lineIndex);

                movingPawn.transform.DOMove(targetPos, 0.25f).SetEase(Ease.Linear).SetDelay(GameManager.THIS.Constants.afterTetrisForwardDelay)
                    .onComplete = () =>
                        {
                            Pawn newPawn = Merge(stationaryPawn, movingPawn);
                            pawns[column, endLine] = newPawn;
                        };
            }
            yield return new WaitForSeconds(GameManager.THIS.Constants.afterTetrisFallDelay + GameManager.THIS.Constants.afterTetrisForwardDelay);

            Fall(lineIndex);
        }
        private IEnumerator GoToWar(int lineIndex)
        {
            for (int w = 0; w < pawns.GetLength(0); w++)
            {
                int column = w;
                Pawn fighterPawn = pawns[column, lineIndex];
                pawns[column, lineIndex] = null;
                fighterPawn.Fight();

                Vector3 targetPos = fighterPawn.transform.position + Vector3.forward;

                fighterPawn.transform.DOMove(targetPos, 0.25f).SetEase(Ease.Linear).SetDelay(GameManager.THIS.Constants.afterWarFightDelay)
                    .onComplete = () =>
                    {

                    };
            }

            yield return new WaitForSeconds(GameManager.THIS.Constants.afterWarFallDelay + GameManager.THIS.Constants.afterWarFightDelay);

            Fall(lineIndex);
        }
        private Pawn Merge(Pawn stationaryPawn, Pawn movingPawn)
        {
            if (stationaryPawn == null)
            {
                return movingPawn;

            }
            int mergeLevel = stationaryPawn.level + movingPawn.level;

            Pool pawnType = GameManager.THIS.Constants.pawns[mergeLevel - 1];
            Pawn newPawn = pawnType.Spawn<Pawn>(this.transform);
            newPawn.transform.position = stationaryPawn.transform.position;

            stationaryPawn.Despawn();
            movingPawn.Despawn();

            return newPawn;
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

            StartCoroutine(CheckRows());
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