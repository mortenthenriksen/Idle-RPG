
using Godot;

namespace Upgrades;

public partial class AncestryEntry : GodotObject
{
    public Statistics.Traits Trait;
    public float AmountPerLevel;
    public float CurrentLevel;
    public float MaxLevel;

    public AncestryEntry(Statistics.Traits trait, float amountPerLevel, float maxLevel)
    {
        Trait = trait;
        AmountPerLevel = amountPerLevel;
        CurrentLevel = 0;
        MaxLevel = maxLevel;
    }
}