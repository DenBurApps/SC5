using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TheraBytes.BetterUi;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Games
{
    public class SlotColumn : MonoBehaviour
    {
        [Header("Slot Settings")] [SerializeField]
        private List<SlotItemHolder> _slotItemHolders;

        [SerializeField] private List<SlotItem> _items;

        [Header("Spin Settings")] [SerializeField]
        private float _spinSpeed = 500f;

        [SerializeField] private float _spinDuration = 2f;
        [SerializeField] private float _verticalOffset = 1.0f;

        [Header("References")] [SerializeField]
        private BetterAxisAlignedLayoutGroup _layoutGroup;

        private List<SlotItem> _finalResult;

        public event Action OnStartSpinning;
        public event Action OnStoppedSpinning;

        private void Start()
        {
            DisableAllFlashAnimations();
        }

        public void SpinReel()
        {
            if (_slotItemHolders == null || _slotItemHolders.Count == 0)
            {
                Debug.LogError("Slot item holders are not assigned or empty.");
                return;
            }

            if (_items == null || _items.Count == 0)
            {
                Debug.LogError("Slot items are not assigned or empty.");
                return;
            }
            
            _finalResult = GenerateRandomResults();
            StartCoroutine(SpinCoroutine());
        }

        public void DisableAllFlashAnimations()
        {
            foreach (var slotItemHolder in _slotItemHolders)
            {
                slotItemHolder.ToggleFlashAnimation(false);
            }
        }

        private IEnumerator SpinCoroutine()
        {
            float elapsedTime = 0f;
            OnStartSpinning?.Invoke();

            while (elapsedTime < _spinDuration)
            {
                foreach (var slotItemHolder in _slotItemHolders)
                {
                    slotItemHolder.transform.Translate(Vector3.down * (_spinSpeed * Time.deltaTime));

                    if (slotItemHolder.transform.localPosition.y <= -slotItemHolder.GetHeight() * _verticalOffset)
                    {
                        LoopItemToTop(slotItemHolder);
                    }
                }

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            AlignToFinalResult();
        }

        private float GetTopPosition()
        {
            float topY = float.MinValue;
            foreach (var slotItemHolder in _slotItemHolders)
            {
                if (slotItemHolder.transform.localPosition.y > topY)
                    topY = slotItemHolder.transform.localPosition.y;
            }

            return topY;
        }

        private void LoopItemToTop(SlotItemHolder itemHolder)
        {
            float topPosition = GetTopPosition();

            itemHolder.transform.localPosition = new Vector3(
                itemHolder.transform.localPosition.x,
                topPosition + itemHolder.GetHeight(),
                itemHolder.transform.localPosition.z
            );

            itemHolder.SetItem(GetRandomItem());
        }

        private void AlignToFinalResult()
        {
            float spacing = _slotItemHolders[0].GetHeight();

            for (int i = 0; i < _slotItemHolders.Count; i++)
            {
                //_slotItemHolders[i].SetItem(_finalResult[i % _finalResult.Count]);
                /*_slotItemHolders[i].transform.localPosition = new Vector3(
                    0,
                    (_slotItemHolders.Count - i - 1) * spacing,
                    0
                );*/
            }

            _layoutGroup.SetLayoutVertical();
            OnStoppedSpinning?.Invoke();
        }

        private List<SlotItem> GenerateRandomResults()
        {
            List<SlotItem> results = new List<SlotItem>();
            for (int i = 0; i < _slotItemHolders.Count; i++)
            {
                results.Add(GetRandomItem());
            }
            
            return results;
        }

        private SlotItem GetRandomItem()
        {
            if (_items == null || _items.Count == 0)
            {
                throw new InvalidOperationException("No items available to select from.");
            }

            return _items[Random.Range(0, _items.Count)];
        }
    }
}