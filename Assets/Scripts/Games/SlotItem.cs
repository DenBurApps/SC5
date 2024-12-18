using UnityEngine;

namespace Games
{
    [CreateAssetMenu(fileName = "New item", menuName = "Item")]
    public class SlotItem : ScriptableObject
    {
        [SerializeField] private Sprite _sprite;
        [SerializeField] private Type _type;

        public Sprite Sprite => _sprite;
        public Type Type => _type;
    }
    
    public enum Type
    {
        Empty,
        Bar,
        [Header("ChinaStreet")]
        Scroll,
        Bongo,
        Rice,
        Flower,
        [Header("BambooFortune")]
        Banana,
        Cherry,
        Grape,
        Melon,
    }
}
