using UnityEngine;
using System;

namespace QuietSam.Common
{
    [CreateAssetMenu(menuName = "Events/Int2 Event")]
    public class Int2EventChannel : ScriptableObject
    {
        public event Action<int, int> OnEventRaised;

        public void Raise(int value1, int value2)
        {
            OnEventRaised?.Invoke(value1, value2);
        }
    }
}