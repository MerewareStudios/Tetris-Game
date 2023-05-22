using DG.Tweening;
using Game;
using System.Collections.Generic;
using UnityEngine;

namespace Internal.Core
{
    [CreateAssetMenu(fileName = "Game Constants", menuName = "Game/Constants", order = 0)]
    public class Constants : ScriptableObject
    {
        [Header("Pawn Settings")]
        [SerializeField] public List<Pool> pawns;
        [Header("Block Timer Settings")]
        [SerializeField] public float forwardDelay;
        [SerializeField] public float afterMovementDelay;
        [SerializeField] public float forwardMovementDuration;
        [SerializeField] public float sideMovementDuration;
        [Header("Tetris Timer Settings")]
        [SerializeField] public float afterTetrisForwardDelay;
        [SerializeField] public float afterTetrisFallDelay;
        [SerializeField] public float afterWarFallDelay;
        [SerializeField] public float afterWarFightDelay;
    }
}