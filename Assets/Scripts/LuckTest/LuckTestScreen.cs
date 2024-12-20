using System;
using System.Collections.Generic;
using System.Linq;
using DailyBonus;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace LuckTest
{
    public class LuckTestScreen : MonoBehaviour
    {
        private const int InitialChestsToOpen = 3;
        private const int TryCost = 200;
        private const string InitialTopText = "Pick 3 chests";
        private const string GameOverTopText = "Trying is over";
        private const string PickTextTemplate = "Pick {0} chests";

        [SerializeField] private List<Chest> _chests;
        [SerializeField] private GameObject _totalWinPanel;
        [SerializeField] private TMP_Text _totalWinText;
        [SerializeField] private TMP_Text _topText;
        [SerializeField] private Button _getButton;
        [SerializeField] private Button _showAllChestsButton;
        [SerializeField] private Button _tryButton;
        [SerializeField] private GameObject _notEnoughBalancePopup;
        [SerializeField] private GameObject _mainScreen;
        [SerializeField] private CanvasGroup _canvasGroup;

        private readonly int[] _availableCoins =
        {
            5, 15, 15, 15, 15, 30, 30, 30, 35, 35, 35, 35, 40, 40, 40, 40, 40, 40, 40, 45, 50, 50, 50, 55, 55, 55, 60,
            60, 65, 65, 75, 80, 85, 90, 95, 100
        };

        private readonly int _bigScore = 2000;

        private int _chestsToOpen;
        private int _totalWin;
        private List<Chest> _openedChests = new List<Chest>();

        private void Start()
        {
            DisableScreen();
        }

        public void EnableScreen()
        {
            _canvasGroup.alpha = 1;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            InitializeScreen();
        }

        public void DisableScreen()
        {
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            CleanupScreen();
        }

        private void InitializeScreen()
        {
            ResetScreen();

            foreach (var chest in _chests)
            {
                chest.Enable(GetRandomCoinValue());
                chest.Opened += OnChestOpened;
            }

            int randomNumber = Random.Range(0, 2);

            if (randomNumber == 1)
            {
                SetBigScoreChest();
            }

            _chestsToOpen = InitialChestsToOpen;

            UpdateTopText();
            ManageButtonsState(showGetButton: false, showTryButton: false, showShowAllButton: false);

            _getButton.onClick.AddListener(OnGetButtonClicked);
            _tryButton.onClick.AddListener(OnTryClicked);
            _showAllChestsButton.onClick.AddListener(ShowAllChests);
            _notEnoughBalancePopup.gameObject.SetActive(false);
        }

        private void CleanupScreen()
        {
            foreach (var chest in _chests)
            {
                chest.Opened -= OnChestOpened;
            }

            _getButton.onClick.RemoveListener(OnGetButtonClicked);
            _tryButton.onClick.RemoveListener(OnTryClicked);
            _showAllChestsButton.onClick.RemoveListener(ShowAllChests);
        }

        private void ResetScreen()
        {
            DisableAllChests();
            _openedChests.Clear();
            _totalWin = 0;

            _totalWinPanel.SetActive(false);
            _topText.text = InitialTopText;
        }

        private void OnChestOpened(Chest chest)
        {
            _openedChests.Add(chest);
            UpdateTopText();

            if (_openedChests.Count >= _chestsToOpen)
            {
                EndChestSelection();
            }
        }

        private void EndChestSelection()
        {
            DisableChestInteractions();
            _topText.text = GameOverTopText;

            _totalWin = _openedChests.Sum(chest => chest.CurrentAmount);
            _totalWinText.text = "+" + _totalWin;

            _totalWinPanel.SetActive(true);
            ManageButtonsState(showGetButton: false, showTryButton: true, showShowAllButton: true);
        }

        private void OnTryClicked()
        {
            if (PlayerBalanceController.CurrentBalance < TryCost)
            {
                _notEnoughBalancePopup.SetActive(true);
                return;
            }

            PlayerBalanceController.DecreaseBalance(TryCost);

            _chestsToOpen += 1;
            ActivateRemainingChests();
            UpdateTopText();

            _totalWinPanel.SetActive(false);
            ManageButtonsState(showGetButton: false, showTryButton: false, showShowAllButton: false);
        }

        private void ShowAllChests()
        {
            foreach (var chest in _chests.Where(chest => !_openedChests.Contains(chest)))
            {
                chest.ShowWin();
            }

            ManageButtonsState(showGetButton: true, showTryButton: false, showShowAllButton: false);
        }

        private void OnGetButtonClicked()
        {
            PlayerBalanceController.IncreaseBalance(_totalWin);
            DisableScreen();
        }

        private void SetBigScoreChest()
        {
            var bigScoreChest = _chests[Random.Range(0, _chests.Count)];
            bigScoreChest.CurrentAmount = _bigScore;
        }

        private void ActivateRemainingChests()
        {
            foreach (var chest in _chests.Where(chest => !_openedChests.Contains(chest)))
            {
                chest.EnableInteractions();
            }
        }

        private void DisableAllChests()
        {
            foreach (var chest in _chests)
            {
                chest.gameObject.SetActive(false);
            }
        }

        private void DisableChestInteractions()
        {
            foreach (var chest in _chests)
            {
                chest.DisableInteractions();
            }
        }

        private int GetRandomCoinValue()
        {
            return _availableCoins[Random.Range(0, _availableCoins.Length)];
        }

        private void UpdateTopText()
        {
            _topText.text = string.Format(PickTextTemplate, _chestsToOpen - _openedChests.Count);
        }

        private void ManageButtonsState(bool showGetButton, bool showTryButton, bool showShowAllButton)
        {
            _getButton.gameObject.SetActive(showGetButton);
            _tryButton.gameObject.SetActive(showTryButton);
            _showAllChestsButton.gameObject.SetActive(showShowAllButton);
        }
    }
}