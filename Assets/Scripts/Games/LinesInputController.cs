using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Games
{
    public class LinesInputController : MonoBehaviour
    {
        private const float InactiveAlpha = 0.3f;
        private const float ActiveAlpha = 1f;
        
        [SerializeField] private int _maxLines;
        [SerializeField] private Button _plusButton;
        [SerializeField] private Button _minusButton;
        [SerializeField] private TMP_Text _currentLineText;

        [Header("Line Visualization")]
        [SerializeField] private Image[] _lineImages;

        [SerializeField] private Image[] _leftNumberImages;
        [SerializeField] private Image[] _rightNumberImages;

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
            UpdateLineVisualization();
        }

        private void UpdateLineVisualization()
        {
            for (int i = 0; i < _lineImages.Length; i++)
            {
                bool isLineActive = i < CurrentLines;
                
                if (_lineImages[i] != null)
                {
                    _lineImages[i].gameObject.SetActive(isLineActive);
                }
                
                if (_leftNumberImages[i] != null)
                {
                    SetImageAlpha(_leftNumberImages[i], isLineActive ? ActiveAlpha : InactiveAlpha);
                }

                if (_rightNumberImages[i] != null)
                {
                    SetImageAlpha(_rightNumberImages[i], isLineActive ? ActiveAlpha : InactiveAlpha);
                }
            }
        }

        private void SetImageAlpha(Image image, float alpha)
        {
            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }

        public void DisableAllLines()
        {
            foreach (var lineImage in _lineImages)
            {
                if (lineImage != null)
                {
                    lineImage.gameObject.SetActive(false);
                }
            }

            foreach (var numberImage in _leftNumberImages)
            {
                if (numberImage != null)
                {
                    numberImage.gameObject.SetActive(false);
                }
            }

            foreach (var numberImage in _rightNumberImages)
            {
                if (numberImage != null)
                {
                    numberImage.gameObject.SetActive(false);
                }
            }
        }

        public void RestoreLines()
        {
            foreach (var numberImage in _leftNumberImages)
            {
                if (numberImage != null)
                {
                    numberImage.gameObject.SetActive(true);
                }
            }

            foreach (var numberImage in _rightNumberImages)
            {
                if (numberImage != null)
                {
                    numberImage.gameObject.SetActive(true);
                }
            }

            UpdateLineVisualization();
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
            UpdateLineVisualization();
            LinesChanged?.Invoke(CurrentLines);
        }

        public void ReturnToDefault()
        {
            ToggleButtons();
            UpdateLineVisualization();
        }

        private void IncreaseLine()
        {
            CurrentLines = Mathf.Clamp(CurrentLines + 1, _minLines, _maxLines);
            UpdateLineDisplay();
            UpdateLineVisualization();
            ToggleButtons();
            LinesChanged?.Invoke(CurrentLines);
        }

        private void DecreaseLine()
        {
            CurrentLines = Mathf.Clamp(CurrentLines - 1, _minLines, _maxLines);
            UpdateLineDisplay();
            UpdateLineVisualization();
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