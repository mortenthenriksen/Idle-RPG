using Godot;
using Components;
using Characters;
using System.Collections.Generic;
using Upgrades;

namespace Managers;

public partial class DamageManager : Node
{
    [Signal]
    public delegate void DamageDealtEventHandler(CharacterBody2D source, CharacterBody2D target, float DamageAmount, bool isCrit);

    [Signal]
    public delegate void AttackBlockedEventHandler(CharacterBody2D source, CharacterBody2D target);

    public static DamageManager Instance { get; private set; }

    private Dictionary<Statistics.Traits, ModifiableStat> playerStats;
    private Dictionary<Statistics.Traits, ModifiableStat> enemyStats;
    
    public async override void _Ready()
    {
        Instance = this;

        await ToSignal(GetTree(), "process_frame"); // Wait one frame

        playerStats = Statistics.Instance.GetplayerStats();
        enemyStats = Statistics.Instance.GetenemyStats();
    }

    public void ApplyDamage(CharacterBody2D source, CharacterBody2D target, bool isCrit)
    {
        var healthNode = target.GetNode<HealthNode>("HealthNode");
        if (healthNode.isDying) return;

        var attackerStats = GetStatsFor(source);
        var damage = attackerStats[Statistics.Traits.Damage].GetValue();

        if (source.IsInGroup("player"))
        {
            if (isCrit)
            {
                damage *= playerStats[Statistics.Traits.CritDamage].GetValue();
            }
        }

        if (target.GetType() == typeof(Player))
        {
            var player = (Player)target;
            if (player.GetIsBlocking())
            {
                EmitSignal("AttackBlocked", source, target);
                return;
            }
            damage = CalculateDamageAfterDefence(damage);
        }

        healthNode.ApplyDamage(damage);
        EmitSignal("DamageDealt", source, target, damage, isCrit);
    }
    
    private float CalculateDamageAfterDefence(float rawDamage)
    {
        var defence = (float)playerStats[Statistics.Traits.Defence].GetValue();
        // D4-style: damage reduction = defence / (defence + 50)
        // 50 defence = 50% reduction, 100 = 67%, scales but never reaches 100%
        var reductionPercent = defence / (defence + 50f);
        return rawDamage * (1f - reductionPercent);
    }

    private Dictionary<Statistics.Traits, ModifiableStat> GetStatsFor(CharacterBody2D character2d)
    {
        // If the unit is the player, return player stats; otherwise, return enemy stats
        if (character2d.IsInGroup("enemy")) return enemyStats;
        if (character2d.IsInGroup("player")) return playerStats;
        return null;
    }
}

