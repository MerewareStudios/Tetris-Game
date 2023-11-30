using System;
using System.Collections;
using DG.Tweening;
using Game;
using Internal.Core;
using UnityEngine;
using Random = UnityEngine.Random;

public class Airplane : MonoBehaviour
{
    [SerializeField] private Transform thisTransform;
    [SerializeField] private Transform startPivot;
    [SerializeField] private Transform target;
    [SerializeField] private float altitude = 28.0f;
    [SerializeField] private ParticleSystem leftEnginePS;
    [SerializeField] private ParticleSystem rightEnginePS;
    [System.NonSerialized] private Tween _delayedDisable;

    public void CarryCargo(Pawn.Usage usage)
    {
        this.gameObject.SetActive(true);
        thisTransform.DOKill();
        _delayedDisable?.Kill();
        
        Vector3 startPosition = startPivot.position + Vector3.forward * Random.Range(-4.0f, 4.0f);
        startPosition.y = altitude;
        Vector3 targetPosition = target.position;
        targetPosition.y = altitude;
        Vector3 direction = (targetPosition - startPosition).normalized;
        Vector3 endPosition = targetPosition + direction * 4.0f;

        thisTransform.position = startPosition;
        thisTransform.forward = direction;

        Travel(targetPosition, 5.0f, Ease.OutSine).onComplete = () =>
        {
            Travel(endPosition, 20.0f, Ease.InSine).onComplete = () =>
            {
                _delayedDisable = DOVirtual.DelayedCall(0.6f, () =>
                {
                    this.gameObject.SetActive(false);
                }, false);
            };
        };
        
        leftEnginePS.Clear();
        leftEnginePS.Play();
        rightEnginePS.Clear();
        rightEnginePS.Play();
    }


    private Tween Travel(Vector3 toPosition, float speed, Ease ease)
    {
        return thisTransform.DOMove(toPosition, speed).SetSpeedBased(true).SetEase(ease, 4.0f);
    }
}
