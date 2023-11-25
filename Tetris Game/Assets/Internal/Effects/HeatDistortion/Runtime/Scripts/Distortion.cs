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
        [System.NonSerialized] public static System.Action<GameObject, bool> Complete;
        [System.NonSerialized] private Tween _animationTween;
    
        public void Distort(Vector3 worldPosition, Vector3 forward, float scale, float power, float duration, Ease ease)
        {
            var thisTransform = transform;
            thisTransform.position = worldPosition;
            thisTransform.forward = forward;
            thisTransform.localScale = Vector3.zero;

            Distortion.Recent = this;
            
            float value = 0.0f;
            _animationTween?.Kill();
            _animationTween = DOTween.To((x) => value = x, 0.0f, 1.0f, duration).SetEase(ease).SetUpdate(true);
            _animationTween.onUpdate = () =>
            {
                meshRenderer.material.SetFloat(PowerID, power * (1.0f - value));
                thisTransform.localScale = Vector3.one * scale * value;
            };
            _animationTween.onComplete = () => Complete.Invoke(this.gameObject, Distortion.Recent == this);
        }
    }
}
