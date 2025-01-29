using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

namespace Games
{
    public class GameController : MonoBehaviour
    {
        private const int MinMatchingSymbols = 3;
        private const int MaxEmptySlots = 2;

        [Header("UI References")]
        [SerializeField] private Color _decimalColor;

        [SerializeField] private TMP_Text _playerBalance;
        [SerializeField] private TMP_Text _winAmount;
        [SerializeField] private Button _spinButton;
        [SerializeField] private GameObject _animatedSpinButton;
        [SerializeField] private Button _homeButton;
        [SerializeField] private GameObject _notEnoughPopup;
        [SerializeField] private GameObject _rules;

        [Header("Game Components")]
        [SerializeField] private List<SlotColumn> _reels;

        [SerializeField] private SlotReelsHolder _slotReelsHolder;
    
        [SerializeField] private List<WinLine> _winLines;
        [SerializeField] private WinAmountPlane _winAmountPlane;
        [SerializeField] private JackpotPlane _jackpotPlane;
        [SerializeField] private LinesInputController _linesInputController;
        [SerializeField] private BetController _betInputer;
        [SerializeField] private MultiplierManager _multiplierManager;
        [SerializeField] private ParticleSystem[] _particles;
        [SerializeField] private GameType _gameType;
        [SerializeField] private AudioSource _winSound;
        [SerializeField] private AudioSource _spinSound;
        
        private Queue<ParticleSystem> _particlePool;

        private int _currentBet;
        private int _currentLines;
        private bool _isSpinning;

        private void Awake()
        {
            _particlePool = new Queue<ParticleSystem>(_particles);
            InitializeGameState();
        }

        private void OnEnable() => SubscribeToEvents();
        private void OnDisable() => UnsubscribeFromEvents();

        private void InitializeGameState()
        {
            _notEnoughPopup.SetActive(false);
            _jackpotPlane.gameObject.SetActive(false);
            _rules.SetActive(true);
            _spinButton.gameObject.SetActive(true);
            _animatedSpinButton.SetActive(false);
            
            UpdateLinesCount(1);
            UpdateWinText(0);
            UpdateUIState(true);
            UpdateBalanceText(PlayerDataController.CurrentBalance);
            DisableAllParticles();
        }

        private void SubscribeToEvents()
        {
            PlayerDataController.OnBalanceChanged += UpdateBalanceText;
            _spinButton.onClick.AddListener(OnSpinClicked);
            _homeButton.onClick.AddListener(ReturnToMainScene);
            _slotReelsHolder.StartedSpin += OnReelStartSpin;
            _slotReelsHolder.StopedSpin += OnReelStopped;
        }

        private void UnsubscribeFromEvents()
        {
            PlayerDataController.OnBalanceChanged -= UpdateBalanceText;
            _spinButton.onClick.RemoveListener(OnSpinClicked);
            _homeButton.onClick.RemoveListener(ReturnToMainScene);
            _slotReelsHolder.StartedSpin -= OnReelStartSpin;
            _slotReelsHolder.StopedSpin -= OnReelStopped;
        }

        public void OnSpinClicked()
        {
            if (_isSpinning) return;

            int totalBetCost = CalculateTotalBet();
            if (!ValidateBet(totalBetCost)) return;

            PlayerDataController.DecreaseBalance(totalBetCost);
            StartSpin();
        }

        private int CalculateTotalBet() => _betInputer.CurrentBet * _currentLines;

        private bool ValidateBet(int totalBetCost)
        {
            if (PlayerDataController.CurrentBalance >= totalBetCost) return true;
            
            _notEnoughPopup.SetActive(true);
            return false;
        }

        private void StartSpin()
        {
            _isSpinning = true;
            _spinSound.Play();
            _currentBet = _betInputer.CurrentBet;
            DisableAllStars();
            _slotReelsHolder.StartSpinning();
        }

        private void OnReelStartSpin()
        {
            UpdateUIState(false);
        }

        private void OnReelStopped()
        {
            VerifyResults();
            _isSpinning = false;
        }

        private void VerifyResults()
        {
            var linesToCheck = _winLines.Take(_currentLines).ToList();
            int totalWin = linesToCheck.Sum(line => CalculateLineWin(line.ItemHolders));

            if (totalWin > 0)
            {
                PlayerDataController.IncreaseBalance(totalWin);
                WinningsManager.AddWin(_gameType, totalWin);
                _winSound.Play();
            }

            UpdateWinText(totalWin);
            UpdateUIState(true);
        }


