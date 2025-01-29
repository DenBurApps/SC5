using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Popup : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private float startYPosition = -1000f;
    [SerializeField] private Ease easeType = Ease.OutBack;
        
    private RectTransform rectTransform;
    private Vector2 targetPosition;
    private Tween currentTween;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        targetPosition = rectTransform.anchoredPosition;
    }

    private void OnEnable()
    {
        currentTween?.Kill();
        rectTransform.anchoredPosition = new Vector2(targetPosition.x, startYPosition);
        
        currentTween = rectTransform.DOAnchorPosY(targetPosition.y, animationDuration)
            .SetEase(easeType)
            .OnComplete(() => currentTween = null);
    }

    private void OnDisable()
    {
        currentTween?.Kill();
        currentTween = null;
        
        rectTransform.anchoredPosition = targetPosition;
    }

    private void OnDestroy()
    {
        currentTween?.Kill();
    }
}
