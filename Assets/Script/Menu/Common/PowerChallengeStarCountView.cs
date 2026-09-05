using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace YARG.Menu
{
    public class PowerChallengeStarCountView : MonoBehaviour
    {
        [SerializeField]
        private Image _starIcon;
        [SerializeField]
        private Sprite _standardStar;
        [SerializeField]
        private Sprite _goldStar;
        [SerializeField]
        private TextMeshProUGUI _countText;

        public void SetStars(int count, int maxStars)
        {
            _starIcon.sprite = count >= maxStars ? _goldStar : _standardStar;
            _countText.text = $"x{count}";
            
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform) transform);
        }
    }
}