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
        [SerializeField] public Ease forwardEase;
        [SerializeField] public float forwardDelay;
        [SerializeField] public float forwardDuration;
        [SerializeField] public float sidewayDuration;
        [SerializeField] public Ease sidewayEase;
    }
}