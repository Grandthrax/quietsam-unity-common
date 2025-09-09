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
}