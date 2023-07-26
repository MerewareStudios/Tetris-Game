using DG.Tweening;
using Internal.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Visual.Effects
{
    public class Distortion : MonoBehaviour
    {
        [SerializeField] private MeshRenderer meshRenderer;
        [System.NonSerialized] private static MaterialPropertyBlock mpb;
        [System.NonSerialized] private static int distortionID;
        [System.NonSerialized] private Tween distanceTween;
    
        public static void SetPropertyBlock(MaterialPropertyBlock materialPropertyBlock, int distortionID)
        {
            Distortion.mpb = materialPropertyBlock;
            Distortion.distortionID = distortionID;
        }

        public void Distort(Vector3 worldPosition, Vector3 forward, Vector3 scale, float duration)
        {
            transform.position = worldPosition;
            transform.forward = forward;
            transform.localScale = scale;
            AnimateDistance(-0.1f, 1.0f, duration);
        }

        private void AnimateDistance(float start, float end, float duration)
        {
            float value = 0.0f;
            distanceTween?.Kill();
            distanceTween = DOTween.To((x) => value = x, start, end, duration).SetEase(Ease.InOutSine);
            distanceTween.onUpdate = () =>
            {
                meshRenderer.SetFloat(mpb, distortionID, value);
            };
        }
    }
}
