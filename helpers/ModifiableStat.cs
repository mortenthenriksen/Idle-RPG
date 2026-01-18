using System.Collections.Generic;
using System.Linq;

public class ModifiableStat
{
    public float BaseValue;
    
    private List<float> _flatModifiers = new();      // +10, +20
    private List<float> _increaseModifiers = new();  // +10%, +20% (Additive)
    private List<float> _moreModifiers = new();      // x1.10, x1.20 (Multiplicative)

    public ModifiableStat(float baseValue) => BaseValue = baseValue;

    public void AddFlat(float value) => _flatModifiers.Add(value);
    public void AddIncreased(float value) => _increaseModifiers.Add(value);
    public void AddMore(float value) => _moreModifiers.Add(value);
    
    public void RemoveFlat(float value) => _flatModifiers.Remove(value);
    public void RemoveIncreased(float value) => _increaseModifiers.Remove(value);
    public void RemoveMore(float value) => _moreModifiers.Remove(value);

    public float GetValue()
    {
        // 1. Calculate Base + Sum of Flats
        float totalFlat = BaseValue + _flatModifiers.Sum();

        // 2. Calculate "Increased" (Additive Percentages)
        // 1.0 + (0.1 + 0.2) = 1.3x
        float totalIncreased = 1.0f + _increaseModifiers.Sum();

        // 3. Apply "More" (Multiplicative Percentages)
        // This compounds: Total * 1.1 * 1.2
        float totalMoreMultiplier = 1.0f;
        foreach (var multiplier in _moreModifiers)
        {
            totalMoreMultiplier *= (1.0f + multiplier);
        }

        // Final Formula: (Base + Flat) * Increased * More1 * More2...
        return totalFlat * totalIncreased * totalMoreMultiplier;
    }

    public List<float> GetIncreased()
    {
        return _increaseModifiers;
    }
}