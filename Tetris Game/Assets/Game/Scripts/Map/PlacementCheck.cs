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
            
            Vector3 zeroShift = new Vector3(-1.0f, 0.0f, -1.5f);
            Vector3 zeroShiftRotated = new Vector3(-1.5f, 0.0f, -1.0f);

            List<Vector3> foundPos = new();

            int totalVertShiftStart = 3;
            int totalVertShiftEnd = totalVertShiftStart + 4;

            for (int angle = 0; angle < 360; angle+=90)
            {
                bool rotated = angle % 180 == 90;
                int totalHorShift = boardSize.x + (rotated ? 3 : 2);

                for (int j = totalVertShiftStart; j < totalVertShiftEnd; j++)
                {
                    for (int i = 0; i < totalHorShift; i++)
                    {
                        bool found = true;
                        Vector3 finalPos = Vector3.zero;

                        foundPos.Clear();
                        
                        foreach (var localPawnPosition in localPawnPositions)
                        {
                            Vector3 rotatedPosition = localPawnPosition.RotatePointAroundPivot(Vector3.zero, Quaternion.Euler(0.0f, angle, 0.0f));

                            Vector3 shift = (rotated ? zeroShiftRotated : zeroShift) + new Vector3(i, 0.0f, -j);

                            finalPos = boardPosition + shift + rotatedPosition;

                            bool isEmpty = Board.THIS.IsEmpty(finalPos);
                            
                            if (!isEmpty)
                            {
                                found = false;
                                break;
                            }
                            
                            foundPos.Add(finalPos);
                        }

                        if (found)
                        {
                            Vector3 center = Vector3.zero;
                            foreach (var p in foundPos)
                            {
                                center += p;
                            }

                            center /= foundPos.Count;
                            foreach (var p in foundPos)
                            {
                                Debug.DrawLine(center + Vector3.up * 3.0f, p + Vector3.up * 3.0f, Color.white, 0.5f);
                            }

                            yield return new WaitForSeconds(0.5f);
                            // return true;
                        }
                    }
                }
            }
        }

        
        return false;
    }
    
}
