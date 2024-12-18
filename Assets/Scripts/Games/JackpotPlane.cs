using System.Collections;
using TMPro;
using UnityEngine;

namespace Games
{
    public class JackpotPlane : MonoBehaviour
    {
        [SerializeField] private TMP_Text _winAmount;

        public void Enable(int amount)
        {
            gameObject.SetActive(true);
            _winAmount.text = "+" + amount;
            StartCoroutine(DisablingCoroutine());
        }

        private IEnumerator DisablingCoroutine()
        {
            yield return new WaitForSeconds(3);

            gameObject.SetActive(false);
        }
    }

}
