using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Games
{
    public class LinesInputController : MonoBehaviour
    {
        [SerializeField] private int _maxLines;
        [SerializeField] private Button _plusButton;
        [SerializeField] private Button _minusButton;
        [SerializeField] private TMP_Text _currentLineText;

        private int _minLines = 1;

        public int MaxLines => _maxLines;
        public int MinLines => _minLines;
        public int CurrentLines { get; private set; }

        public event Action<int> LinesChanged;

        private void Start()
        {
            CurrentLines = MinLines;
            UpdateLineDisplay();
            ToggleButtons();
        }

        private void OnEnable()
        {
            if (_plusButton != null)
                _plusButton.onClick.AddListener(IncreaseLine);
            if (_minusButton != null)
                _minusButton.onClick.AddListener(DecreaseLine);
        }

        private void OnDisable()
        {
            if (_plusButton != null)
                _plusButton.onClick.RemoveListener(IncreaseLine);
            if (_minusButton != null)
                _minusButton.onClick.RemoveListener(DecreaseLine);
        }

        public void EnableOneLine()
        {
            _plusButton.interactable = false;
            _minusButton.interactable = false;
            CurrentLines = 1;
            LinesChanged?.Invoke(CurrentLines);
        }

        public void ReturnToDefault()
        {
            ToggleButtons();
        }

        private void IncreaseLine()
        {
            CurrentLines = Mathf.Clamp(CurrentLines + 1, _minLines, _maxLines);
            UpdateLineDisplay();
            ToggleButtons();
            LinesChanged?.Invoke(CurrentLines);
        }

        private void DecreaseLine()
        {
            CurrentLines = Mathf.Clamp(CurrentLines - 1, _minLines, _maxLines);
            UpdateLineDisplay();
            ToggleButtons();
            LinesChanged?.Invoke(CurrentLines);
        }

        private void UpdateLineDisplay()
        {
            if (_currentLineText != null)
            {
                _currentLineText.text = CurrentLines.ToString();
            }
        }

        private void ToggleButtons()
        {
            if (_plusButton != null)
                _plusButton.interactable = CurrentLines < _maxLines;

            if (_minusButton != null)
                _minusButton.interactable = CurrentLines > _minLines;
        }
    }
}
