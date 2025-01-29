using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using DanielLochner.Assets.SimpleScrollSnap;
using Random = UnityEngine.Random;

namespace Games
{
    public class SlotColumn : MonoBehaviour
    {
        [Header("Slot Settings")] 
        [SerializeField] private List<SlotItemHolder> _slotItemHolders;
        [SerializeField] private List<SlotItem> _items;
        
        [Header("Scroll Snap Settings")]
        [SerializeField] private SimpleScrollSnap _scrollSnap;
        
        [Header("Spin Animation Settings")]
        [SerializeField] private float _startSpeed = 2f;
        [SerializeField] private float _maxSpeed = 15f;
        [SerializeField] private float _accelerationTime = 0.8f;
        [SerializeField] private float _maintainSpeedTime = 1.5f;
        [SerializeField] private float _decelerationTime = 1.2f;
        [SerializeField] private Ease _accelerationEase = Ease.InSine;
        [SerializeField] private Ease _decelerationEase = Ease.OutSine;

        private Sequence _spinSequence;
        private float _currentSpeed;
        private bool _isSpinning;
        private float _panelHeight;
        public event Action OnStoppedSpinning;

        private void Start()
        {
            InitializeScrollSnap();
            DisableAllFlashAnimations();
            ShuffleItems();
            _panelHeight = _scrollSnap.Content.rect.height / _scrollSnap.NumberOfPanels;
            
            UpdateVisibleItems();
        }

        private void InitializeScrollSnap()
        {
            _scrollSnap.enabled = true;
            _scrollSnap.UseInfiniteScrolling = true;
        }

        private void ShuffleItems()
        {
            if (_items != null && _items.Count > 0)
            {
                _items = _items.OrderBy(x => Random.value).ToList();
            }
        }

        private void OnDisable()
        {
            _spinSequence?.Kill();
        }

         public void SpinReel()
        {
            if (_slotItemHolders == null || _slotItemHolders.Count == 0 || _scrollSnap == null)
            {
                Debug.LogError("Required components are not assigned.");
                return;
            }

            if (_isSpinning)
            {
                return;
            }

            StartSpinAnimation();
        }

        private void StartSpinAnimation()
        {
            _isSpinning = true;
            
            _currentSpeed = _startSpeed;
            
            _spinSequence?.Kill();
            _spinSequence = DOTween.Sequence();
            
            _spinSequence.Append(
                DOTween.To(() => _currentSpeed, x => {
                    _currentSpeed = x;
                    ScrollContent();
                }, _maxSpeed, _accelerationTime)
                .SetEase(_accelerationEase)
            );

            _spinSequence.Append(
                DOTween.To(() => _currentSpeed, x => {
                    _currentSpeed = x;
                    ScrollContent();
                    UpdateVisibleItems();
                }, _maxSpeed, _maintainSpeedTime)
                .SetEase(Ease.Linear)
            );

            _spinSequence.Append(
                DOTween.To(() => _currentSpeed, x => {
                    _currentSpeed = x;
                    ScrollContent();
                }, 0, _decelerationTime)
                .SetEase(_decelerationEase)
            );

            _spinSequence.OnComplete(() => {
                CompleteSpinAnimation();
            });

            _spinSequence.Play();
        }

        private void ScrollContent()
        {
            if (_scrollSnap != null)
            {
                float newPosition = _scrollSnap.Content.anchoredPosition.y - (_currentSpeed * Time.deltaTime * 500);

                _scrollSnap.Content.anchoredPosition = new Vector2(
                    _scrollSnap.Content.anchoredPosition.x,
                    newPosition
                );
            }
        }

        private void UpdateVisibleItems()
        {
            float currentPosition = Mathf.Abs(_scrollSnap.Content.anchoredPosition.y);

            for (int i = 0; i < _slotItemHolders.Count; i++)
            {
                int itemIndex = Mathf.FloorToInt(currentPosition / _panelHeight) % _items.Count;
                _slotItemHolders[i].SetItem(_items[(itemIndex + i) % _items.Count]);
            }
        }
        
        private void CompleteSpinAnimation()
        {
            _isSpinning = false;
            OnStoppedSpinning?.Invoke();
        }

        public void DisableAllFlashAnimations()
        {
            foreach (var slotItemHolder in _slotItemHolders)
            {
                slotItemHolder.ToggleFlashAnimation(false);
            }
        }
    }
}