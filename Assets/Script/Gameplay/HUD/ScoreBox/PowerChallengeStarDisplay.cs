using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YARG.Audio;
using YARG.Core.Audio;
using YARG.Player;

namespace YARG.Gameplay.HUD
{
    public class PowerChallengeStarDisplay : GameplayBehaviour
    {
        private const int MAX_STARS = YargPlayer.POWER_CHALLENGE_MAX_STARS;
        private const int LAST_STAR = MAX_STARS - 1;

        [SerializeField]
        private Image _emptyStar;
        [SerializeField]
        private Image _completedStar;
        [SerializeField]
        private Image _starProgress;
        [SerializeField]
        private Image _completedGold;
        [SerializeField]
        private Image _white;

        [Space]
        [SerializeField]
        private CanvasGroup _goldProgressGroup;
        [SerializeField]
        private Image _goldProgress;
        [SerializeField]
        private RawImage _goldProgressLine;

        [SerializeField]
        private TextMeshProUGUI _countText;

        private int  _currentStar;
        private bool _isGoldAchieved;
        private bool _hasObtainedFirstStar;

        private Vector3 _baseScale;
        private float   _goldMeterHeight;

        private Sequence _pulseSequence;
        private Sequence _goldSequence;

        private int _highestStarReached;

        protected override void GameplayAwake()
        {
            var t = transform;
            _baseScale = t.localScale;
            _goldMeterHeight = _goldProgress.rectTransform.rect.height;

            _pulseSequence = DOTween.Sequence()
                .Append(t.DOScale(_baseScale * 1.3f, 0.15f))
                .Append(t.DOScale(_baseScale, 0.15f))
                .SetAutoKill(false).Pause().SetLink(gameObject);

            _goldSequence = DOTween.Sequence()
                .Append(t.DOScale(_baseScale * 1.6f, 0.25f))
                .Insert(0.04f, _white.DOFade(1f, 0.21f))
                .Append(t.DOScale(_baseScale, 0.25f))
                .Insert(0.25f, _white.DOFade(0f, 0.25f))
                .Insert(0.25f, _completedGold.DOFade(1f, 0.25f))
                .SetAutoKill(false).Pause().SetLink(gameObject);
        }

        private void Update()
        {
            if (_currentStar == LAST_STAR && !_isGoldAchieved)
            {
                float pulse = 1 - (float) ((GameManager.BeatEventHandler.Visual.StrongBeat.CurrentProgress / 2) % 1);
                _goldProgressGroup.alpha = pulse;
            }
        }

        public void ResetDisplay()
        {
            _currentStar = 0;
            _highestStarReached = 0;
            _isGoldAchieved = false;
            _hasObtainedFirstStar = false;

            _emptyStar.gameObject.SetActive(true);
            _completedStar.gameObject.SetActive(false);

            _starProgress.gameObject.SetActive(true);
            _starProgress.fillAmount = 0;

            _goldProgressGroup.gameObject.SetActive(false);
            _goldProgress.fillAmount = 0;
            _goldProgressLine.rectTransform.anchoredPosition = Vector2.zero;

            _countText.text = string.Empty;

            _pulseSequence.Rewind();
            _goldSequence.Rewind();
            transform.localScale = _baseScale;
        }

        public void SetStars(float stars)
        {
            stars = Mathf.Clamp(stars, 0f, MAX_STARS);
            int   topStar      = (int) stars;
            float starProgress = stars - topStar;

            _currentStar = topStar;
            _countText.text = _currentStar > 0 ? _currentStar.ToString() : string.Empty;

            if (topStar > _highestStarReached)
            {
                _highestStarReached = topStar;

                if (!_hasObtainedFirstStar)
                {
                    _hasObtainedFirstStar = true;
                    _emptyStar.gameObject.SetActive(false);
                    _completedStar.gameObject.SetActive(true);
                }

                if (_highestStarReached < MAX_STARS)
                {
                    GlobalAudioHandler.PlaySoundEffect(SfxSample.StarGain);
                    _pulseSequence.Restart();
                }
            }

            if (topStar >= MAX_STARS)
            {
                _starProgress.gameObject.SetActive(false);
                _goldProgressGroup.gameObject.SetActive(false);

                if (!_isGoldAchieved)
                {
                    GlobalAudioHandler.PlaySoundEffect(SfxSample.StarGold);
                    _isGoldAchieved = true;
                    _goldSequence.Restart();
                }

                return;
            }

            if (topStar >= LAST_STAR)
            {
                _starProgress.gameObject.SetActive(false);
                _goldProgressGroup.gameObject.SetActive(true);
                _goldProgress.fillAmount = starProgress;
                _goldProgressLine.rectTransform.anchoredPosition = new Vector2(0, starProgress * _goldMeterHeight);
            }
            else
            {
                _goldProgressGroup.gameObject.SetActive(false);
                _starProgress.gameObject.SetActive(true);
                _starProgress.fillAmount = starProgress;
            }
        }
    }
}