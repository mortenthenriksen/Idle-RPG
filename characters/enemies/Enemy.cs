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
    private HealthNode healthNode;
    
    public override void _Ready()
    {
        AddToGroup(Groups.Enemy);
        healthBar = GetNode<ProgressBar>("HealthBar");
        healthNode = GetNode<HealthNode>("HealthNode");
        healthNode.HealthChanged += OnHealthChanged;

        DifficultyToHealthMultiplier();
        
        healthNode.maxHealth = Math.Floor(healthNode.maxHealth * healthMultiplier);
        healthNode.currentHealth = Math.Floor(healthNode.currentHealth * healthMultiplier);
    }

    private void OnHealthChanged(float newHealth, float maxHealth)
    {
        if (healthBar != null)
            healthBar.Value = newHealth;
    }

    private double DifficultyToHealthMultiplier()
    {
        // 2% increase per wave difficulty?
        var waveDifficulty = WaveManager.Instance.GetWaveDifficulty();
        healthMultiplier = 1 + (float)(waveDifficulty * 0.1);
        return healthMultiplier;
    }
}
