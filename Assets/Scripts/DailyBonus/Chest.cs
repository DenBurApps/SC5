using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace DailyBonus
{
    public class Chest : MonoBehaviour
    {
        [SerializeField] private TMP_ColorGradient _openGradient;
        [SerializeField] private TMP_ColorGradient _defaultGradient;
        
        [SerializeField] private GameObject _cloud;
        [SerializeField] private Image _chestImage;
        [SerializeField] private TMP_Text _amountText;
        [SerializeField] private Image _coinImage;
        [SerializeField] private Button _button;

        public event Action<Chest> Opened; 
        
        public int CurrentAmount { get; set; }
        
        public void Enable(int coinAmount)
        {
            gameObject.SetActive(true);
            CurrentAmount = coinAmount;
            
            _cloud.gameObject.SetActive(false);
            _chestImage.enabled = true;
            _amountText.colorGradientPreset = _defaultGradient;
            _amountText.enabled = false;
            _coinImage.enabled = false;
            _button.enabled = true;

            _button.onClick.AddListener(OpenChest);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OpenChest);
        }
        
        public void DisableInteractions()
        {
            _button.enabled = false;
        }

        public void ShowWin()
        {
            _button.enabled = false;
            _chestImage.enabled = false;
            _amountText.text = "+" + CurrentAmount.ToString();
            _amountText.enabled = true;
            _coinImage.enabled = true;
        }

        public void EnableInteractions()
        {
            _button.enabled = true;
        }

        private void OpenChest()
        {
            _button.enabled = false;
            _chestImage.enabled = false;
            _cloud.gameObject.SetActive(true);
            _amountText.text = "+" + CurrentAmount.ToString();
            _amountText.colorGradientPreset = _openGradient;
            _amountText.enabled = true;
            _coinImage.enabled = true;
            Opened?.Invoke(this);
        }
    } 
}
