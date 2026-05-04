using System.Collections.Generic;
using Autoload;
using Godot;
using Managers;

namespace Upgrades;

public partial class Statistics : Control
{
    [Signal]
    public delegate void PlayerStatUpgradedEventHandler(Traits traits);

    [Signal]
    public delegate void EnemyStatUpgradedEventHandler(Traits traits);

    public static Statistics Instance { get; private set; }

    public enum Traits { Damage, Health, AttackSpeed, MovementSpeed, ExperienceGained }

    public Dictionary<Traits, ModifiableStat> playerStats = new();
    public Dictionary<Traits, ModifiableStat> enemyStats = new();

    // ── To add a new stat upgrade: add one entry here ────────────────────────
    private static readonly Dictionary<Traits, (float flat, float increased)> upgradeAmounts = new()
    {
        { Traits.Damage,        (flat: 2,  increased: 0)      },
        { Traits.Health,        (flat: 5,  increased: 0)      },
        { Traits.AttackSpeed,   (flat: 0,  increased: 100f)   },
        { Traits.MovementSpeed, (flat: 0,  increased: 0.01f)  },
    };

    public override void _Ready()
    {
        Instance = this;
        InitializeStats();
        SetupButtons();
    }

    private void InitializeStats()
    {
        playerStats[Traits.Damage]           = new ModifiableStat(30);
        playerStats[Traits.Health]           = new ModifiableStat(30);
        playerStats[Traits.AttackSpeed]      = new ModifiableStat(1.33f);
        playerStats[Traits.MovementSpeed]    = new ModifiableStat(85f);
        playerStats[Traits.ExperienceGained] = new ModifiableStat(0f);

        enemyStats[Traits.Damage]            = new ModifiableStat(1);
        enemyStats[Traits.Health]            = new ModifiableStat(17);
        enemyStats[Traits.AttackSpeed]       = new ModifiableStat(1.33f);
        enemyStats[Traits.MovementSpeed]     = new ModifiableStat(-60f);
    }

    private void SetupButtons()
    {
        var playerButtons = new Dictionary<string, Traits>
        {
            { "%PlayerAttackDamageButton",  Traits.Damage        },
            { "%PlayerHealthButton",        Traits.Health          },
            { "%PlayerAttackSpeedButton",   Traits.AttackSpeed   },
            { "%PlayerMovementSpeedButton", Traits.MovementSpeed },
        };

        var enemyButtons = new Dictionary<string, Traits>
        {
            { "%EnemyAttackDamageButton",   Traits.Damage        },
            { "%EnemyHealthButton",         Traits.Health          },
            { "%EnemyAttackSpeedButton",    Traits.AttackSpeed   },
            { "%EnemyMovementSpeedButton",  Traits.MovementSpeed },
        };

        foreach (var (path, trait) in playerButtons)
            GetNode<Button>(path).Pressed += () => Upgrade(playerStats, trait, SignalName.PlayerStatUpgraded);

        foreach (var (path, trait) in enemyButtons)
            GetNode<Button>(path).Pressed += () => Upgrade(enemyStats, trait, SignalName.EnemyStatUpgraded);
    }

    private void Upgrade(Dictionary<Traits, ModifiableStat> stats, Traits trait, StringName signal)
    {
        if (ExperienceManager.Instance.GetUnspentSkillPoints() <= 0) return;
        ExperienceManager.Instance.DecreaseUnspentSkillPoints();
        var (flat, increased) = upgradeAmounts[trait];
        if (flat      != 0) stats[trait].AddFlat(flat);
        if (increased != 0) stats[trait].AddIncreased(increased);

        EmitSignal(signal, Variant.From(trait));
    }

    public Dictionary<Traits, ModifiableStat> GetplayerStats() => playerStats;
    public Dictionary<Traits, ModifiableStat> GetenemyStats() => enemyStats;
}