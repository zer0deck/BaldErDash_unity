using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HealthSystem
{
    [System.Serializable]
public class HealthSystem 
{
    [SerializeField] private ValueEvent ValueChanged = new ValueEvent();
    private int _value;
    private int _valueMax;


    public void Setup(int value)
    {
        _value = _valueMax = value;
        SayChanged();
    }


    public void AddValue(int value)
    {
        _value += value;
        if (_value > _valueMax) _value = _valueMax;
        SayChanged();

    }


    public void RemoveValue(int value)
    {
        _value -= value;
        if (_value < 0) _value = 0;
        SayChanged(); 
    }
    

    private void SayChanged()
    {
        ValueChanged.Invoke((float)_value / _valueMax);
    }
}
   [System.Serializable]
   public class ValueEvent : UnityEvent<float> {}

}
