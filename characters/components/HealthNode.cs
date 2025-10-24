using Autoload;
using Characters;
using Godot;

namespace Components;

public partial class HealthNode : Node
{
    [Export] 
    public float MaxHealth = 100;
    public float CurrentHealth { get; private set; }

    [Signal]
    public delegate void HealthChangedEventHandler(float newHealth);

    [Signal]
    public delegate void DiedEventHandler(Enemy enemy);
    
    public override void _Ready()
    {
        CurrentHealth = MaxHealth;
    }

    public void ApplyDamage(float amount)
    {
        CurrentHealth -= amount;
        EmitSignal(SignalName.HealthChanged, CurrentHealth);

        if (CurrentHealth <= 0)
        {
            var owner = GetParent<CharacterBody2D>();
            EmitSignal(SignalName.Died, owner);
        }
    }


}
