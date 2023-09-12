using System.Collections.Generic;
using DG.Tweening;
using Internal.Core;
using UnityEngine;

public class CubeExplosion : MonoBehaviour
{
    [SerializeField] private List<Transform> fragments;
    [SerializeField] private List<Rigidbody> rbs;

    public void Explode(Vector3 position)
    {
        int index = 0;
        transform.SetPositionAndRotation(position, Random.rotation);
        for (int i = 0; i < fragments.Count; i++)
        {
            Transform fragment = fragments[i];
            
            fragment.DOKill();

            Vector3 direction = Random.insideUnitSphere.normalized;
            
            fragment.localScale = AnimConst.THIS.fragmentScale;
            // fragment.localPosition = positions[i] + ((fragment.position - position).normalized * 0.15f);
            fragment.localPosition = AnimConst.THIS.explodeRadius * direction;
            fragment.rotation = Random.rotation;
            
            // renderers[i].SetColor(GameManager.MPB_EXPLODE, GameManager.BaseColor, AnimConst.THIS.explodeGradient.Evaluate(0.0f));
            fragment.DOScale(Vector3.zero, 0.4f).SetEase(Ease.InOutSine).SetDelay(Random.Range(0.35f, 0.5f)).onComplete = () =>
            {
                index++;
                if (index == fragments.Count)
                {
                    this.Despawn();
                }
            };

            Rigidbody rb = rbs[i];
            rb.velocity = direction * (AnimConst.THIS.explodePower * Random.Range(0.5f, 1.0f)) + new Vector3(0.0f, AnimConst.THIS.jumpPower * (fragment.position.y - position.y), 0.0f);
            rb.angularVelocity = Random.insideUnitSphere * 20.0f;
        }
    }
}
