using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MainScreen
{
    public class ConfirmPurchaseScreen : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Button _confirmButton;
        
        public event Action ConfirmClicked;
        
        private void OnEnable()
        {
            _confirmButton.onClick.AddListener(OnConfirmClicked);
        }

        private void OnDisable()
        {
            _confirmButton.onClick.RemoveListener(OnConfirmClicked);
        }

        public void EnableScreen(string gameName)
        {
            gameObject.SetActive(true);
            _text.text = $"Are you sure you want to purchase {gameName}? Spin the reels and discover your luck!";
        }

        private void OnConfirmClicked()
        {
            ConfirmClicked?.Invoke();
            gameObject.SetActive(false);
        }
    }
}
