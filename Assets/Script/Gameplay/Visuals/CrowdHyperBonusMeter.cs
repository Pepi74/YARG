using DG.Tweening;
using UnityEngine;

namespace YARG.Gameplay.Visuals
{
    public class CrowdHyperBonusMeter : MonoBehaviour
    {
        // Reuses the Star Power bar's shader, whose fill is driven by this property.
        private static readonly int _fillProperty      = Shader.PropertyToID("Fill");
        private static readonly int _fadeAlphaProperty = Shader.PropertyToID("_FadeAlpha");

        private const float FADE_OUT_SECONDS = 0.4f;

        [SerializeField]
        private MeshRenderer _barRenderer;

        private Sequence _fadeSequence;
        private bool     _visible;

        public void SetProgress(float progress)
        {
            _fadeSequence?.Kill();

            if (!_visible)
            {
                _visible = true;
                gameObject.SetActive(true);
                _barRenderer.material.SetFloat(_fadeAlphaProperty, 1f);
            }

            _barRenderer.material.SetFloat(_fillProperty, Mathf.Clamp01(progress));
        }

        public void Hide()
        {
            if (!_visible)
            {
                return;
            }

            _visible = false;
            _fadeSequence?.Kill();

            _fadeSequence = DOTween.Sequence();
            _fadeSequence.Join(_barRenderer.material.DOFloat(0f, _fadeAlphaProperty, FADE_OUT_SECONDS));
            _fadeSequence.OnComplete(() => gameObject.SetActive(false));
        }

        public void HideAtFull()
        {
            if (!_visible)
            {
                return;
            }

            _barRenderer.material.SetFloat(_fillProperty, 1f);
            Hide();
        }
    }
}
