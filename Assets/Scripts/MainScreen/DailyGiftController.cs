using System;
using System.IO;
using DailyBonus;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace MainScreen
{
    public class DailyGiftController : MonoBehaviour
    {
        [SerializeField] private Button _giftButton;
        [SerializeField] private DailyBonusScreen _dailyBonusScreen;

        private string _savePath;

        public bool GiftCollected { get; private set; }

        private void Awake()
        {
            _savePath = Path.Combine(Application.persistentDataPath, "GiftSave.json");
        }

        private void OnEnable()
        {
            _dailyBonusScreen.DailyBonusCollected += OnGiftCollected;
        }

        private void OnDisable()
        {
            _dailyBonusScreen.DailyBonusCollected -= OnGiftCollected;
        }

        private void Start()
        {
            Load();
        }

        private void SaveGiftData()
        {
            DailyGiftInfoSaver wrapper = new DailyGiftInfoSaver(DateTime.Today);
            string json = JsonConvert.SerializeObject(wrapper, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                DateFormatString = "yyyy-MM-dd"
            });

            if (File.Exists(_savePath) && File.ReadAllText(_savePath) == json) return;

            File.WriteAllText(_savePath, json);
        }

        private void Load()
        {
            try
            {
                if (!File.Exists(_savePath))
                {
                    ResetGift();
                    return;
                }

                var json = File.ReadAllText(_savePath);
                var wrapper = JsonConvert.DeserializeObject<DailyGiftInfoSaver>(json);

                if (wrapper.CollectedGiftDate.Date > DateTime.Today.Date)
                {
                    Debug.LogWarning("Invalid saved date detected. Resetting gift data.");
                    ResetGift();
                    return;
                }

                if (DateTime.Today > wrapper.CollectedGiftDate)
                {
                    ResetGift();
                    return;
                }

                GiftCollected = true;
                ToggleCollectButton();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading gift data: {ex}");
                ResetGift();
            }
        }

        private void ResetGift()
        {
            GiftCollected = false;
            ToggleCollectButton();
        }

        private void OnGiftCollected()
        {
            GiftCollected = true;
            SaveGiftData();
            ToggleCollectButton();
        }

        private void ToggleCollectButton()
        {
            _giftButton.enabled = !GiftCollected;
            UpdateButtonVisual();
        }

        private void UpdateButtonVisual()
        {
            _giftButton.interactable = !GiftCollected;
        }
    }

    [Serializable]
    public class DailyGiftInfoSaver
    {
        public DateTime CollectedGiftDate;

        public DailyGiftInfoSaver(DateTime collectedGiftDate)
        {
            CollectedGiftDate = collectedGiftDate;
        }
    }
}