        private int CalculateLineWin(List<SlotItemHolder> itemHolders)
        {
            var nonEmptyHolders = itemHolders.Where(holder => holder.SlotItem.Type != Type.Empty).ToList();
            int emptyCount = itemHolders.Count - nonEmptyHolders.Count;

            if (emptyCount >= MaxEmptySlots)
                return CalculateSingleSymbolWin(nonEmptyHolders);

            if (emptyCount > 0)
                return CalculateTwoMatchingSymbolsWin(nonEmptyHolders);

            return CalculateFullLineWin(itemHolders);
        }

        private int CalculateSingleSymbolWin(List<SlotItemHolder> nonEmptyHolders)
        {
            var firstSymbol = nonEmptyHolders.FirstOrDefault();
            if (firstSymbol == null) return 0;

            HighlightWinningSymbol(firstSymbol);
            return _currentBet * _multiplierManager.GetMultiplier(firstSymbol.SlotItem.Type, 1);
        }

        private int CalculateTwoMatchingSymbolsWin(List<SlotItemHolder> nonEmptyHolders)
        {
            var firstSymbol = nonEmptyHolders.FirstOrDefault();
            if (firstSymbol == null) return 0;

            var matchingSymbol = nonEmptyHolders.FirstOrDefault(holder => 
                holder.SlotItem.Type == firstSymbol.SlotItem.Type && holder != firstSymbol);

            if (matchingSymbol == null) return 0;

            HighlightWinningSymbol(firstSymbol);
            HighlightWinningSymbol(matchingSymbol);
            return _currentBet * _multiplierManager.GetMultiplier(firstSymbol.SlotItem.Type, 2);
        }

        private int CalculateFullLineWin(List<SlotItemHolder> itemHolders)
        {
            var groups = itemHolders.GroupBy(holder => holder.SlotItem.Type);
            int totalWin = 0;

            foreach (var group in groups)
            {
                if (group.Count() >= MinMatchingSymbols)
                {
                    foreach (var holder in group)
                    {
                        HighlightWinningSymbol(holder);
                    }

                    totalWin += _currentBet * _multiplierManager.GetMultiplier(group.Key, group.Count());

                    if (group.Key == Type.Bar && group.Count() >= MinMatchingSymbols)
                    {
                        _jackpotPlane.Enable(totalWin);
                    }
                }
            }

            return totalWin;
        }

        private void HighlightWinningSymbol(SlotItemHolder holder)
        {
            holder.ToggleFlashAnimation(true);
            SpawnParticleAtPosition(holder.transform.position);
        }

        private void UpdateUIState(bool enabled)
        {
            _spinButton.gameObject.SetActive(enabled);
            _animatedSpinButton.SetActive(!enabled);
            _betInputer.ToggleButtons(enabled);
        }

        private void UpdateLinesCount(int count) => _currentLines = count;

        private void UpdateBalanceText(int value)
        {
            string decimalColorHex = ColorUtility.ToHtmlStringRGBA(_decimalColor);
            _playerBalance.text = $"{value}<color=#{decimalColorHex}>.00</color>";
        }

        private void UpdateWinText(int win)
        {
            if (win > 0)
                _winAmountPlane.Enable(win);
            else
                _winAmountPlane.Disable();

            string decimalColorHex = ColorUtility.ToHtmlStringRGBA(_decimalColor);
            _winAmount.text = $"{win}<color=#{decimalColorHex}>.00</color>";
        }

        private void DisableAllStars() => _reels.ForEach(reel => reel.DisableAllFlashAnimations());

        private void DisableAllParticles()
        {
            foreach (var particle in _particles)
            {
                if (particle.IsAlive()) particle.Stop();
            }
        }

        private void SpawnParticleAtPosition(Vector3 position)
        {
            var particle = GetAvailableParticle();
            if (particle == null) return;
            
            particle.transform.position = position;
            particle.Play();
        }

        private ParticleSystem GetAvailableParticle() => 
            _particles.FirstOrDefault(p => !p.IsAlive());

        private void ReturnToMainScene() =>
            SceneLoader.LoadScene("MainScene");
    }

    [Serializable]
    public class WinLine
    {
        public List<SlotItemHolder> ItemHolders;
    }
}