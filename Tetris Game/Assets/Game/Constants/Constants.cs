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
        [Header("Level")]
        public int enemyStartingHealth = 0;
        public int enemyFinishingHealth = 0;
        [Range(-1, 1)] public int gainSign;
        public float enemyHealthGainInterval;
        public int enemyHealthAdditionPerInterval;
        [Header("Combo")]
        public Color singleColor;
        public Color comboColor;
    }
}