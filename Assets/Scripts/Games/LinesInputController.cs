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
        }

        private void OnEnable()
        {
            if (_plusButton && _minusButton != null)
            {
                _plusButton.onClick.AddListener(IncreaseLine);
                _minusButton.onClick.AddListener(DecreaseLine);
            }
        }

        private void OnDisable()
        {
            if (_plusButton && _minusButton != null)
            {
                _plusButton.onClick.RemoveListener(IncreaseLine);
                _minusButton.onClick.RemoveListener(DecreaseLine);
            }
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
            ToggleButtons();
        }

        private void ToggleButtons()
        {
            int activeLines = 0;

            _minusButton.interactable = activeLines > _minLines;
        }
    }
}