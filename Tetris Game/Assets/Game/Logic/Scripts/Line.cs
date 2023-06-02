using DG.Tweening;
using Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Line : MonoBehaviour
    {
        // [System.NonSerialized] private List<Pawn> pawns = new();
        [SerializeField] private Transform pawnParent;
        [SerializeField] public Pawn pawnBig;

        public void Construct(int count, int level)
        {
            pawnBig.Level = level;
            // for (int i = 0; i < count; i++)
            // {
            //     Pawn pawn = Spawner.THIS.SpawnPawn(pawnParent, pawnParent.position + new Vector3(-2.5f + i, 0.0f, 0.0f), 50);
            //     pawn.MarkEnemyColor();
            //     pawns.Add(pawn);
            // }
        }

        public void Deconstruct()
        {
            // foreach (var pawn in pawns)
            // {
            //     pawn.Deconstruct();
            // }
            pawnBig.Deconstruct();
        }
        // public Pawn GetPawn(int index)
        // {
        //     return pawns[index];
        // }
    }
}