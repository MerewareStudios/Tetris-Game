using Internal.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Map : Singleton<Map>
    {
        [System.NonSerialized] private Coroutine _mainRoutine = null;
        [System.NonSerialized] private Queue<int> _queuedMergeLines = new();

        public void StartMainLoop()
        {
            StopLoop();
            _mainRoutine = StartCoroutine(MainLoop());

            IEnumerator MainLoop()
            {
                yield return new WaitForSeconds(0.25f);

                while (true)
                {
                    Board.THIS.CheckAll();

                    Board.THIS.MoveAll(0.25f);
                    Spawner.THIS.HighlightCurrentBlock();

                    // yield return new WaitWhile(() => moving);
                    
                    yield return new WaitForSeconds(0.25f);
                    Board.THIS.CheckAll();
                    Spawner.THIS.HighlightCurrentBlock();


                    bool found = true;
                    while (found)
                    {
                        float powerWait;
                        int lineIndex = 0;
                        (found, powerWait, lineIndex) = Board.THIS.UsePowerups();
                        if (lineIndex >= 0)
                        {
                            Board.THIS.MarkAllMover(lineIndex);
                        }
                        if (found)
                        {
                            Board.THIS.CheckAll();
                            Spawner.THIS.HighlightCurrentBlock();
                            yield return new WaitForSeconds(powerWait);
                        }
                    }

                    
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
                        float mergeWait = Board.THIS.MergeLines(tetrisLines);
                    
                        Board.THIS.MarkAllMover(tetrisLines[0]);
                        Board.THIS.CheckAll();
                        Spawner.THIS.HighlightCurrentBlock();

                        yield return new WaitForSeconds(mergeWait);
                        Spawner.THIS.CheckedMergeAfterMove = true;
                    }
                    yield return new WaitForSeconds(0.25f);
                    Board.THIS.CheckDeadLock();
                }
            }
        }

        private void StopLoop()
        {
            if (_mainRoutine != null)
            {
                StopCoroutine(_mainRoutine);
                _mainRoutine = null;
            }
        }

        public void ForceMerge(int line)
        {
            _queuedMergeLines.Enqueue(line);
        }

        public void Deconstruct()
        {
            StopLoop();
        }
        
        public void OnLevelEnd()
        {
            StopLoop();
        }
    }
}
