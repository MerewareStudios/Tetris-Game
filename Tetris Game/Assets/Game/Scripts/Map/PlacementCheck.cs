using System.Collections;
using System.Collections.Generic;
using Game;
using Internal.Core;
using UnityEngine;

public static class PlacementCheck
{
    public static bool DetectFit(this Block block, MonoBehaviour mono)
    {
        mono.StartCoroutine(SearchRoutine());
        
        IEnumerator SearchRoutine()
        {
            Vector3 boardPosition = Board.THIS.transform.position;
            Vector2Int boardSize = Board.THIS.Size;

            List<Vector3> localPawnPositions = block.LocalPawnPositions;
            
            Vector3 zeroShift = Vector3.zero;

            List<Vector3> foundPos = new();

            for (int angle = 0; angle < 360; angle+=90)
            {
                int totalHorShiftStart = 0;
                int totalHorShiftEnd = 0;
                
                int totalVertShiftStart = 0;
                int totalVertShiftEnd = 0;

                switch (angle)
                {
                    case 0:
                        zeroShift = new Vector3(1.0f, 0.0f, 1.5f);
                        
                        
                        totalHorShiftStart = 0;
                        totalHorShiftEnd = boardSize.x - block.NormalWidth + 1;
                        
                        
                        totalVertShiftStart = block.NormalHeight - 1 + (boardSize.y - block.FitHeight);
                        totalVertShiftEnd = boardSize.y;
                        break;
                    case 90:
                        zeroShift = new Vector3(1.5f, 0.0f, -1.0f);
                        
                        
                        totalHorShiftStart = 0;
                        totalHorShiftEnd = boardSize.x - block.NormalHeight + 1;
                        
                        
                        totalVertShiftStart = boardSize.y - block.FitHeight;
                        totalVertShiftEnd = boardSize.y - block.NormalWidth + 1;
                        break;
                    case 180:
                        zeroShift = new Vector3(-1.0f, 0.0f, -1.5f);
                        
                        
                        totalHorShiftStart = block.NormalWidth - 1;
                        totalHorShiftEnd = boardSize.x;
                        
                        
                        totalVertShiftStart = boardSize.y - block.FitHeight;
                        totalVertShiftEnd = boardSize.y - block.NormalHeight + 1;
                        break;
                    case 270:
                        zeroShift = new Vector3(-1.5f, 0.0f, 1.0f);
                        
                        
                        totalHorShiftStart = block.NormalHeight - 1;
                        totalHorShiftEnd = boardSize.x;
                        
                        
                        totalVertShiftStart = block.NormalWidth - 1 + boardSize.y - block.FitHeight;
                        totalVertShiftEnd = boardSize.y;
                        break;
                }

                

                for (int j = totalVertShiftStart; j < totalVertShiftEnd; j++)
                {
                    for (int i = totalHorShiftStart; i < totalHorShiftEnd; i++)
                    {
                        bool found = true;
                        Vector3 finalPos = Vector3.zero;

                        foundPos.Clear();
                        
                        foreach (var localPawnPosition in localPawnPositions)
                        {
                            Vector3 rotatedPosition = localPawnPosition.RotatePointAroundPivot(Vector3.zero, Quaternion.Euler(0.0f, angle, 0.0f));

                            Vector3 shift = zeroShift + new Vector3(i, 0.0f, -j);

                            finalPos = boardPosition + shift + rotatedPosition;

                            bool isEmpty = Board.THIS.IsEmpty(finalPos);
                            
                            // if (!isEmpty)
                            // {
                            //     found = false;
                            //     break;
                            // }
                            
                            foundPos.Add(finalPos);
                        }

                        if (found)
                        {
                            foreach (var p in foundPos)
                            {
                                Debug.DrawLine(Spawner.THIS.transform.position + Vector3.up * 3.0f, p + Vector3.up * 3.0f, Color.white, 0.1f);
                            }

                            yield return new WaitForSeconds(0.1f);
                            // return true;
                        }
                    }
                }
            }
        }
        return false;
    }
}
