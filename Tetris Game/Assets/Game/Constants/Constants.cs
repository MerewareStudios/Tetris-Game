using DG.Tweening;
using Game;
using System.Collections.Generic;
using UnityEngine;

namespace Internal.Core
{
    [CreateAssetMenu(fileName = "Game Constants", menuName = "Game/Constants", order = 0)]
    public class Constants : ScriptableObject
    {
        public float tickInterval = 0.15f;
        public Pool[] blocks;
        [Header("Pawn")] public Color spawnColor;
        public Color moverColor;
        public Color steadyColor;
        public Color enemyColor;
        public Color bigColor;
        [Header("Colors")] public Color placeColorHighlight;
        public Color placeColorDeny;
        public Color placeColorDefault;
        [Header("Spawner")] public float rotationDuration = 0.15f;
        public Ease rotationEase;
    }
}