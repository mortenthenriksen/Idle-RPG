using System;
using Components;
using Godot;
using Helpers;
using Managers;
using Upgrades;

namespace Characters;

public abstract partial class Enemy : CharacterBody2D
{
    [Signal]
    public delegate void DeathAnimationFinishedEventHandler();
    
    private float healthMultiplier;
    private float damageMultiplier;
    private HealthNode healthNode;
        
    public override void _Ready()
    {
        AddToGroup(Groups.Enemy);
        healthNode = GetNode<HealthNode>("HealthNode");

        var waveDifficulty = WaveManager.Instance.GetWaveDifficulty();
        healthMultiplier  = 1 + (float)(waveDifficulty * 0.1);
        damageMultiplier  = 1 + (float)(waveDifficulty * 0.05);

        var baseHealth = Statistics.Instance.enemyStats[Statistics.Traits.Health].GetValue();
        healthNode.maxHealth     = Math.Floor(baseHealth * healthMultiplier);
        healthNode.currentHealth = healthNode.maxHealth;
    }
    
    public float GetAttackInterval()
    {
        var attackSpeed = Statistics.Instance.enemyStats[Statistics.Traits.AttackSpeed].GetValue();
        return 1f / attackSpeed;
    }

    protected void EmitOnDeathAnimationFinished()
    {
        EmitSignal(SignalName.DeathAnimationFinished);
    }
    
    public float GetDamageMultiplier() => damageMultiplier;
}