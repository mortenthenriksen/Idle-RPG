using System.Collections.Generic;
using System.Linq;

public class ModifiableStat
{
    public float BaseValue;
    
    private List<float> _flatModifiers = new();
    private List<float> _percentModifiers = new(); // 0.1 = 10%

    public ModifiableStat(float baseValue) => BaseValue = baseValue;

    public void AddFlat(float value) => _flatModifiers.Add(value);
    public void AddPercent(float value) => _percentModifiers.Add(value);
    
    public void RemoveFlat(float value) => _flatModifiers.Remove(value);
    public void RemovePercent(float value) => _percentModifiers.Remove(value);

    public float GetValue()
    {
        float totalFlat = _flatModifiers.Sum();
        float totalPercent = 1.0f + _percentModifiers.Sum(); 
        
        return (BaseValue + totalFlat) * totalPercent;
    }

    public List<float> GetPercentages()
    {
        return _percentModifiers;
    }
}