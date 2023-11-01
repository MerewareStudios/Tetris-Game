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
        [SerializeField] public float speed = 0.45f;
        [SerializeField] public bool jump = true;
        [SerializeField] public Ease ease = Ease.Linear;
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
    // public static Pool GetPrefab(this Pool gunType)
    // {
    //     GunSo gunSo = Const.THIS.Guns[(int)gunType];
    //     return gunSo.prefab;
    // } 
    // public static TransformData GetTransformData(this Pool type)
    // {
    //     GunSo gunSo = Const.THIS.Guns[(int)type];
    //     return gunSo.holsterTransformData;
    // }
    public static void Set(this Transform transform, TransformData transformData)
    {
        transform.localPosition = transformData.localPosition;
        transform.localEulerAngles = transformData.localEulerAngles;
        transform.localScale = transformData.localScale;
    }
}