using Godot;
using Components;
using Characters;
using Autoload;
using System.Collections.Generic;
using static Statistics;

namespace Managers;

public partial class DamageManager : Node
{
    [Signal]
    public delegate void DamageDealtEventHandler(CharacterBody2D source, CharacterBody2D target, float DamageAmount);

    [Signal]
    public delegate void AttackBlockedEventHandler(CharacterBody2D source, CharacterBody2D target);

    public static DamageManager Instance { get; private set; }

    private Dictionary<Stats, ModifiableStat> basePlayerStats;
    private Dictionary<Stats, ModifiableStat> baseEnemyStats;
    
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
        var damageAmount = attackerStats[Stats.Damage].GetValue();
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

    private Dictionary<Stats, ModifiableStat> GetStatsFor(CharacterBody2D character2d)
    {
        // If the unit is the player, return player stats; otherwise, return enemy stats
        if (character2d.IsInGroup("enemy")) return baseEnemyStats;
        if (character2d.IsInGroup("player")) return basePlayerStats;
        return null;
    }

    public void AdditiveIncreasePlayerDamage(float increaseDamageValue)
    {
        var damageStat = basePlayerStats[Stats.Damage];
        damageStat.AddFlat(1);
    }

    public void MultiplicativeIncreasePlayerDamage(float increaseDamageValue)
    {
        var damageStat = basePlayerStats[Stats.Damage];
        damageStat.AddPercent(1f);
    }

    public void IncreasePlayerAttackSpeed(float percentageIncrease)
    {
        var damageStat = basePlayerStats[Stats.Damage];
        damageStat.RemovePercent(1f);
    }
}

