using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Games
{
    public class MultilinesGameController : MonoBehaviour
    {
        private const int MinMatchingSymbols = 3;
        private const int MaxMatchingSymbols = 5;

        [Header("UI References")]
        [SerializeField] private TMP_Text _freeSpinsCountText;

        [SerializeField] private Color _decimalColor;
        [SerializeField] private TMP_Text _playerBalance;
        [SerializeField] private TMP_Text _winAmount;
        [SerializeField] private TMP_Text _totalBetText;
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
        [SerializeField] private ParticleSystem _freeSpinsGlow;
        [SerializeField] private GameType _gameType;
        [SerializeField] private AudioSource _winSound;
        [SerializeField] private AudioSource _spinSound;

        private Queue<ParticleSystem> _particlePool;
        
        private int _currentBet;
        private int _currentLines;
        private int _playerFreeSpinsCount;
        private bool _isSpinning;

        private void Awake()
        {
            _particlePool = new Queue<ParticleSystem>(_particles);
        }

        private void Start() => InitializeGameState();

        private void OnEnable() => SubscribeToEvents();

        private void OnDisable() => UnsubscribeFromEvents();

        private void InitializeGameState()
        {
            _notEnoughPopup.SetActive(false);
            _jackpotPlane.gameObject.SetActive(false);
            _rules.SetActive(true);
            _spinButton.gameObject.SetActive(true);
            _animatedSpinButton.SetActive(false);

            UpdateBalanceText(PlayerDataController.CurrentBalance);
            OnFreeSpinsActive(PlayerDataController.FreeSpinsCount);
            UpdateLinesCount(_linesInputController.CurrentLines);
            UpdateWinText(0);
            UpdateTotalBetText();

            if (_playerFreeSpinsCount <= 0)
            {
                _betInputer.ToggleButtons(true);
            }

            DisableAllParticles();
        }

        private void SubscribeToEvents()
        {
            PlayerDataController.OnBalanceChanged += UpdateBalanceText;
            PlayerDataController.OnFreeSpinsChanged += OnFreeSpinsActive;
            _spinButton.onClick.AddListener(OnSpinClicked);
            _homeButton.onClick.AddListener(ReturnToMainScene);
            _linesInputController.LinesChanged += UpdateLinesCount;
            _betInputer.BetChanged += UpdateTotalBetText;
            _slotReelsHolder.StartedSpin += OnReelStartSpin;
            _slotReelsHolder.StopedSpin += OnReelStopped;
        }

        private void UnsubscribeFromEvents()
        {
            PlayerDataController.OnBalanceChanged -= UpdateBalanceText;
            PlayerDataController.OnFreeSpinsChanged -= OnFreeSpinsActive;
            _spinButton.onClick.RemoveListener(OnSpinClicked);
            _homeButton.onClick.RemoveListener(ReturnToMainScene);
            _linesInputController.LinesChanged -= UpdateLinesCount;
            _betInputer.BetChanged -= UpdateTotalBetText;
            _slotReelsHolder.StartedSpin -= OnReelStartSpin;
            _slotReelsHolder.StopedSpin -= OnReelStopped;
        }

        public void OnSpinClicked()
        {
            if (_isSpinning) return;

            _currentBet = _betInputer.CurrentBet;
            int totalBetCost = CalculateTotalBet();
            
            if (!ValidateBet(totalBetCost)) return;

            PlayerDataController.DecreaseBalance(totalBetCost);
            _linesInputController.DisableAllLines();
            StartSpin();
        }

        private int CalculateTotalBet()
        {
            int totalBet = _currentBet * _currentLines;
            UpdateTotalBetText();
            return totalBet;
        }

        private bool ValidateBet(int totalBetCost)
        {
            if (PlayerDataController.CurrentBalance >= totalBetCost) return true;
            
            _notEnoughPopup.SetActive(true);
            return false;
        }

        private void OnFreeSpinsActive(int count)
        {
            _playerFreeSpinsCount = count;
            
            if (count > 0)
            {
                EnableFreeSpinsMode(count);
                return;
            }

            DisableFreeSpinsMode();
        }

        private void EnableFreeSpinsMode(int count)
        {
            _freeSpinsCountText.enabled = true;
            _freeSpinsCountText.text = count.ToString();
            _winAmountPlane.EnableWithSpins(count);
            _freeSpinsGlow.Play();

            _betInputer.EnableFreeSpinsMode();
            _linesInputController.EnableOneLine();
        }

        private void DisableFreeSpinsMode()
        {
            _freeSpinsCountText.enabled = false;
            _freeSpinsGlow.Stop();
            _betInputer.ReturnToDefault();
            _linesInputController.ReturnToDefault();
        }

        private void UpdateTotalBetText()
        {
            _currentBet = _betInputer.CurrentBet;
            _currentLines = _linesInputController.CurrentLines;
            _totalBetText.text = (_currentBet * _currentLines).ToString();
        }

        private void UpdateLinesCount(int count)
        {
            _currentLines = count;
            UpdateTotalBetText();
        }

        private void StartSpin()
        {
            _isSpinning = true;
            _spinSound.Play();
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
            var linesToCheck = _winLines.Take(_currentLines).Distinct().ToList();
            int totalWin = linesToCheck.Sum(line => CalculateLineWin(line.ItemHolders));

            if (totalWin > 0)
            {
                PlayerDataController.IncreaseBalance(totalWin);
                WinningsManager.AddWin(_gameType, totalWin);
                _winSound.Play();
            }

            UpdateWinText(totalWin);
            UpdateUIState(true);
            _linesInputController.RestoreLines();
        }

        private int CalculateLineWin(List<SlotItemHolder> itemHolders)
        {
            int win = 0;
            bool wildUsed = false;
            
            var groupedItems = itemHolders
                .Where(holder => holder.SlotItem.Type != Type.Wild)
                .GroupBy(holder => holder.SlotItem.Type)
                .ToList();

            int wildCount = itemHolders.Count(holder => holder.SlotItem.Type == Type.Wild);

            foreach (var group in groupedItems)
            {
                var type = group.Key;
                int count = group.Count();
                
                if (wildCount > 0 && !wildUsed)
                {
                    count++;
                    wildUsed = true;
                }

                if (count >= MinMatchingSymbols)
                {
                    HandleSpecialSymbols(type, count);
                    win += ProcessWinningCombination(group, type, count, wildUsed, itemHolders);
                    
                    if (wildUsed) break;
                }
            }

            return win;
        }

        private void HandleSpecialSymbols(Type type, int count)
        {
            if (type != Type.Clover) return;
            
            int freeSpins = Mathf.Min(count, MaxMatchingSymbols);
            PlayerDataController.AddFreeSpins(freeSpins);
            _winAmountPlane.EnableWithSpins(freeSpins);
            _freeSpinsGlow.Play();
        }

        private int ProcessWinningCombination(IGrouping<Type, SlotItemHolder> group, Type type, int count, bool wildUsed, List<SlotItemHolder> itemHolders)
        {
            int finalCount = Mathf.Min(count, MaxMatchingSymbols);
            
            var contributingItems = group
                .Take(finalCount - (wildUsed ? 1 : 0))
                .Concat(itemHolders.Where(holder => holder.SlotItem.Type == Type.Wild)
                    .Take(wildUsed ? 1 : 0));

            foreach (var holder in contributingItems)
            {
                HighlightWinningSymbol(holder);
            }

            int win = _currentBet * _multiplierManager.GetMultiplier(type, finalCount);

            if (type == Type.Bar && finalCount >= MinMatchingSymbols)
            {
                _jackpotPlane.Enable(win);
            }

            return win;
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

            if (_playerFreeSpinsCount <= 0)
            {
                _betInputer.ToggleButtons(enabled);
            }
        }

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
}