using DG.Tweening;
using Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Anim Const", menuName = "Game/Anim Const", order = 0)]
public class AnimConst : SSingleton<AnimConst>
{
    [Header("Pawn")]
    public Ease moveEase;
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
    [Header("Explode")]
    public Vector3 fragmentScale;
    public float explodePower;
    public float jumpPower;
    public float jumpDuration;
    public Ease explosionJumpEase;


    public float MergeShowDelay => mergeTravelDelay + mergeTravelDur;
}
