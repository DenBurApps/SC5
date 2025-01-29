using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Games
{
    public class SlotReelsHolder : MonoBehaviour
    {
        [SerializeField] private List<SlotColumn> _slotColumns;
        [SerializeField] private float _delayBetweenColumns = 0.2f;
        [SerializeField] private float _delayBeforeStopEvent = 0.5f;

        private bool _isSpinning = false;
        private int _reelsStopped;

        public event Action StopedSpin;
        public event Action StartedSpin;

        private void Start()
        {
            _slotColumns = _slotColumns.OrderBy(column => column.transform.position.x).ToList();
            
            foreach (var column in _slotColumns)
            {
                column.OnStoppedSpinning += OnColumnStoppedSpinning;
            }
        }

        private void OnDisable()
        {
            foreach (var column in _slotColumns)
            {
                if (column != null)
                    column.OnStoppedSpinning -= OnColumnStoppedSpinning;
            }
        }

        public void StartSpinning()
        {
            if (_isSpinning) return;

            _isSpinning = true;
            _reelsStopped = 0;
            StartedSpin?.Invoke();
            _currentColumnIndex = 0;
            StartNextColumn();
        }

        private int _currentColumnIndex = 0;

        private void StartNextColumn()
        {
            if (_currentColumnIndex < _slotColumns.Count)
            {
                var column = _slotColumns[_currentColumnIndex];
                column.SpinReel();

                if (_currentColumnIndex < _slotColumns.Count - 1)
                {
                    Invoke(nameof(StartNextColumn), _delayBetweenColumns);
                }

                _currentColumnIndex++;
            }
        }

        private void OnColumnStoppedSpinning()
        {
            _reelsStopped++;

            if (_reelsStopped < _slotColumns.Count) return;

            _isSpinning = false;
            Invoke(nameof(InvokeStopEvent), _delayBeforeStopEvent);
        }

        private void InvokeStopEvent()
        {
            StopedSpin?.Invoke();
        }
    }
}