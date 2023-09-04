using System.Collections.Generic;
using Game;
using UnityEngine;

public static class PlacementCheck
{
    public static bool DetectFit(this Block block)
    {
        Vector3 boardPosition = Board.THIS.transform.position;
        Vector2Int boardSize = Board.THIS.Size;

        List<Vector3> localPawnPositions = block.LocalPawnPositions;
        
        Vector3 zeroShift = new Vector3(-1.0f, 0.0f, -1.5f);

        // 3 4 -1
        // 4 3 0
        // 5 2
        
        int totalHorShift = boardSize.x + 2;
        int totalVertShiftStart = 3;
        int totalVertShiftEnd = totalVertShiftStart + 4 + (block.FitHeight - 4);

        for (int j = totalVertShiftStart; j < totalVertShiftEnd; j++)
        {
            for (int i = 0; i < totalHorShift; i++)
            {
                Color randColor = Random.ColorHSV();

                foreach (var localPawnPosition in localPawnPositions)
                {
                    Vector3 shift = zeroShift + new Vector3(i, 0.0f, -j);

                    Vector3 finalPos = boardPosition + shift + localPawnPosition;

                    bool isEmpty = Board.THIS.IsEmpty(finalPos);
                    
                    if (!isEmpty)
                    {
                        break;
                    }
                    
                    Debug.DrawLine(block.transform.position + localPawnPosition + Vector3.up * 3.0f, finalPos + Vector3.up * 3.0f, randColor, 10.0f);
                }
            }
        }
        return true;
    }
    
}
