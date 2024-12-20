using System;
using System.Collections.Generic;
using System.Linq;
using Games;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Games
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private Color _decimalColor;
        [SerializeField] private LinesInputController _linesInputController;
        [SerializeField] private TMP_Text _playerBalance;
        [SerializeField] private TMP_Text _winAmount;
        [SerializeField] private WinAmountPlane _winAmountPlane;
        [SerializeField] private BetController _betInputer;
        [SerializeField] private Button _spinButton;
        [SerializeField] private GameObject _animatedSpinButton;
        [SerializeField] private Button _homeButton;
        [SerializeField] private JackpotPlane _jackpotPlane;
        [SerializeField] private GameObject _notEnoughPopup;
        [SerializeField] private List<WinLine> _winLines;
        [SerializeField] private MultiplierManager _multiplierManager;
        [SerializeField] private GameObject _rules;

        [SerializeField] private List<SlotColumn> _reels;

        private int _currentBet;
        private int _currentLines;
        private int _reelsStopped;

        private void OnEnable()
        {
            PlayerBalanceController.OnBalanceChanged += UpdateBalanceText;
            _spinButton.onClick.AddListener(OnSpinClicked);
            _homeButton.onClick.AddListener(ReturnToMainScene);

            foreach (var reel in _reels)
            {
                reel.OnStartSpinning += OnReelStartSpin;
                reel.OnStoppedSpinning += OnReelStopped;
            }
        }

        private void OnDisable()
        {
            PlayerBalanceController.OnBalanceChanged -= UpdateBalanceText;
            _spinButton.onClick.RemoveListener(OnSpinClicked);
            _homeButton.onClick.RemoveListener(ReturnToMainScene);

            foreach (var reel in _reels)
            {
                reel.OnStartSpinning -= OnReelStartSpin;
                reel.OnStoppedSpinning -= OnReelStopped;
            }
        }

        private void Start()
        {
            _notEnoughPopup.SetActive(false);
            _jackpotPlane.gameObject.SetActive(false);
            UpdateLinesCount(1);
            UpdateWinText(0);
            _rules.SetActive(true);
            _spinButton.gameObject.SetActive(true);
            _animatedSpinButton.SetActive(false);
            _betInputer.ToggleButtons(true);
            UpdateBalanceText(PlayerBalanceController.CurrentBalance);
        }

        public void OnSpinClicked()
        {
            _currentBet = _betInputer.CurrentBet;

            int totalBetCost = _currentBet * _currentLines;

            if (PlayerBalanceController.CurrentBalance < totalBetCost)
            {
                _notEnoughPopup.SetActive(true);
                return;
            }

            PlayerBalanceController.DecreaseBalance(totalBetCost);

            Debug.Log("clicked");
            StartSpin();
        }

        private void ReturnToMainScene()
        {
            SceneManager.LoadScene("MainScene");
        }

        private void UpdateLinesCount(int count)
        {
            _currentLines = count;
        }

        private void OnReelStartSpin()
        {
            _spinButton.gameObject.SetActive(false);
            _animatedSpinButton.SetActive(true);
            _betInputer.ToggleButtons(false);
        }

        private void OnReelStopped()
        {
            _reelsStopped++;
            if (_reelsStopped >= _reels.Count)
            {
                VerifyResults();
                _reelsStopped = 0;
            }
        }

        private void UpdateBalanceText(int value)
        {
            string decimalColorHex = ColorUtility.ToHtmlStringRGBA(_decimalColor);
            
            _playerBalance.text = $"{value}<color=#{decimalColorHex}>.00</color>";
        }

        private void StartSpin()
        {
            DisableAllStars();
            foreach (var reel in _reels)
            {
                reel.SpinReel();
            }
        }

        private void VerifyResults()
        {
            var linesToCheck = _winLines.Take(_currentLines).ToList();
            int winAmount = 0;

            for (int i = 0; i < linesToCheck.Count; i++)
            {
                var lineWin = CalculateLineWin(linesToCheck[i].ItemHolders);
                winAmount += lineWin;
            }

            if (winAmount > 0)
            {
                PlayerBalanceController.IncreaseBalance(winAmount);
            }

            UpdateWinText(winAmount);
            _spinButton.gameObject.SetActive(true);
            _animatedSpinButton.SetActive(false);
            _betInputer.ToggleButtons(true);
        }

        private int CalculateLineWin(List<SlotItemHolder> itemHolders)
        {
            int win = 0;
            var groupedItems = itemHolders.GroupBy(holder => holder.SlotItem.Type);
            var nonEmptyHolders = itemHolders.Where(holder => holder.SlotItem.Type != Type.Empty).ToList();

            if (itemHolders.Count(holder => holder.SlotItem.Type == Type.Empty) >= 2)
            {
                var firstNonEmpty = nonEmptyHolders.FirstOrDefault();
                if (firstNonEmpty != null)
                {
                    firstNonEmpty.ToggleFlashAnimation(true);
                    win += _currentBet * _multiplierManager.GetMultiplier(firstNonEmpty.SlotItem.Type, 1);
                    Debug.Log(firstNonEmpty.SlotItem.Type);
                    Debug.Log(_multiplierManager.GetMultiplier(firstNonEmpty.SlotItem.Type, 1));
                    Debug.Log(win);
                }

                return win;
            }

            if (itemHolders.Any(holder => holder.SlotItem.Type == Type.Empty))
            {
                var firstNonEmpty = nonEmptyHolders.FirstOrDefault();
                if (firstNonEmpty != null)
                {
                    var matchingSecond = nonEmptyHolders.FirstOrDefault(holder =>
                        holder.SlotItem.Type == firstNonEmpty.SlotItem.Type && holder != firstNonEmpty);
                    
                    if (matchingSecond != null)
                    {
                        firstNonEmpty.ToggleFlashAnimation(true);
                        matchingSecond.ToggleFlashAnimation(true);
                        win += _currentBet * _multiplierManager.GetMultiplier(firstNonEmpty.SlotItem.Type, 2);

                        Debug.Log(firstNonEmpty.SlotItem.Type);
                        Debug.Log(_multiplierManager.GetMultiplier(firstNonEmpty.SlotItem.Type, 1));

                        Debug.Log(matchingSecond.SlotItem.Type);
                        Debug.Log(_multiplierManager.GetMultiplier(matchingSecond.SlotItem.Type, 1));
                        Debug.Log(win);
                    }
                }

                return win;
            }

            foreach (var group in groupedItems)
            {
                if (group.Count() >= 3)
                {
                    foreach (var holder in group)
                    {
                        holder.ToggleFlashAnimation(true);
                    }

                    win += _currentBet * _multiplierManager.GetMultiplier(group.Key, group.Count());
                    
                    Debug.Log(group.Key);
                    Debug.Log(group.Count());
                    Debug.Log(win);

                    if (group.Key == Type.Bar && group.Count() >= 3)
                    {
                        _jackpotPlane.Enable(win);
                    }
                }
            }

            return win;
        }


        private void UpdateWinText(int win)
        {
            if (win > 0)
            {
                _winAmountPlane.Enable(win);
            }
            else
            {
                _winAmountPlane.Disable();
            }

            string decimalColorHex = ColorUtility.ToHtmlStringRGBA(_decimalColor);
            
            _winAmount.text = $"{win}<color=#{decimalColorHex}>.00</color>";
        }

        private void DisableAllStars()
        {
            foreach (var reel in _reels)
            {
                reel.DisableAllFlashAnimations();
            }
        }
    }
}

[Serializable]
public class WinLine
{
    public List<SlotItemHolder> ItemHolders;
}