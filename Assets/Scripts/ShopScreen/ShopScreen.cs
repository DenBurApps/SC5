using System;
using LuckTest;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace ShopScreen
{
    public class ShopScreen : MonoBehaviour
    {
        private const int TestLuckCost = 200;
        
        [SerializeField] private TMP_Text _balanceText;
        [SerializeField] private Button _luckTextButton;
        [SerializeField] private GameObject _notEnoughPlane;
        [SerializeField] private LuckTestScreen _luckTestScreen;

        private void OnEnable()
        {
            _balanceText.text = PlayerBalanceController.CurrentBalance.ToString();
            _notEnoughPlane.SetActive(false);
            
            _luckTextButton.onClick.AddListener(OnTestLuckClicked);
        }

        private void OnDisable()
        {
            _luckTextButton.onClick.RemoveListener(OnTestLuckClicked);
        }

        private void OnTestLuckClicked()
        {
            if (PlayerBalanceController.CurrentBalance < TestLuckCost)
            {
                _notEnoughPlane.SetActive(true);
                return;
            }
            
            PlayerBalanceController.DecreaseBalance(TestLuckCost);
            _luckTestScreen.gameObject.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}
