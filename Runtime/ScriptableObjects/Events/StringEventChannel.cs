using UnityEngine;
using System;

namespace QuietSam.Common
{
    [CreateAssetMenu(menuName = "Events/String Event")]
    public class StringEventChannel : ScriptableObject
    {
        public event Action<string> OnEventRaised;
        public void Raise(string value)
        {
            OnEventRaised?.Invoke(value);
        }
    }
}