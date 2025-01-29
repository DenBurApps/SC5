using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WinningsScreen;

public class WinPlane : MonoBehaviour
{
    [SerializeField] private Image _gameIcon;
    [SerializeField] private TMP_Text _winAmountText;
    [SerializeField] private TMP_Text _positionText;
    
    [SerializeField] private Sprite _bambooFortuneIcon;
    [SerializeField] private Sprite _spinFestivalIcon;
    [SerializeField] private Sprite _chinaSlotsIcon;
    [SerializeField] private Sprite _jungleMystiqueIcon;

    public void Setup(WinData data, int position)
    {
        _gameIcon.sprite = GetGameIcon(data.GameType);
        _winAmountText.text = $"{data.Win:N0}";
        _positionText.text = position.ToString();
        gameObject.SetActive(true);
    }

    private Sprite GetGameIcon(GameType gameType)
    {
        return gameType switch
        {
            GameType.BambooFortune => _bambooFortuneIcon,
            GameType.SpinFestival => _spinFestivalIcon,
            GameType.ChinaSlots => _chinaSlotsIcon,
            GameType.JungleMystique => _jungleMystiqueIcon,
            _ => null
        };
    }
}
