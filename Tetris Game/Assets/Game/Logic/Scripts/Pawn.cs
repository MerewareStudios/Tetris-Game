using Internal.Core;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Game
{
    public class Pawn : MonoBehaviour
    {
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] public int level = 1;
        [SerializeField] public TextMeshPro levelText;

        void Awake()
        {
            levelText.text = level.ToString();    
        }

        public void Mark()
        {
            meshRenderer.SetColor(Map.THIS.MPB_PAWN, "_BaseColor", Color.gray, 0);
        }
        public void Light()
        {
            meshRenderer.SetColor(Map.THIS.MPB_PAWN, "_BaseColor", Color.green, 0);
        }
    }
}
