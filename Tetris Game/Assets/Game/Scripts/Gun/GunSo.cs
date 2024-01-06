using System;
using DG.Tweening;
using Game;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(fileName = "Gun", menuName = "Game/Gun Data", order = 0)]
    public class GunSo : ScriptableObject
    {
        [SerializeField] public TransformData holsterTransformData;
        [SerializeField] public float jumpPower = 2.25f;
        [SerializeField] public float travelDuration = 0.45f;
        [SerializeField] public Ease ease = Ease.Linear;
        [SerializeField] public AudioClip audioClip;
        [Range(0.0f, 1.0f)] [SerializeField] public float audioVolume = 1.0f;
    }

    [Serializable]
    public class TransformData
    {
        public Vector3 localPosition;
        public Vector3 localEulerAngles;
        public Vector3 localScale;
    }
    
    
}

public static class GunSoExtension
{
    public static void Set(this Transform transform, TransformData transformData)
    {
        transform.localPosition = transformData.localPosition;
        transform.localEulerAngles = transformData.localEulerAngles;
        transform.localScale = transformData.localScale;
    }
}