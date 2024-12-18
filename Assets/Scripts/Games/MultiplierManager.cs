using System.Collections.Generic;
using UnityEngine;

namespace Games
{
    public class MultiplierManager : MonoBehaviour
    {
        [SerializeField] private List<MultiplierData> _multiplierDataList = new List<MultiplierData>();

        private Dictionary<Type, Dictionary<int, int>> _multiplierLookup;

        private void Awake()
        {
            InitializeMultiplierLookup();
        }

        private void InitializeMultiplierLookup()
        {
            _multiplierLookup = new Dictionary<Type, Dictionary<int, int>>();

            foreach (var data in _multiplierDataList)
            {
                var hitMultiplierDict = new Dictionary<int, int>();
                foreach (var hitMultiplier in data.Multipliers)
                {
                    hitMultiplierDict[hitMultiplier.HitCount] = hitMultiplier.Multiplier;
                }

                _multiplierLookup[data.SlotType] = hitMultiplierDict;
            }
        }

        public int GetMultiplier(Type slotType, int hitCount)
        {
            if (_multiplierLookup.TryGetValue(slotType, out var hitMultiplierDict))
            {
                if (hitMultiplierDict.TryGetValue(hitCount, out int multiplier))
                {
                    return multiplier;
                }
            }
            
            return 1;
        }
    }

    [System.Serializable]
    public class HitMultiplier
    {
        public int HitCount;
        public int Multiplier;
    }

    [System.Serializable]
    public class MultiplierData
    {
        public Type SlotType;
        public List<HitMultiplier> Multipliers = new List<HitMultiplier>();
    }
}