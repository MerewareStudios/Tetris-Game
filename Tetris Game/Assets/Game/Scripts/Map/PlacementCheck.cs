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

            List<Place> places = new();
            
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
                        totalHorShiftEnd = boardSize.x - block.blockData.NormalWidth + 1;
                        
                        
                        totalVertShiftStart = block.blockData.NormalHeight - 1 + (boardSize.y - block.blockData.FitHeight);
                        totalVertShiftEnd = boardSize.y;
                        break;
                    case 90:
                        zeroShift = new Vector3(1.5f, 0.0f, -1.0f);
                        
                        
                        totalHorShiftStart = 0;
                        totalHorShiftEnd = boardSize.x - block.blockData.NormalHeight + 1;
                        
                        
                        totalVertShiftStart = boardSize.y - block.blockData.FitHeight;
                        totalVertShiftEnd = boardSize.y - block.blockData.NormalWidth + 1;
                        break;
                    case 180:
                        zeroShift = new Vector3(-1.0f, 0.0f, -1.5f);
                        
                        
                        totalHorShiftStart = block.blockData.NormalWidth - 1;
                        totalHorShiftEnd = boardSize.x;
                        
                        
                        totalVertShiftStart = boardSize.y - block.blockData.FitHeight;
                        totalVertShiftEnd = boardSize.y - block.blockData.NormalHeight + 1;
                        break;
                    case 270:
                        zeroShift = new Vector3(-1.5f, 0.0f, 1.0f);
                        
                        
                        totalHorShiftStart = block.blockData.NormalHeight - 1;
                        totalHorShiftEnd = boardSize.x;
                        
                        
                        totalVertShiftStart = block.blockData.NormalWidth - 1 + boardSize.y - block.blockData.FitHeight;
                        totalVertShiftEnd = boardSize.y;
                        break;
                }

                

                for (int j = totalVertShiftStart; j < totalVertShiftEnd; j++)
                {
                    for (int i = totalHorShiftStart; i < totalHorShiftEnd; i++)
                    {
                        bool found = true;
                        Vector3 finalPos = Vector3.zero;

                        places.Clear();
                        
                        foreach (var localPawnPosition in localPawnPositions)
                        {
                            Vector3 rotatedPosition = localPawnPosition.RotatePointAroundPivot(Vector3.zero, Quaternion.Euler(0.0f, angle, 0.0f));

                            Vector3 shift = zeroShift + new Vector3(i, 0.0f, -j);

                            finalPos = boardPosition + shift + rotatedPosition;

                            Place place = Board.THIS.IsEmpty(finalPos);
                            
                            if (!place)
                            {
                                found = false;
                                break;
                            }
                            
                            places.Add(place);
                        }

                        if (found)
                        {
                            // foreach (var p in foundPos)
                            // {
                            //     Debug.DrawLine(Spawner.THIS.transform.position + Vector3.up * 3.0f, p + Vector3.up * 3.0f, Color.white, 0.1f);
                            // }
                            
                            Board.THIS.Highlight(places);

                            yield return new WaitForSeconds(1.5f);
                            // return true;
                        }
                    }
                }
            }
        }
        return false;
    }
}
