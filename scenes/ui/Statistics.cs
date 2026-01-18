using System.Collections.Generic;
using Godot;
using Managers;

namespace Upgrades;

public partial class Statistics : Control
{
    [Signal]
    public delegate void PlayerStatUpgradedEventHandler(string statName);

    [Signal]
    public delegate void EnemyStatUpgradedEventHandler(float newHealth, float maxHealth);

    public static Statistics Instance {get; private set;}

    public enum Traits {Damage, Life, AttackSpeed, MovementSpeed, ExperienceGained}
    public Dictionary<Traits, ModifiableStat> basePlayerStats = new();
    public Dictionary<Traits, ModifiableStat> baseEnemyStats = new();

    private Button playerLifeButton;
    private Button playerAttackDamageButton;
    private Button playerAttackSpeedButton;
    private Button playerMovementSpeedButton;

    public override void _Ready()
    {
        Instance = this;
        var statsNode = GetNode("/root/Main/UserInterface/Statistics");
        playerLifeButton = statsNode.GetNode<Button>("%PlayerLifeButton");
        playerAttackDamageButton = statsNode.GetNode<Button>("%PlayerAttackDamageButton");
        playerAttackSpeedButton = statsNode.GetNode<Button>("%PlayerAttackSpeedButton");
        playerMovementSpeedButton = statsNode.GetNode<Button>("%PlayerMovementSpeedButton");

        playerLifeButton.Pressed += OnPlayerLifeButtonPressed;
        playerAttackDamageButton.Pressed += OnPlayerAttackDamageButtonPressed;
        playerAttackSpeedButton.Pressed += OnPlayerAttackSpeedButtonPressed;
        playerMovementSpeedButton.Pressed += OnPlayerMovementSpeedButtonPressed;
        CreateBasePlayerStatsDict();
        CreateBaseEnemyStatsDict();
    }

    private void CreateBasePlayerStatsDict()
    {
        basePlayerStats.Add(Traits.Damage, new ModifiableStat(2));
        basePlayerStats.Add(Traits.Life, new ModifiableStat(20));
        basePlayerStats.Add(Traits.AttackSpeed, new ModifiableStat(1.33f));
        basePlayerStats.Add(Traits.MovementSpeed, new ModifiableStat(85f));
        basePlayerStats.Add(Traits.ExperienceGained, new ModifiableStat(0f));
    }

    private void CreateBaseEnemyStatsDict()
    {
        baseEnemyStats.Add(Traits.Damage, new ModifiableStat(1));
        baseEnemyStats.Add(Traits.Life, new ModifiableStat(10));
        baseEnemyStats.Add(Traits.AttackSpeed, new ModifiableStat(1.33f));
        baseEnemyStats.Add(Traits.MovementSpeed, new ModifiableStat(0.85f));
    }
    
    private bool HasUnspentSkillPoints()
    {
        return ExperienceManager.Instance.GetUnspentSkillPoints() > 0;
    }

    private void OnPlayerLifeButtonPressed()
    {
        if (!HasUnspentSkillPoints())
            return;
        EmitSignal(SignalName.PlayerStatUpgraded, "Life");
    }

    private void OnPlayerAttackDamageButtonPressed()
    {
        if (!HasUnspentSkillPoints())
            return;
        EmitSignal(SignalName.PlayerStatUpgraded, "AttackDamageAdditive");
    }

    private void OnPlayerAttackSpeedButtonPressed()
    {
        if (!HasUnspentSkillPoints())
            return;
        EmitSignal(SignalName.PlayerStatUpgraded, "AttackSpeed");
    }

    private void OnPlayerMovementSpeedButtonPressed()
    {
        if (!HasUnspentSkillPoints())
            return;
        EmitSignal(SignalName.PlayerStatUpgraded, "MovementSpeed");
    }

    public Dictionary<Traits, ModifiableStat> GetBasePlayerStats() => basePlayerStats;
    public Dictionary<Traits, ModifiableStat> GetBaseEnemyStats() => baseEnemyStats;
}
