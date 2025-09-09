using UnityEngine;
using System;

namespace QuietSam.Common
{
    [CreateAssetMenu(menuName = "Events/Int Event")]
    public class IntEventChannel : ScriptableObject
    {
        [SerializeField] private int startValue;
        private int _value;
        public event Action<int> OnEventRaised;
        public int Value => _value;

        public void Raise(int valueChange)
        {
            _value += valueChange;
            OnEventRaised?.Invoke(valueChange);
        }
    }
}