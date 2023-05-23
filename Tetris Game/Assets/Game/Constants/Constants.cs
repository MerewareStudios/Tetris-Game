using DG.Tweening;
using Game;
using System.Collections.Generic;
using UnityEngine;

namespace Internal.Core
{
    [CreateAssetMenu(fileName = "Game Constants", menuName = "Game/Constants", order = 0)]
    public class Constants : ScriptableObject
    {
        public Color placeColorHighlight;
        public Color placeColorDefault;
        public LayerMask segmentPlaceLayer;
    }
}