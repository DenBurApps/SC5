using DG.Tweening;
using UnityEngine;

namespace TutorialScreen
{
    public class TutorialWindowController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup[] _windowCanvasGroups;
        [SerializeField] private float _transitionDuration = 0.5f;
        [SerializeField] private float _slideDistance = 1000f;
        [SerializeField] private Ease _easeType = Ease.InOutQuad;
        [SerializeField] private RectTransform[] _tutorialWindows;
        private int _currentWindowIndex = 0;

        private void Awake()
        {
            for (int i = 0; i < _tutorialWindows.Length; i++)
            {
                _windowCanvasGroups[i].alpha = i == 0 ? 1f : 0f;
                _tutorialWindows[i].gameObject.SetActive(i == 0);
            }
        }

        public void ShowNextWindow()
        {
            if (_currentWindowIndex >= _tutorialWindows.Length - 1) return;

            TransitionWindows(_currentWindowIndex, _currentWindowIndex + 1, true);
            _currentWindowIndex++;
        }

        public void ShowPreviousWindow()
        {
            if (_currentWindowIndex <= 0) return;

            TransitionWindows(_currentWindowIndex, _currentWindowIndex - 1, false);
            _currentWindowIndex--;
        }

        private void TransitionWindows(int fromIndex, int toIndex, bool slideRight)
        {
            RectTransform currentWindow = _tutorialWindows[fromIndex];
            RectTransform nextWindow = _tutorialWindows[toIndex];
            CanvasGroup currentCanvasGroup = _windowCanvasGroups[fromIndex];
            CanvasGroup nextCanvasGroup = _windowCanvasGroups[toIndex];

            nextWindow.gameObject.SetActive(true);
            nextWindow.anchoredPosition = new Vector2(slideRight ? _slideDistance : -_slideDistance, 0f);
            nextCanvasGroup.alpha = 0f;

            Sequence exitSequence = DOTween.Sequence();
            exitSequence.Join(currentWindow.DOAnchorPosX(slideRight ? -_slideDistance : _slideDistance,
                _transitionDuration));
            exitSequence.Join(currentCanvasGroup.DOFade(0f, _transitionDuration));

            Sequence enterSequence = DOTween.Sequence();
            enterSequence.Join(nextWindow.DOAnchorPosX(0f, _transitionDuration));
            enterSequence.Join(nextCanvasGroup.DOFade(1f, _transitionDuration));

            exitSequence.OnComplete(() =>
            {
                currentWindow.gameObject.SetActive(false);
                currentWindow.anchoredPosition = Vector2.zero;
            });

            exitSequence.SetEase(_easeType);
            enterSequence.SetEase(_easeType);
        }

        private void OnDestroy()
        {
            DOTween.KillAll();
        }
    }
}