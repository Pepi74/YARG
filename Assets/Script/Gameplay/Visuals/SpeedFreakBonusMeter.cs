using DG.Tweening;
using UnityEngine;

namespace YARG.Gameplay.Visuals
{
    public class SpeedFreakBonusMeter : MonoBehaviour
    {
        private static readonly int _progressProperty  = Shader.PropertyToID("_Progress");
        private static readonly int _fadeAlphaProperty = Shader.PropertyToID("_FadeAlpha");
        private const string SPEED_FREAK_COLOR_PROPERTY = "_SpeedFreakColor";

        private const float FADE_OUT_SECONDS = 0.4f;

        [SerializeField]
        private MeshRenderer _meshRenderer;

        [SerializeField]
        private Transform[] _shrinkTransforms;

        [SerializeField]
        private Color _color = Color.green;

        private Vector3[] _shrinkBaseScales;
        private Sequence  _fadeSequence;
        private bool      _visible;

        private void Awake()
        {
            _meshRenderer.material.SetColor(SPEED_FREAK_COLOR_PROPERTY, _color);

            _shrinkBaseScales = new Vector3[_shrinkTransforms.Length];
            for (int i = 0; i < _shrinkTransforms.Length; i++)
            {
                _shrinkBaseScales[i] = _shrinkTransforms[i].localScale;
            }
        }

        public void SetProgress(float progress)
        {
            _fadeSequence?.Kill();

            if (!_visible)
            {
                _visible = true;
                gameObject.SetActive(true);
                _meshRenderer.material.SetFloat(_fadeAlphaProperty, 1f);
                for (int i = 0; i < _shrinkTransforms.Length; i++)
                {
                    _shrinkTransforms[i].localScale = _shrinkBaseScales[i];
                }
            }

            float shaderValue = Mathf.Clamp01(progress) - 0.5f;
            _meshRenderer.material.SetFloat(_progressProperty, shaderValue);
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
            _fadeSequence.Join(_meshRenderer.material.DOFloat(0f, _fadeAlphaProperty, FADE_OUT_SECONDS));
            for (int i = 0; i < _shrinkTransforms.Length; i++)
            {
                _fadeSequence.Join(_shrinkTransforms[i].DOScale(Vector3.zero, FADE_OUT_SECONDS));
            }
            _fadeSequence.OnComplete(() => gameObject.SetActive(false));
        }

        public void HideAtFull()
        {
            if (!_visible)
            {
                return;
            }

            float shaderValue = 0.5f;
            _meshRenderer.material.SetFloat(_progressProperty, shaderValue);
            Hide();
        }
    }
}