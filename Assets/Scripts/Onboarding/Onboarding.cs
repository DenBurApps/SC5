using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;

public class Onboarding : MonoBehaviour
{
    [SerializeField] private List<CanvasGroup> _steps;

    [Header("Transition Settings")] 
    [SerializeField] private float _transitionDuration = 0.5f;
    [SerializeField] private float _slideDistance = 1000f;
    [SerializeField] private Ease _enterEase = Ease.OutBack;
    [SerializeField] private Ease _exitEase = Ease.InBack;

    [Header("Additional Animation Settings")]
    [SerializeField] private float _scaleMultiplier = 0.8f;
    [SerializeField] private float _staggerDelay = 0.1f;

    private int _currentIndex = 0;
    private bool _isTransitioning = false;

    public event Action OnboardingShown;

    private void Awake()
    {
        if (PlayerPrefs.HasKey("Onboarding"))
        {
            gameObject.SetActive(false);
            OnboardingShown?.Invoke();
        }
        else
        {
            gameObject.SetActive(true);
            ShowOnboarding();
        }
    }

    private void ShowOnboarding()
    {
        _currentIndex = 0;
        foreach (var step in _steps)
        {
            var rectTransform = step.GetComponent<RectTransform>();
            step.alpha = 0;
            step.interactable = false;
            step.blocksRaycasts = false;
            rectTransform.anchoredPosition = new Vector2(_slideDistance, 0);
            rectTransform.localScale = Vector3.one * _scaleMultiplier;
        }

        var firstStep = _steps[_currentIndex];
        firstStep.alpha = 1;
        firstStep.interactable = true;
        firstStep.blocksRaycasts = true;
        var firstRectTransform = firstStep.GetComponent<RectTransform>();
        firstRectTransform.anchoredPosition = Vector2.zero;
        firstRectTransform.localScale = Vector3.one;
    }

    public void ShowNextStep()
    {
        if (_isTransitioning) return;

        _currentIndex++;
        if (_currentIndex < _steps.Count)
        {
            TransitionToNextStep();
        }
        else
        {
            SkipOnboarding();
        }
    }

    private void TransitionToNextStep()
    {
        _isTransitioning = true;
        CanvasGroup currentStep = _steps[_currentIndex - 1];
        CanvasGroup nextStep = _steps[_currentIndex];

        var nextRectTransform = nextStep.GetComponent<RectTransform>();
        nextStep.alpha = 0;
        nextStep.interactable = false;
        nextStep.blocksRaycasts = false;
        nextRectTransform.anchoredPosition = new Vector2(_slideDistance, 0);
        nextRectTransform.localScale = Vector3.one * _scaleMultiplier;

        Sequence transition = DOTween.Sequence();
        
        currentStep.interactable = false;
        currentStep.blocksRaycasts = false;
        
        transition.Append(AnimateStepOut(currentStep));

        transition.Insert(_staggerDelay, AnimateStepIn(nextStep));
        
        transition.OnComplete(() =>
        {
            nextStep.interactable = true;
            nextStep.blocksRaycasts = true;
            _isTransitioning = false;
        });
    }

    private Sequence AnimateStepIn(CanvasGroup step)
    {
        var rectTransform = step.GetComponent<RectTransform>();
        Sequence sequence = DOTween.Sequence();

        sequence.Join(step.DOFade(1, _transitionDuration * 0.8f)
            .SetEase(Ease.OutQuad));
        
        sequence.Join(rectTransform.DOAnchorPosX(0, _transitionDuration)
            .SetEase(_enterEase));

        sequence.Join(rectTransform.DOScale(Vector3.one, _transitionDuration)
            .SetEase(Ease.OutBack));

        return sequence;
    }

    private Sequence AnimateStepOut(CanvasGroup step)
    {
        var rectTransform = step.GetComponent<RectTransform>();
        Sequence sequence = DOTween.Sequence();

        sequence.Join(step.DOFade(0, _transitionDuration * 0.8f)
            .SetEase(Ease.InQuad));
        
        sequence.Join(rectTransform.DOAnchorPosX(-_slideDistance, _transitionDuration)
            .SetEase(_exitEase));

        sequence.Join(rectTransform.DOScale(Vector3.one * _scaleMultiplier, _transitionDuration)
            .SetEase(Ease.InBack));

        return sequence;
    }

    public void SkipOnboarding()
    {
        if (_currentIndex < _steps.Count)
        {
            var currentStep = _steps[_currentIndex];
            currentStep.interactable = false;
            currentStep.blocksRaycasts = false;
            
            AnimateStepOut(currentStep).OnComplete(() =>
            {
                PlayerPrefs.SetInt("Onboarding", 1);
                OnboardingShown?.Invoke();
                gameObject.SetActive(false);
            });
        }
        else
        {
            PlayerPrefs.SetInt("Onboarding", 1);
            OnboardingShown?.Invoke();
            gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        DOTween.Kill(transform);
    }
}