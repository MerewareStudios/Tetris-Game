using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CubeExplosion : MonoBehaviour
{
    [SerializeField] private List<Transform> fragments;
    [SerializeField] private List<Vector3> positions;

    private int index = 0;
    public void Explode(Vector3 position)
    {
        index = 0;
        transform.SetPositionAndRotation(position, Random.rotation);
        for (int i = 0; i < fragments.Count; i++)
        {
            Transform fragment = fragments[i];
            
            fragment.DOKill();
            
            fragment.localScale = AnimConst.THIS.fragmentScale;
            fragment.localPosition = positions[i];
            fragment.localEulerAngles = Vector3.zero;
            
            fragment.DOScale(Vector3.zero, 0.35f).SetDelay(Random.Range(0.35f, 0.5f)).onComplete = () =>
            {
                index++;
                if (index == fragments.Count)
                {
                    this.Despawn();
                }
            };
            fragment.DORotate(Random.insideUnitSphere * 360.0f, AnimConst.THIS.jumpDuration);
            
            var fragPos = fragment.position;
            Vector3 target = fragPos + (fragPos - position).normalized * (AnimConst.THIS.explodePower * Random.Range(0.5f, 1.0f));
            target.y = 0.263f;
            
            fragment.DOJump(target, AnimConst.THIS.jumpPower * Random.Range(0.5f, 1.0f), 1, AnimConst.THIS.jumpDuration).SetEase(AnimConst.THIS.explosionJumpEase);
        }
    }
}
