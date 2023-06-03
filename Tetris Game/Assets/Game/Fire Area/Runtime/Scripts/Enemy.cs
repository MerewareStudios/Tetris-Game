using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float speed = 0.5f;
    void Update()
    {
        transform.position += transform.forward * (Time.deltaTime * speed);
    }

    public void Kill()
    {
        Particle.BloodExplosion.Play(transform.position);
        this.Despawn();
    }
}
