using System.Dynamic;
using Characters;
using Godot;

namespace Components;

public partial class HealthNode : Node
{
    [Export]
    public float maxHealth = 100;
    public float currentHealth { get; private set; }

    [Signal]
    public delegate void HealthChangedEventHandler(float newHealth, float maxHealth);

    [Signal]
    public delegate void DiedEventHandler(Enemy enemy);
    
    public override void _Ready()
    {
        currentHealth = maxHealth;
    }

    public void ApplyDamage(float amount)
    {
        currentHealth -= amount;
        EmitSignal(SignalName.HealthChanged, currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            var owner = GetParent<CharacterBody2D>();
            EmitSignal(SignalName.Died, owner);
        }
    }
    
    public void IncreaseMaxHealth(float value)
    {
        maxHealth += value;
    }


}
