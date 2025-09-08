using UnityEngine;
using System;

namespace QuietSam.Common
{
    [CreateAssetMenu(menuName = "Events/Void Event")]
    public class VoidEventChannel : ScriptableObject
    {
        public event Action OnEventRaised;

        public void Raise()
        {
            OnEventRaised?.Invoke();
        }
    }

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

    [CreateAssetMenu(menuName = "Events/Int2 Event")]
    public class Int2EventChannel : ScriptableObject
    {
        public event Action<int, int> OnEventRaised;

        public void Raise(int value1, int value2)
        {
            OnEventRaised?.Invoke(value1, value2);
        }
    }

    [CreateAssetMenu(menuName = "Events/Bool Event")]
    public class BoolEventChannel : ScriptableObject
    {
        public event Action<bool> OnEventRaised;
        public void Raise(bool value)
        {
            OnEventRaised?.Invoke(value);
        }
    }
}