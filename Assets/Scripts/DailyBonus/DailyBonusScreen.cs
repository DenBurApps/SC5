using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace DailyBonus
{
    public class DailyBonusScreen : MonoBehaviour
    {
        private const int MaxChestsToOpen = 3;
        private const string InitialTopText = "Pick 3 chests";
        private const string GameOverTopText = "Trying is over";
        
        [SerializeField] private List<Chest> _chests;
        [SerializeField] private GameObject _totalWinPanel;
        [SerializeField] private TMP_Text _totalWinText;
        [SerializeField] private TMP_Text _topText;
        [SerializeField] private Button _getButton;
        [SerializeField] private CanvasGroup _canvasGroup;
        
        private readonly int[] _availableCoins = { 5, 15, 25, 35, 50, 75, 100 };
        private int _totalWin;
        private List<Chest> _openedChests = new List<Chest>();

        public event Action DailyBonusCollected;

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

            _getButton.onClick.AddListener(OnGetButtonClicked);
        }

        private void CleanupScreen()
        {
            foreach (var chest in _chests)
            {
                chest.Opened -= OnChestOpened;
            }

            _getButton.onClick.RemoveListener(OnGetButtonClicked);
        }

        private void ResetScreen()
        {
            DisableAllChests();
            _openedChests.Clear();
            _totalWin = 0;

            _topText.text = InitialTopText;
            _totalWinPanel.SetActive(false);
        }

        private void OnChestOpened(Chest chest)
        {
            _openedChests.Add(chest);

            if (_openedChests.Count >= MaxChestsToOpen)
            {
                EndChestSelection();
            }
        }

        private void EndChestSelection()
        {
            MakeChestsUninteractive();
            _topText.text = GameOverTopText;

            _totalWin = _openedChests.Sum(chest => chest.CurrentAmount);
            _totalWinText.text = "+" + _totalWin.ToString();

            _totalWinPanel.SetActive(true);
        }

        private void MakeChestsUninteractive()
        {
            foreach (var chest in _chests)
            {
                chest.DisableInteractions();
            }
        }

        private void DisableAllChests()
        {
            foreach (var chest in _chests)
            {
                chest.gameObject.SetActive(false);
            }
        }

        private int GetRandomCoinValue()
        {
            int index = Random.Range(0, _availableCoins.Length);
            return _availableCoins[index];
        }

        private void OnGetButtonClicked()
        {
            PlayerBalanceController.IncreaseBalance(_totalWin);
            DailyBonusCollected?.Invoke();
            DisableScreen();
        }
    }
}
