using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float speed = 0.5f;
    [System.NonSerialized] private Transform _thisTransform;

    private void Awake()
    {
        _thisTransform = this.transform;
    }

    public void OnSpawn(Vector3 position, Vector3 forward)
    {
        _thisTransform.forward = forward;

        _thisTransform.position = position;
        _thisTransform.DOKill();
        _thisTransform.localScale = Vector3.zero;
        _thisTransform.DOScale(Vector3.one, 0.2f).SetEase(Ease.Linear);
    }
    void Update()
    {
        transform.position += transform.forward * (Time.deltaTime * speed);
        if (transform.position.z < FireArea.THIS.protectionLine.position.z)
        {
            Kill();
            FireArea.THIS._turret.NextTarget();
        }
    }

    public void Kill()
    {
        _thisTransform.DOKill();
        Particle.BloodExplosion.Play(transform.position);
        this.Despawn();
    }
}
