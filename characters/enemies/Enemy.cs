using Autoload;
using Components;
using Godot;

namespace Characters;

public abstract partial class Enemy : CharacterBody2D
{
    private ProgressBar healthBar;
    
    public override void _Ready()
    {
        AddToGroup("enemy");
        healthBar = GetNode<ProgressBar>("HealthBar");

        var healthNode = GetNode<HealthNode>("HealthNode");
        healthNode.HealthChanged += OnHealthChanged;
        healthNode.Died += OnDeath;
        healthBar.Value = healthNode.MaxHealth;
        healthBar.MaxValue = healthNode.MaxHealth;
    }

    private void OnHealthChanged(float newHealth)
    {
        if (healthBar != null)
            healthBar.Value = newHealth;
    }
    
    protected virtual void OnDeath(CharacterBody2D characterBody2D)
    {
        healthBar.Visible = false;
    }
}
