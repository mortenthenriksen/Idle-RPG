using Godot;
using Components;
using Characters;
using System.Collections.Generic;
using Upgrades;

namespace Managers;

public partial class DamageManager : Node
{
    [Signal]
    public delegate void DamageDealtEventHandler(CharacterBody2D source, CharacterBody2D target, float DamageAmount);

    [Signal]
    public delegate void AttackBlockedEventHandler(CharacterBody2D source, CharacterBody2D target);

    public static DamageManager Instance { get; private set; }

    private Dictionary<Statistics.Traits, ModifiableStat> basePlayerStats;
    private Dictionary<Statistics.Traits, ModifiableStat> baseEnemyStats;
    
    public async override void _Ready()
    {
        Instance = this;

        await ToSignal(GetTree(), "process_frame"); // Wait one frame

        basePlayerStats = Statistics.Instance.GetBasePlayerStats();
        baseEnemyStats = Statistics.Instance.GetBaseEnemyStats();
    }

    public void ApplyDamage(CharacterBody2D source, CharacterBody2D target)
    {
        var attackerStats = GetStatsFor(source);
        var damageAmount = attackerStats[Statistics.Traits.Damage].GetValue();
        var healthNode = target.GetNode<HealthNode>("HealthNode");
        if (target.GetType() == typeof(Player))
        {
            var player = (Player)target;
            if (player.GetIsBlocking())
            {
                EmitSignal("AttackBlocked", source, target);
                return;   
            }
        }
        healthNode.ApplyDamage(damageAmount);
        EmitSignal("DamageDealt", source, target, damageAmount);
    }

    private Dictionary<Statistics.Traits, ModifiableStat> GetStatsFor(CharacterBody2D character2d)
    {
        // If the unit is the player, return player stats; otherwise, return enemy stats
        if (character2d.IsInGroup("enemy")) return baseEnemyStats;
        if (character2d.IsInGroup("player")) return basePlayerStats;
        return null;
    }

    public void AdditiveIncreasePlayerDamage(float increaseDamageValue)
    {
        var damageStat = basePlayerStats[Statistics.Traits.Damage];
        damageStat.AddFlat(increaseDamageValue);
    }

    public void MultiplicativeIncreasePlayerDamage(float increaseDamageValue)
    {
        var damageStat = basePlayerStats[Statistics.Traits.Damage];
        damageStat.AddPercent(increaseDamageValue);
    }

    public void IncreasePlayerAttackSpeed(float percentageIncrease)
    {
        var attackSpeedStat = basePlayerStats[Statistics.Traits.AttackSpeed];
        attackSpeedStat.AddPercent(percentageIncrease);
    }
}

