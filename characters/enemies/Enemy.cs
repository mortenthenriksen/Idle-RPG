using System;
using Components;
using Godot;
using Helpers;
using Managers;

namespace Characters;

public abstract partial class Enemy : CharacterBody2D
{
    private ProgressBar healthBar;
    private float healthMultiplier;
    private float damageMultiplier;
    private HealthNode healthNode;

    protected float MeeleeEnemyMovementSpeed = -60.0f;
    
    public override void _Ready()
    {
        AddToGroup(Groups.Enemy);
        healthBar = GetNode<ProgressBar>("HealthBar");
        healthNode = GetNode<HealthNode>("HealthNode");
        healthNode.HealthChanged += OnHealthChanged;

        var waveDifficulty = WaveManager.Instance.GetWaveDifficulty();

        // HP scales at 10% per difficulty
        healthMultiplier = 1 + (float)(waveDifficulty * 0.1);
        healthNode.maxHealth = Math.Floor(healthNode.maxHealth * healthMultiplier);
        healthNode.currentHealth = Math.Floor(healthNode.currentHealth * healthMultiplier);

        // Damage scales at 5% per difficulty (slower than HP)
        damageMultiplier = 1 + (float)(waveDifficulty * 0.05);
        Upgrades.Statistics.Instance.enemyStats[Upgrades.Statistics.Traits.Damage].AddMore(damageMultiplier - 1);
    }

    private void OnHealthChanged(float newHealth, float maxHealth)
    {
        if (healthBar != null)
            healthBar.Value = newHealth;
    }
}