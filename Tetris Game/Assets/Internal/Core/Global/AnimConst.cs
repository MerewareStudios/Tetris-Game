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
    public float mergedScalePunch = 0.35f;
    public float mergedScaleDuration = 0.35f;
    [Header("Distort")]
    public float distortScale = 4.0f;
    public float distortStartRamp = -0.2f;
    public float distortEndRamp = 0.6f;
    public float distortDuration = 0.55f;
    public Ease distortEase;


    public float MergeTotalDur => mergeTravelDelay + mergeTravelDur;
}
