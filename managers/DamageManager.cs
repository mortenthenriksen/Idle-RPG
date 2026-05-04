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

    public void ApplyDamage(CharacterBody2D source, CharacterBody2D target)
    {
        var healthNode = target.GetNode<HealthNode>("HealthNode");
        if (healthNode.isDying) return;

        var attackerStats = GetStatsFor(source);
        var damage = attackerStats[Statistics.Traits.Damage].GetValue();

        bool isCrit = false;
        if (source.IsInGroup("player"))
        {
            float critChance = playerStats[Statistics.Traits.CritChance].GetValue();
            if (GD.Randf() <= critChance)
            {
                damage *= playerStats[Statistics.Traits.CritDamage].GetValue();
                isCrit = true;
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
        }

        healthNode.ApplyDamage(damage);
        EmitSignal("DamageDealt", source, target, damage, isCrit);
    }

    private Dictionary<Statistics.Traits, ModifiableStat> GetStatsFor(CharacterBody2D character2d)
    {
        // If the unit is the player, return player stats; otherwise, return enemy stats
        if (character2d.IsInGroup("enemy")) return enemyStats;
        if (character2d.IsInGroup("player")) return playerStats;
        return null;
    }
}

