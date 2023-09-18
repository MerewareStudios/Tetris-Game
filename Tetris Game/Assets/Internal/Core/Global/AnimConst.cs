using DG.Tweening;
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
    public float mergedPunchScale = 0.35f;
    public float mergedPunchDuration = 0.35f;
    [Header("Distort")]
    public float distortScale = 4.0f;
    public float distortPower;
    public float distortDuration = 0.55f;
    public Ease distortEase;
    [Header("Explode")]
    public Vector3 fragmentScale;
    public float explodePower;
    public float explodeRadius;
    public float jumpPower;
    [Header("Glimmer")]
    public float glimmerSpeedBlock;
    public float glimmerSpeedWeapon;
    public float glimmerSpeedUpgrade;
    public Ease glimmerEase;


    public float MergeShowDelay => mergeTravelDelay + mergeTravelDur;
}
