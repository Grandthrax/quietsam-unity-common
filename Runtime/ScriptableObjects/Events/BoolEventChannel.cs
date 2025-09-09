using UnityEngine;
using System;

namespace QuietSam.Common
{
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