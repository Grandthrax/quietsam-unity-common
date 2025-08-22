using UnityEngine;
using System;

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
    public event Action<int> OnEventRaised;

    public void Raise(int value)
    {
        OnEventRaised?.Invoke(value);
    }
}

[CreateAssetMenu(menuName="Events/Int2 Event")]
public class Int2EventChannel : ScriptableObject
{
    public event Action<int, int> OnEventRaised;

    public void Raise(int value1, int value2)
    {
        OnEventRaised?.Invoke(value1, value2);
    }
}