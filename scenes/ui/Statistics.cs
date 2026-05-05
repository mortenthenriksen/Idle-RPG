using System.Collections.Generic;
using Autoload;
using Godot;
using Managers;

namespace Upgrades;

public partial class Statistics : Control
{
    [Signal]
    public delegate void PlayerStatUpgradedEventHandler(Traits traits, float value);

    [Signal]
    public delegate void EnemyStatUpgradedEventHandler(Traits traits, float value);

    public static Statistics Instance { get; private set; }

    public enum Traits { Damage, Health, Defence, AttackSpeed, MovementSpeed, ExperienceGained, CritChance, CritDamage}

    public Dictionary<Traits, ModifiableStat> playerStats = new();
    public Dictionary<Traits, ModifiableStat> enemyStats = new();

    // ── To add a new stat upgrade: add one entry here ────────────────────────
    private static readonly Dictionary<Traits, (float flat, float increased)> upgradeAmounts = new()
    {
        { Traits.Health,        (flat: 5,  increased: 0)      },
        { Traits.Defence,       (flat: 10,  increased: 0)      },
        { Traits.Damage,        (flat: 2,  increased: 0)      },
        { Traits.AttackSpeed,   (flat: 0,  increased: 0.1f)   },
        { Traits.MovementSpeed, (flat: 0,  increased: 0.1f)  },
        { Traits.CritChance,    (flat: 0.1f,  increased: 0.0f)  },
        { Traits.CritDamage,    (flat: 0,  increased: 10f)  },
    };

    public override void _Ready()
    {
        Instance = this;
        InitializeStats();
        SetupButtons();
    }

    private void InitializeStats()
    {
        playerStats[Traits.Health]           = new ModifiableStat(30);
        playerStats[Traits.Defence]          = new ModifiableStat(0);
        playerStats[Traits.Damage]           = new ModifiableStat(1);
        playerStats[Traits.AttackSpeed]      = new ModifiableStat(2f);
        playerStats[Traits.MovementSpeed]    = new ModifiableStat(85f);
        playerStats[Traits.ExperienceGained] = new ModifiableStat(0f);
        playerStats[Traits.CritChance]       = new ModifiableStat(0.25f); // 5% base
        playerStats[Traits.CritDamage]       = new ModifiableStat(1.5f);      // 1.5x base

        enemyStats[Traits.Damage]            = new ModifiableStat(100);
        enemyStats[Traits.Health]            = new ModifiableStat(12);
        enemyStats[Traits.AttackSpeed]       = new ModifiableStat(0.75f);
        enemyStats[Traits.MovementSpeed]     = new ModifiableStat(-60f);
    }

    private void SetupButtons()
    {
        // MAKE THIS MORE SOLID
        var playerButtons = new Dictionary<string, Traits>
        {
            { "%PlayerAttackDamageButton",  Traits.Damage        },
            { "%PlayerDefenceButton",       Traits.Defence        },
            { "%PlayerHealthButton",        Traits.Health          },
            { "%PlayerAttackSpeedButton",   Traits.AttackSpeed   },
            { "%PlayerMovementSpeedButton", Traits.MovementSpeed },
            { "%PlayerCriticalChanceButton", Traits.CritChance },
            { "%PlayerCriticalDamageButton", Traits.CritDamage },
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
        if (flat      != 0)
        {
            stats[trait].AddFlat(flat);
            EmitSignal(signal, Variant.From(trait), flat);
        }
        if (increased != 0) 
        {
            stats[trait].AddIncreased(increased);
            EmitSignal(signal, Variant.From(trait), increased);
        }

        
    }

    public Dictionary<Traits, ModifiableStat> GetplayerStats() => playerStats;
    public Dictionary<Traits, ModifiableStat> GetenemyStats() => enemyStats;
}