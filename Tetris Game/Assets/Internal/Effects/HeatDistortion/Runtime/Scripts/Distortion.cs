using DG.Tweening;
using Internal.Core;
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

        public void Distort(Vector3 worldPosition, Vector3 forward, float scale, float startRamp, float endRamp, float duration, Ease ease, float delay)
        {
            var thisTransform = transform;
            thisTransform.position = worldPosition;
            thisTransform.forward = forward;
            thisTransform.localScale = Vector3.one * scale;
            Animate(startRamp, endRamp, 0.135f, 0.0f, duration, ease, delay);
        }

        private void Animate(float startRamp, float endRamp, float startPower, float endPower, float duration, Ease ease, float delay)
        {
            meshRenderer.SetFloat(mpb, rampID, startRamp);
            meshRenderer.SetFloat(mpb, powerID, startPower);

            float value = 0.0f;
            animationTween?.Kill();
            animationTween = DOTween.To((x) => value = x, 0.0f, 1.0f, duration).SetEase(ease).SetDelay(delay);
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
