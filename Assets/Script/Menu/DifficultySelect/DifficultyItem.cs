using TMPro;
using UnityEngine;
using UnityEngine.Events;
using YARG.Menu.Navigation;

namespace YARG.Menu.DifficultySelect
{
    public class DifficultyItem : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _header;
        [SerializeField]
        private TextMeshProUGUI _body;

        [field: SerializeField]
        public NavigatableButton Button { get; private set; }

        private bool _interactable = true;

        private void LateUpdate()
        {
            float alpha = _interactable ? 1f : 0.4f;
            _header.alpha = alpha;
            _body.alpha = alpha;
        }

        public bool Interactable
        {
            get => Button.Interactable;
            set
            {
                Button.Interactable = value;
                _interactable = value;
            }
        }

        public void Initialize(string header, string body, UnityAction action)
        {
            _header.gameObject.SetActive(true);
            _header.text = header;

            _body.text = body;
            Button.SetOnClickEvent(action);
        }

        public void Initialize(string body, UnityAction action)
        {
            _header.gameObject.SetActive(false);

            _body.text = body;
            Button.SetOnClickEvent(action);
        }
    }
}