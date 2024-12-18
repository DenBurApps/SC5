using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Games
{
    public class BetController : MonoBehaviour
    {
        [SerializeField] private Button _plusButton;
        [SerializeField] private Button _minusButton;
        [SerializeField] private TMP_Text _currentBetText;

        private int _currentBet = 10;

        public int CurrentBet => _currentBet;

        private void OnEnable()
        {
            _plusButton.onClick.AddListener(OnPlusButtonClicked);
            _minusButton.onClick.AddListener(OnMinusButtonClicked);
        }

        private void OnDisable()
        {
            _plusButton.onClick.RemoveListener(OnPlusButtonClicked);
            _minusButton.onClick.RemoveListener(OnMinusButtonClicked);
        }

        private void Start()
        {
            UpdateBetUI();
        }

        public void ToggleButtons(bool status)
        {
            _plusButton.interactable = status;
            _minusButton.interactable = status;
        }

        private void OnPlusButtonClicked()
        {
            ChangeBet(10);
        }

        private void OnMinusButtonClicked()
        {
            ChangeBet(-10);
        }

        private void ChangeBet(int amount)
        {
            _currentBet = Mathf.Clamp(_currentBet + amount, 10, PlayerBalanceController.CurrentBalance);

            UpdateBetUI();
        }

        private void UpdateBetUI()
        {
            _currentBetText.text = _currentBet.ToString();

            _plusButton.interactable = _currentBet < PlayerBalanceController.CurrentBalance;
            _minusButton.interactable = _currentBet > 10;
        }
    }
}