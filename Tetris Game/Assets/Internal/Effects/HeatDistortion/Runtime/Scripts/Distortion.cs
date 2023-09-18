using DG.Tweening;
using Internal.Core;
using UnityEngine;

namespace Visual.Effects
{
    public class Distortion : MonoBehaviour
    {
        public static readonly int PowerID = Shader.PropertyToID("_Power");
        public static Distortion Recent = null;

        [SerializeField] private MeshRenderer meshRenderer;
        [System.NonSerialized] public static System.Action<GameObject, bool> OnComplete;
        [System.NonSerialized] private Tween animationTween;
    
        public static void SetPropertyBlock(System.Action<GameObject, bool> OnComplete)
        {
            // Distortion.rampID = rampID;
            // Distortion.powerID = powerID;
            Distortion.OnComplete = OnComplete;
        }

        public void Distort(Vector3 worldPosition, Vector3 forward, float scale, float power, float duration, Ease ease, float delay)
        {
            var thisTransform = transform;
            thisTransform.position = worldPosition;
            thisTransform.forward = forward;
            thisTransform.localScale = Vector3.zero;

            Distortion.Recent = this;
            
            float value = 0.0f;
            animationTween?.Kill();
            animationTween = DOTween.To((x) => value = x, 0.0f, 1.0f, duration).SetEase(ease).SetDelay(delay);
            animationTween.onUpdate = () =>
            {
                meshRenderer.material.SetFloat(PowerID, power * (1.0f - value));
                thisTransform.localScale = Vector3.one * scale * value;
            };
            animationTween.onComplete = () => OnComplete.Invoke(this.gameObject, Distortion.Recent == this);
        }
    }
}
