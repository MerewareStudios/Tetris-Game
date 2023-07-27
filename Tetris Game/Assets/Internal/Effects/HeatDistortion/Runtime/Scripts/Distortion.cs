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
        [System.NonSerialized] private static int rampID;
        [System.NonSerialized] private static int powerID;
        [System.NonSerialized] public static System.Action<GameObject> OnComplete;
        [System.NonSerialized] private Tween animationTween;
    
        public static void SetPropertyBlock(MaterialPropertyBlock materialPropertyBlock, int rampID, int powerID, System.Action<GameObject> OnComplete)
        {
            Distortion.mpb = materialPropertyBlock;
            Distortion.rampID = rampID;
            Distortion.powerID = powerID;
            Distortion.OnComplete = OnComplete;
        }

        public void Distort(Vector3 worldPosition, Vector3 forward, Vector3 scale, float duration, float delay)
        {
            transform.position = worldPosition;
            transform.forward = forward;
            transform.localScale = scale;
            Animate(-0.1f, 0.8f, 0.1f, 0.0f, duration, delay);
        }

        private void Animate(float startRamp, float endRamp, float startPower, float endPower, float duration, float delay)
        {
            meshRenderer.SetFloat(mpb, rampID, startRamp);
            meshRenderer.SetFloat(mpb, powerID, startPower);

            float value = 0.0f;
            animationTween?.Kill();
            animationTween = DOTween.To((x) => value = x, 0.0f, 1.0f, duration).SetEase(Ease.InSine).SetDelay(delay);
            animationTween.onUpdate = () =>
            {
                float ramp = Mathf.Lerp(startRamp, endRamp, value);
                meshRenderer.SetFloat(mpb, rampID, ramp);

                float power = Mathf.Lerp(startPower, endPower, value);
                meshRenderer.SetFloat(mpb, powerID, power);
            };
            animationTween.onComplete = () => OnComplete.Invoke(this.gameObject);
        }
    }
}
