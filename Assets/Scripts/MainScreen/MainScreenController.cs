using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MainScreen
{
    public class MainScreenController : MonoBehaviour
    {
        [SerializeField] private Color _decimalColor;
        
        [SerializeField] private Button _chinaStreetButton;
        [SerializeField] private Button _bambooFortuneButton;
        [SerializeField] private Button _jungleMystiqueButton;
        [SerializeField] private Button _spinFestivalButton;
        [SerializeField] private TMP_Text _playerBalance;
        [SerializeField] private CanvasGroup _canvasGroup;

        private void Start()
        {
            UpdateBalance(PlayerBalanceController.CurrentBalance);
            EnableScreen();
        }

        private void OnEnable()
        {
            PlayerBalanceController.OnBalanceChanged += UpdateBalance;
            
            _chinaStreetButton.onClick.AddListener(LoadChinaStreet);
            _bambooFortuneButton.onClick.AddListener(LoadBambooFortune);
            _jungleMystiqueButton.onClick.AddListener(LoadJungleMystique);
            _spinFestivalButton.onClick.AddListener(LoadSpinFestival);
        }

        private void OnDisable()
        {
            PlayerBalanceController.OnBalanceChanged -= UpdateBalance;
            
            _chinaStreetButton.onClick.RemoveListener(LoadChinaStreet);
            _bambooFortuneButton.onClick.RemoveListener(LoadBambooFortune);
            _jungleMystiqueButton.onClick.RemoveListener(LoadJungleMystique);
            _spinFestivalButton.onClick.RemoveListener(LoadSpinFestival);
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
        
        private void UpdateBalance(int balance)
        {
            string decimalColorHex = ColorUtility.ToHtmlStringRGBA(_decimalColor);
            
            _playerBalance.text = $"{balance}<color=#{decimalColorHex}>.00</color>";
        }


        private void LoadChinaStreet()
        {
            SceneManager.LoadScene("ChinaStreetScene");
        }

        private void LoadBambooFortune()
        {
            SceneManager.LoadScene("BambooFortuneScene");
        }

        private void LoadJungleMystique()
        {
            SceneManager.LoadScene("JungleMystiqueScene");
        }

        private void LoadSpinFestival()
        {
            SceneManager.LoadScene("SpinFestivalScene");
        }
    }
}
