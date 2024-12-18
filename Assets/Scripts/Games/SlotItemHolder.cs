using System;
using UnityEngine;
using UnityEngine.UI;

namespace Games
{
    [RequireComponent(typeof(Image))]
    public class SlotItemHolder : MonoBehaviour
    {
        [SerializeField] private Image _flashAnimator;
        [SerializeField] private Color _transparentColor;
        [SerializeField] private Color _defaultColor;

        private Image _image;
        
        public SlotItem SlotItem { get; private set; }

        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        private void OnEnable()
        {
            ToggleFlashAnimation(false);
        }

        public void SetItem(SlotItem data)
        {
            SlotItem = data;
            _image.sprite = SlotItem.Sprite;
            _image.color = _defaultColor;

            if (data.Type == Type.Empty)
            {
                _image.color = _transparentColor;
            }
        }
        
        public void ToggleFlashAnimation(bool status)
        {
            _flashAnimator.gameObject.SetActive(status);
        }
        
        public float GetHeight()
        {
            return _image.rectTransform.rect.height;
        }
    }
}
