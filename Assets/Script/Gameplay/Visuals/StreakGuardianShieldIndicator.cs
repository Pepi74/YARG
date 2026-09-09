using DG.Tweening;
using UnityEngine;

namespace YARG.Gameplay.Visuals
{
    public class StreakGuardianShieldIndicator : MonoBehaviour
    {
        private static readonly int _rechargeProgressProperty = Shader.PropertyToID("_RechargeProgress");

        private const float POP_SCALE      = 1.3f;
        private const float POP_SECONDS    = 0.15f;

        // Must match STREAK_GUARDIAN_SHIELD_COOLDOWN_SECONDS in BaseEngine.cs (YARG.Core).
        private const float RECHARGE_SECONDS = 1f;

        [SerializeField]
        private MeshRenderer[] _shieldRenderers;

        private Vector3[]  _baseScales;
        private Sequence[] _popSequences;
        private bool[]     _previousCharged;

        private void Awake()
        {
            _baseScales   = new Vector3[_shieldRenderers.Length];
            _popSequences = new Sequence[_shieldRenderers.Length];
            _previousCharged = new bool[_shieldRenderers.Length];

            for (int i = 0; i < _shieldRenderers.Length; i++)
            {
                var t = _shieldRenderers[i].transform;
                _baseScales[i] = t.localScale;

                _popSequences[i] = DOTween.Sequence()
                    .Append(t.DOScale(_baseScales[i] * POP_SCALE, POP_SECONDS))
                    .Append(t.DOScale(_baseScales[i], POP_SECONDS))
                    .SetAutoKill(false).Pause().SetLink(gameObject);

                _shieldRenderers[i].material.SetFloat(_rechargeProgressProperty, 1f);
                _previousCharged[i] = true;
            }
        }

        public void SetShieldStates(bool[] charged)
        {
            for (int i = 0; i < _shieldRenderers.Length && i < charged.Length; i++)
            {
                if (_previousCharged[i] && !charged[i])
                {
                    // This shield was just consumed: pop it, and start its recharge from red, filling back in over time.
                    _popSequences[i].Restart();

                    var material = _shieldRenderers[i].material;
                    material.DOKill();
                    material.SetFloat(_rechargeProgressProperty, 0f);
                    material.DOFloat(1f, _rechargeProgressProperty, RECHARGE_SECONDS);
                }

                _previousCharged[i] = charged[i];
            }
        }
    }
}