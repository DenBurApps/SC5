using System;
using LuckTest;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using ColorUtility = UnityEngine.ColorUtility;

namespace ShopScreen
{
    public class ShopScreen : MonoBehaviour
    {
        private const int TestLuckCost = 400;
        
        [SerializeField] private Color _decimalColor;
        [SerializeField] private TMP_Text _balanceText;
        [SerializeField] private Button _luckTextButton;
        [SerializeField] private GameObject _notEnoughPlane;
        [SerializeField] private LuckTestScreen _luckTestScreen;
        [SerializeField] private CanvasGroup _canvasGroup;

        private void OnEnable()
        {
            UpdateBalance(PlayerBalanceController.CurrentBalance);
            _notEnoughPlane.SetActive(false);
            
            _luckTextButton.onClick.AddListener(OnTestLuckClicked);
            PlayerBalanceController.OnBalanceChanged += UpdateBalance;
        }

        private void OnDisable()
        {
            _luckTextButton.onClick.RemoveListener(OnTestLuckClicked);
            PlayerBalanceController.OnBalanceChanged -= UpdateBalance;
        }

        private void Start()
        {
            DisableScreen();
        }

        public void EnableScreen()
        {
            _canvasGroup.alpha = 1;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        public void DisableScreen()
        {
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        private void OnTestLuckClicked()
        {
            if (PlayerBalanceController.CurrentBalance < TestLuckCost)
            {
                _notEnoughPlane.SetActive(true);
                return;
            }
            
            PlayerBalanceController.DecreaseBalance(TestLuckCost);
            _luckTestScreen.EnableScreen();
            DisableScreen();
        }
        
        private void UpdateBalance(int balance)
        {
            string decimalColorHex = ColorUtility.ToHtmlStringRGBA(_decimalColor);
            
            _balanceText.text = $"{balance}<color=#{decimalColorHex}>.00</color>";
        }
    }
}
