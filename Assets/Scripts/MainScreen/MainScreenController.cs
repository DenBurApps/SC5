using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MainScreen
{
    public class MainScreenController : MonoBehaviour
    {
        private const int GamePrice = 2000;
        
        [SerializeField] private Color _decimalColor;
        
        [SerializeField] private Button _chinaStreetButton;
        [SerializeField] private Button _bambooFortuneButton;
        [SerializeField] private Button _jungleMystiqueButton;
        [SerializeField] private Button _spinFestivalButton;
        [SerializeField] private TMP_Text _playerBalance;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private ParticleSystem[] _particles;
        [SerializeField] private ConfirmPurchaseScreen _confirmPurchaseScreen;
        [SerializeField] private GameObject _bambooPricePlane;
        [SerializeField] private GameObject _spinPricePlane;
        [SerializeField] private GameObject _notEnoughPlane;
        [SerializeField] private LuckTestButton _luckTestButton;
        [SerializeField] private Onboarding _onboarding;

        private GameType _selectedGame;

        private void Awake()
        {
            DisableScreen();
        }

        private void Start()
        {
            UpdateBalance(PlayerDataController.CurrentBalance);
            UpdatePurchaseStatus();
        }

        private void OnEnable()
        {
            PlayerDataController.OnBalanceChanged += UpdateBalance;
            PlayerDataController.OnGamePurchaseStatusChanged += OnGamePurchaseStatusChanged;
            _confirmPurchaseScreen.ConfirmClicked += OnPurchaseConfirmed;
            
            _chinaStreetButton.onClick.AddListener(LoadChinaStreet);
            _bambooFortuneButton.onClick.AddListener(() => HandleGameButton(GameType.BambooFortune, "Bamboo Fortune"));
            _jungleMystiqueButton.onClick.AddListener(LoadJungleMystique);
            _spinFestivalButton.onClick.AddListener(() => HandleGameButton(GameType.SpinFestival, "Spin Festival"));
            _luckTestButton.LuckTestOpened += DisableScreen;
            _onboarding.OnboardingShown += EnableScreen;
        }

        private void OnDisable()
        {
            PlayerDataController.OnBalanceChanged -= UpdateBalance;
            PlayerDataController.OnGamePurchaseStatusChanged -= OnGamePurchaseStatusChanged;
            _confirmPurchaseScreen.ConfirmClicked -= OnPurchaseConfirmed;
            
            _chinaStreetButton.onClick.RemoveListener(LoadChinaStreet);
            _bambooFortuneButton.onClick.RemoveListener(() => HandleGameButton(GameType.BambooFortune, "Bamboo Fortune"));
            _jungleMystiqueButton.onClick.RemoveListener(LoadJungleMystique);
            _spinFestivalButton.onClick.RemoveListener(() => HandleGameButton(GameType.SpinFestival, "Spin Festival"));
            _luckTestButton.LuckTestOpened -= DisableScreen;
            _onboarding.OnboardingShown -= EnableScreen;
        }

        private void UpdatePurchaseStatus()
        {
            _bambooPricePlane.SetActive(!PlayerDataController.IsGamePurchased(GameType.BambooFortune));
            _spinPricePlane.SetActive(!PlayerDataController.IsGamePurchased(GameType.SpinFestival));
        }

        private void HandleGameButton(GameType gameType, string gameName)
        {
            if (PlayerDataController.IsGamePurchased(gameType))
            {
                LoadGameScene(gameType);
            }
            else if (PlayerDataController.CurrentBalance >= GamePrice)
            {
                _selectedGame = gameType;
                _confirmPurchaseScreen.EnableScreen(gameName);
            }
            else
            {
                _notEnoughPlane.SetActive(true);
            }
        }

        private void OnPurchaseConfirmed()
        {
            PlayerDataController.PurchaseGame(_selectedGame, GamePrice);
        }

        private void OnGamePurchaseStatusChanged(GameType gameType, bool isPurchased)
        {
            UpdatePurchaseStatus();
            if (isPurchased)
            {
                LoadGameScene(gameType);
            }
        }

        private void LoadGameScene(GameType gameType)
        {
            string sceneName = gameType switch
            {
                GameType.BambooFortune => "BambooFortuneScene",
                GameType.SpinFestival => "SpinFestivalScene",
                _ => throw new ArgumentException($"Unknown game type: {gameType}")
            };
            
            SceneLoader.LoadScene(sceneName);
        }

        public void EnableScreen()
        {
            _canvasGroup.alpha = 1;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;

            foreach (var particle in _particles)
            {
                particle.Play();
            }
        }

        public void DisableScreen()
        {
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            
            foreach (var particle in _particles)
            {
                particle.Stop();
            }
        }
        
        private void UpdateBalance(int balance)
        {
            string decimalColorHex = ColorUtility.ToHtmlStringRGBA(_decimalColor);
            _playerBalance.text = $"{balance}<color=#{decimalColorHex}>.00</color>";
        }

        private void LoadChinaStreet()
        {
            SceneLoader.LoadScene("ChinaStreetScene");
        }

        private void LoadJungleMystique()
        {
            SceneLoader.LoadScene("JungleMystiqueScene");
        }
    }
}