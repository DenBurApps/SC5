using System;
using LuckTest;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MainScreen
{
    public class LuckTestButton : MonoBehaviour
    {
        private const string LastTryTimeKey = "LastLuckTestTime";
        private const float CooldownDuration = 300f;
        private const int TestLuckCost = 300;
        
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _timerText;
        [SerializeField] private GameObject _timerHolder;
        [SerializeField] private LuckTestScreen _luckTestScreen;
        [SerializeField] private GameObject _notEnoughScreen;
    
        private float _remainingTime;
        private bool _isOnCooldown;

        public event Action LuckTestOpened;

        private void Start()
        {
            _timerText.gameObject.SetActive(false);
            _timerHolder.gameObject.SetActive(false);
            CheckCooldown();
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(OnButtonClick);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnButtonClick);
        }

        private void Update()
        {
            if (_isOnCooldown)
            {
                UpdateTimer();
            }
        }

        private void CheckCooldown()
        {
            if (PlayerPrefs.HasKey(LastTryTimeKey))
            {
                string lastTryStr = PlayerPrefs.GetString(LastTryTimeKey);
                if (DateTime.TryParse(lastTryStr, out DateTime lastTryTime))
                {
                    TimeSpan timeSinceLastTry = DateTime.Now - lastTryTime;
                
                    if (timeSinceLastTry.TotalSeconds < CooldownDuration)
                    {
                        _remainingTime = CooldownDuration - (float)timeSinceLastTry.TotalSeconds;
                        StartCooldown();
                    }
                }
            }
        }

        private void OnButtonClick()
        {
            if (PlayerDataController.CurrentBalance < TestLuckCost)
            {
                _notEnoughScreen.SetActive(true);
                return;
            }
            
            PlayerDataController.DecreaseBalance(TestLuckCost);
            _luckTestScreen.EnableScreen();
            
            
            PlayerPrefs.SetString(LastTryTimeKey, DateTime.Now.ToString());
            PlayerPrefs.Save();
        
            _remainingTime = CooldownDuration;
            StartCooldown();
            
            LuckTestOpened?.Invoke();
        }

        private void StartCooldown()
        {
            _isOnCooldown = true;
            _button.gameObject.SetActive(false);
            _timerHolder.gameObject.SetActive(true);
            _timerText.gameObject.SetActive(true);
        }

        private void UpdateTimer()
        {
            _remainingTime -= Time.deltaTime;
        
            if (_remainingTime <= 0)
            {
                EndCooldown();
                return;
            }

            int minutes = Mathf.FloorToInt(_remainingTime / 60);
            int seconds = Mathf.FloorToInt(_remainingTime % 60);
            _timerText.text = $"{minutes:00}:{seconds:00}";
        }

        private void EndCooldown()
        {
            _isOnCooldown = false;
            _button.gameObject.SetActive(true);
            _timerText.gameObject.SetActive(false);
            _timerHolder.gameObject.SetActive(false);
            
            if (PlayerPrefs.HasKey(LastTryTimeKey))
            {
                PlayerPrefs.DeleteKey(LastTryTimeKey);
                PlayerPrefs.Save();
            }
        }
    }
}