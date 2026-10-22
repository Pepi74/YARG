using TMPro;
using UnityEngine;
using YARG.Core.Input;
using YARG.Menu.Career;
using YARG.Menu.MusicLibrary;
using YARG.Menu.Navigation;
using YARG.Menu.Persistent;
using YARG.Player;

namespace YARG.Menu
{
    public class GameModesMenu : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _versionText;

        [SerializeField]
        private GameObject _powerChallengeButton;

        private NavigatableBehaviour _powerChallengeNavigatable;

        private bool _started;

        private void Awake()
        {
            _powerChallengeNavigatable = _powerChallengeButton.GetComponent<NavigatableBehaviour>();
        }

        private void Start()
        {
            _versionText.text = GlobalVariables.Instance.CurrentVersion;

            _started = true;
            UpdatePowerChallengeAvailability();
        }

        private void OnEnable()
        {
            // Set navigation scheme
            Navigator.Instance.PushScheme(new NavigationScheme(new()
            {
                NavigationScheme.Entry.NavigateSelect,
                NavigationScheme.Entry.NavigateUp,
                NavigationScheme.Entry.NavigateDown,
                new NavigationScheme.Entry(MenuAction.Red, "Menu.Common.Back", () => MenuManager.Instance.PopMenu())
            }, true));

            PlayerContainer.PlayerAdded += OnPlayerCountChanged;
            PlayerContainer.PlayerRemoved += OnPlayerCountChanged;

            if (_started)
            {
                UpdatePowerChallengeAvailability();
            }
        }

        private void OnPlayerCountChanged(YargPlayer player)
        {
            UpdatePowerChallengeAvailability();
        }

        private void UpdatePowerChallengeAvailability()
        {
            bool available = PlayerContainer.Players.Count == 1;
            if (_powerChallengeButton.activeSelf == available)
            {
                return;
            }

            if (!available)
            {
                _powerChallengeNavigatable.SetSelected(false, SelectionOrigin.Programmatically);
            }

            _powerChallengeButton.SetActive(available);
        }

        public void CareerList()
        {
            var menu = MenuManager.Instance.PushMenu(MenuManager.Menu.CareerList, false);
            menu.gameObject.SetActive(true);
        }

        public void Practice()
        {
            var menu = MenuManager.Instance.PushMenu(MenuManager.Menu.MusicLibrary, false);

            MusicLibraryMenu.LibraryMode = MusicLibraryMode.Practice;

            menu.gameObject.SetActive(true);
        }

        public void PowerChallenge()
        {
            var menu = MenuManager.Instance.PushMenu(MenuManager.Menu.MusicLibrary, false);

            MusicLibraryMenu.LibraryMode = MusicLibraryMode.PowerChallenge;

            menu.gameObject.SetActive(true);
        }

        public void Replays()
        {
            MenuManager.Instance.PushMenu(MenuManager.Menu.History);
        }

        public void Back()
        {
            MenuManager.Instance.PopMenu();
        }

        private void OnDisable()
        {
            Navigator.Instance?.PopScheme();

            PlayerContainer.PlayerAdded -= OnPlayerCountChanged;
            PlayerContainer.PlayerRemoved -= OnPlayerCountChanged;
        }
    }
}