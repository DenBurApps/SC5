using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Games
{
    public class WinAmountPlane : MonoBehaviour
    {
        [SerializeField] private TMP_Text _winAmount;

        public void Enable(int amount)
        {
            _winAmount.gameObject.SetActive(true);
            _winAmount.text = "+" + amount;
            StartCoroutine(DisablingCoroutine());
        }

        public void Disable()
        {
            _winAmount.gameObject.SetActive(false);
        }

        private IEnumerator DisablingCoroutine()
        {
            yield return new WaitForSeconds(3);

            _winAmount.gameObject.SetActive(false);
        }
    }
}
