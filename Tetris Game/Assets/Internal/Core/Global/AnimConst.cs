using DG.Tweening;
using Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Anim Const", menuName = "Game/Anim Const", order = 0)]
public class AnimConst : SSingleton<AnimConst>
{
    [Header("Board")]
    public float mergeTravelDelay = 0.15f;
    public float mergeTravelDur = 0.2f;
    public float mergeTravelShoot = 0.2f;
    public Ease mergeTravelEase;
}
