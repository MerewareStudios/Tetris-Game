using DG.Tweening;
using Internal.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Map : Singleton<Map>
    {
        [System.NonSerialized] private Coroutine _mainRoutine = null;
        // [System.NonSerialized] private readonly List<int> _requestedTetrisLines = new();

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
                    // MergeRequestedLines(tetrisLines);
                    if (tetrisLines.Count > 0)
                    {
                        if (tetrisLines.Count > 1)
                        {
                            UIManager.THIS.ft_Combo.FlyScreen("x" + tetrisLines.Count, Vector3.zero, 0.0f);
                            yield return new WaitForSeconds(0.4f);
                        }

                        Board.THIS.MergeLines(tetrisLines, 0.2f);
                    
                        Board.THIS.MarkAllMover(tetrisLines[0]);
                        Board.THIS.CheckAll();
                        yield return new WaitForSeconds(0.75f);
                    }
                    yield return new WaitForSeconds(0.175f);
                }
            }
        }

        // private void AddRequestedMerge(int lineIndex)
        // {
        //     if (_requestedTetrisLines.Contains(lineIndex)) return;
        //     _requestedTetrisLines.Add(lineIndex);
        // }
        //
        // private void Update()
        // {
        //     if (Input.GetKeyDown(KeyCode.Q))
        //     {
        //         AddRequestedMerge(0);
        //     }
        // }
        //
        // private void MergeRequestedLines(List<int> tetrisLines)
        // {
        //     if (_requestedTetrisLines.Count == 0) return;
        //
        //     foreach (var line in _requestedTetrisLines)
        //     {
        //         if (!tetrisLines.Contains(line))
        //         {
        //             tetrisLines.Add(line);
        //         }
        //     }            
        //     _requestedTetrisLines.Clear();
        // }

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
