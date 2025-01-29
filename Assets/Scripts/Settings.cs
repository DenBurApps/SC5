using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_IOS
using UnityEngine.iOS;
#endif

public class Settings : MonoBehaviour
{
    private const string SoundPrefsKey = "SoundEnabled";
    
    [SerializeField] private GameObject _settingsCanvas;
    [SerializeField] private GameObject _privacyCanvas;
    [SerializeField] private GameObject _termsCanvas;
    [SerializeField] private GameObject _contactCanvas;
    [SerializeField] private GameObject _versionCanvas;
    [SerializeField] private TMP_Text _versionText;
    
    [Header("Sound Settings")]
    [SerializeField] private Button _soundToggleButton;

    [SerializeField] private TMP_Text _soundButtonText;
    [SerializeField] private Color _soundOnColor = Color.red;
    [SerializeField] private Color _soundOffColor = Color.green;

    private string _version = "Application version:\n";

    private void Awake()
    {
        _settingsCanvas.SetActive(false);
        _privacyCanvas.SetActive(false);
        _termsCanvas.SetActive(false);
        _contactCanvas.SetActive(false);
        _versionCanvas.SetActive(false);
        SetVersion();
        InitializeSoundSettings();
    }

    private void InitializeSoundSettings()
    {
        if (_soundToggleButton != null && _soundButtonText != null)
        {
            bool isSoundEnabled = PlayerPrefs.GetInt(SoundPrefsKey, 1) == 1;
            
            AudioListener.pause = !isSoundEnabled;
            UpdateSoundButtonVisuals(isSoundEnabled);
            
            _soundToggleButton.onClick.AddListener(ToggleSound);
        }
    }

    private void ToggleSound()
    {
        AudioListener.pause = !AudioListener.pause;
        bool isSoundEnabled = !AudioListener.pause;
        
        PlayerPrefs.SetInt(SoundPrefsKey, isSoundEnabled ? 1 : 0);
        PlayerPrefs.Save();
        
        UpdateSoundButtonVisuals(isSoundEnabled);
    }

    private void UpdateSoundButtonVisuals(bool isSoundEnabled)
    {
        // Update button color
        _soundToggleButton.image.color = isSoundEnabled ? _soundOnColor : _soundOffColor;
        
        // Update text
        _soundButtonText.text = isSoundEnabled ? "Off" : "On";
    }

    private void OnDestroy()
    {
        if (_soundToggleButton != null)
        {
            _soundToggleButton.onClick.RemoveListener(ToggleSound);
        }
    }

    private void SetVersion()
    {
        _versionText.text = _version + Application.version;
    }

    public void ShowSettings()
    {
        _settingsCanvas.SetActive(true);
    }

    public void RateUs()
    {
#if UNITY_IOS
        Device.RequestStoreReview();
#endif
    }
}