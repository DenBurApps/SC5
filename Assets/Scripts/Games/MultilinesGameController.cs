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
        [SerializeField] private TMP_Text _freeSpinsCountText;
        [SerializeField] private Color _decimalColor;
        [SerializeField] private LinesInputController _linesInputController;
        [SerializeField] private TMP_Text _playerBalance;
        [SerializeField] private TMP_Text _winAmount;
        [SerializeField] private TMP_Text _totalBetText;
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
        [SerializeField] private ParticleSystem[] _particles;
        [SerializeField] private ParticleSystem _freeSpinsGlow;

        [SerializeField] private List<SlotColumn> _reels;

        private int _currentBet;
        private int _currentLines;
        private int _reelsStopped;
        private int _playerFreeSpinsCount;

        private void OnEnable()
        {
            PlayerBalanceController.OnBalanceChanged += UpdateBalanceText;
            PlayerBalanceController.OnFreeSpinsChanged += OnFreeSpinsActive;
            _spinButton.onClick.AddListener(OnSpinClicked);
            _homeButton.onClick.AddListener(ReturnToMainScene);
            _linesInputController.LinesChanged += UpdateLinesCount;
            _betInputer.BetChanged += UpdateTotalBetText;

            foreach (var reel in _reels)
            {
                reel.OnStartSpinning += OnReelStartSpin;
                reel.OnStoppedSpinning += OnReelStopped;
            }
        }

        private void OnDisable()
        {
            PlayerBalanceController.OnBalanceChanged -= UpdateBalanceText;
            PlayerBalanceController.OnFreeSpinsChanged -= OnFreeSpinsActive;
            _spinButton.onClick.RemoveListener(OnSpinClicked);
            _homeButton.onClick.RemoveListener(ReturnToMainScene);
            _linesInputController.LinesChanged -= UpdateLinesCount;
            _betInputer.BetChanged -= UpdateTotalBetText;

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
            UpdateBalanceText(PlayerBalanceController.CurrentBalance);
            OnFreeSpinsActive(PlayerBalanceController.FreeSpinsCount);
            UpdateLinesCount(_linesInputController.CurrentLines);
            UpdateWinText(0);
            _rules.SetActive(true);
            _spinButton.gameObject.SetActive(true);
            _animatedSpinButton.SetActive(false);

            if (_playerFreeSpinsCount <= 0)
                _betInputer.ToggleButtons(true);

            UpdateTotalBetText();
        }

        public void OnSpinClicked()
        {
            _currentBet = _betInputer.CurrentBet;

            int totalBetCost = _currentBet * _currentLines;
            UpdateTotalBetText();

            if (PlayerBalanceController.CurrentBalance < totalBetCost)
            {
                _notEnoughPopup.SetActive(true);
                return;
            }

            PlayerBalanceController.DecreaseBalance(totalBetCost);

            Debug.Log("clicked");
            StartSpin();
        }

        private void OnFreeSpinsActive(int count)
        {
            if (count > 0)
            {
                _freeSpinsCountText.enabled = true;
                _freeSpinsCountText.text = count.ToString();
                _winAmountPlane.EnableWithSpins(count);
                _playerFreeSpinsCount = count;
                _freeSpinsGlow.Play();

                _betInputer.EnableFreeSpinsMode();
                _linesInputController.EnableOneLine();
                return;
            }

            _freeSpinsCountText.enabled = false;
            _freeSpinsGlow.Stop();
            _betInputer.ReturnToDefault();
            _linesInputController.ReturnToDefault();
        }

        private void UpdateTotalBetText()
        {
            _currentBet = _betInputer.CurrentBet;
            _currentLines = _linesInputController.CurrentLines;

            int totalBetCost = _currentBet * _currentLines;
            _totalBetText.text = totalBetCost.ToString();
        }

        private void ReturnToMainScene()
        {
            SceneManager.LoadScene("MainScene");
        }

        private void UpdateLinesCount(int count)
        {
            _currentLines = count;
            UpdateTotalBetText();
        }

        private void OnReelStartSpin()
        {
            _spinButton.gameObject.SetActive(false);
            _animatedSpinButton.SetActive(true);

            if (_playerFreeSpinsCount <= 0)
                _betInputer.ToggleButtons(true);
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
            var linesToCheck = _winLines.Take(_currentLines).Distinct().ToList();
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

            if (_playerFreeSpinsCount <= 0)
                _betInputer.ToggleButtons(true);
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
                    count += 1;
                    wildUsed = true;
                }

                if (count >= 3)
                {
                    if (type == Type.Clover)
                    {
                        int freeSpins = Mathf.Min(count, 5);
                        PlayerBalanceController.AddFreeSpins(freeSpins);
                        _winAmountPlane.EnableWithSpins(freeSpins);
                        _freeSpinsGlow.Play();
                        Debug.Log($"Free Spins Awarded: {freeSpins} for Type.Clover combination");
                    }
                    
                    int finalCount = Mathf.Min(count, 5);
                    
                    var contributingItems = group
                        .Take(finalCount - (wildUsed ? 1 : 0))
                        .Concat(itemHolders.Where(holder => holder.SlotItem.Type == Type.Wild)
                            .Take(wildUsed ? 1 : 0));

                    foreach (var holder in contributingItems)
                    {
                        holder.ToggleFlashAnimation(true);
                        SpawnParticleAtPosition(holder.transform.position);
                    }

                    win += _currentBet * _multiplierManager.GetMultiplier(type, finalCount);

                    Debug.Log($"Win with Type.{type} combination: Count = {finalCount}, Wilds Used = {wildUsed}");
                    
                    if (type == Type.Bar && finalCount >= 3)
                    {
                        _jackpotPlane.Enable(win);
                    }
                    
                    if (wildUsed) break;
                }
            }
            
            if (!wildUsed)
            {
                Debug.Log("No valid combination found using Type.Wild.");
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
        
        private void DisableAllParticles()
        {
            foreach (var particle in _particles)
            {
                if (particle.IsAlive()) particle.Stop();
            }
        }
        
        private ParticleSystem GetAvailableParticle()
        {
            var particle = _particles.FirstOrDefault(p => !p.IsAlive());
     
            return particle;
        }

        private void SpawnParticleAtPosition(Vector3 position)
        {
            var particle = GetAvailableParticle();
            if (particle != null)
            {
                particle.transform.position = position;
                particle.Play();
            }
        }
        
    }
}