using DG.Tweening;
using Internal.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game
{
    public class Map : Singleton<Map>
    {
        [System.NonSerialized] private Coroutine _mainRoutine = null;
        [System.NonSerialized] private Queue<int> _queuedMergeLines = new();

        public void StartMainLoop()
        {
            _mainRoutine = StartCoroutine(MainLoop());

            IEnumerator MainLoop()
            {
                while (true)
                {
                    Board.THIS.MoveAll(0.25f);
                    yield return new WaitForSeconds(0.3f);
                    Board.THIS.CheckAll();
                    List<int> tetrisLines = Board.THIS.CheckTetris();
                    
                    while (_queuedMergeLines.Count > 0)
                    {
                        int queuedIndex = _queuedMergeLines.Dequeue();
                        if (!tetrisLines.Contains(queuedIndex))
                        {
                            tetrisLines.Add(queuedIndex);
                        }
                    }
                    
                    if (tetrisLines.Count > 0)
                    {
                        // if (tetrisLines.Count > 1)
                        // {
                            // Combo text emission
                            // UIManagerExtensions.EmitComboText(tetrisLines.Count);
                            //UIManager.THIS.ft_Combo.FlyScreen("<size=125%>x" + tetrisLines.Count + "<size=100%>\nMAX", Vector3.zero,  0.0f);
                            // yield return new WaitForSeconds(0.25f);
                        // }

                      

                        Board.THIS.MergeLines(tetrisLines);
                    
                        Board.THIS.MarkAllMover(tetrisLines[0]);
                        Board.THIS.CheckAll();
                        yield return new WaitForSeconds(0.75f);
                    }
                    yield return new WaitForSeconds(0.175f);
                    Board.THIS.CheckDeadLock();
                }
            }
        }

        public void ForceMerge(int line)
        {
            _queuedMergeLines.Enqueue(line);
        }

        public void Deconstruct()
        {
            if (_mainRoutine != null)
            {
                StopCoroutine(_mainRoutine);
                _mainRoutine = null;
            }
        }
    }
}
