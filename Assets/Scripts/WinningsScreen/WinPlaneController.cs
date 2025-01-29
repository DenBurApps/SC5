using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WinningsScreen
{
    public class RecordScreen : MonoBehaviour
    {
        [SerializeField] private WinPlane[] _winPlanes;
        [SerializeField] private GameObject _emptyPlane;
        [SerializeField] private CanvasGroup _canvasGroup;

        private void Start()
        {
            DisableScreen();
        }

        private void OnEnable()
        {
            WinningsManager.OnNewWin += OnNewWinAdded;
        }

        private void OnDisable()
        {
            WinningsManager.OnNewWin -= OnNewWinAdded;
        }
        
        public void EnableScreen()
        {
            _canvasGroup.alpha = 1;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            
            UpdateWinPlanes();
        }

        public void DisableScreen()
        {
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        private void OnNewWinAdded(GameType gameType, int amount)
        {
            UpdateWinPlanes();
        }

        private void UpdateWinPlanes()
        {
            DisableAllPlanes();
            var winnings = WinningsManager.GetTopWinnings(_winPlanes.Length);

            for (int i = 0; i < winnings.Count; i++)
            {
                if (winnings[i].Win > 0)
                {
                    _winPlanes[i].Setup(winnings[i], i + 1);
                }
            }
            
            ToggleEmptyPlane();
        }

        private void DisableAllPlanes()
        {
            foreach (var plane in _winPlanes)
            {
                plane.gameObject.SetActive(false);
            }
        }

        private void ToggleEmptyPlane()
        {
            _emptyPlane.SetActive(!_winPlanes.Any(plane => plane.isActiveAndEnabled));
        }
    }
}