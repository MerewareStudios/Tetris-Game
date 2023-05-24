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
        [Header("Colors")]
        public Color placeColorHighlight;
        public Color placeColorDeny;
        public Color placeColorDefault;
        [Header("Layers")]
        public LayerMask segmentPlaceLayer;
        [Header("Animations")]
        public Ease segmentAcceptEase = Ease.OutSine;
        public float segmentAcceptDuration = 0.35f;
        public Ease segmentDenyEase = Ease.OutSine;
        public float segmentDenyDuration = 0.35f;
    }
}