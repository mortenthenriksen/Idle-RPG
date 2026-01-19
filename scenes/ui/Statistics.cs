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
    public Dictionary<Traits, ModifiableStat> playerStats = new();
    public Dictionary<Traits, ModifiableStat> enemyStats = new();

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
        CreateplayerStatsDict();
        CreateenemyStatsDict();
    }

    private void CreateplayerStatsDict()
    {
        playerStats.Add(Traits.Damage, new ModifiableStat(2));
        playerStats.Add(Traits.Life, new ModifiableStat(20));
        playerStats.Add(Traits.AttackSpeed, new ModifiableStat(1.33f));
        playerStats.Add(Traits.MovementSpeed, new ModifiableStat(85f));
        playerStats.Add(Traits.ExperienceGained, new ModifiableStat(0f));
    }

    private void CreateenemyStatsDict()
    {
        enemyStats.Add(Traits.Damage, new ModifiableStat(1));
        enemyStats.Add(Traits.Life, new ModifiableStat(10));
        enemyStats.Add(Traits.AttackSpeed, new ModifiableStat(1.33f));
        enemyStats.Add(Traits.MovementSpeed, new ModifiableStat(0.85f));
    }
    
    private bool HasUnspentSkillPoints()
    {
        return ExperienceManager.Instance.GetUnspentSkillPoints() > 0;
    }

    private void OnPlayerLifeButtonPressed()
    {
        if (!HasUnspentSkillPoints()) return;
        EmitSignal(SignalName.PlayerStatUpgraded, "Life");
    }

    private void OnPlayerAttackDamageButtonPressed()
    {
        if (!HasUnspentSkillPoints()) return;
        EmitSignal(SignalName.PlayerStatUpgraded, "AttackDamageAdditive");
    }

    private void OnPlayerAttackSpeedButtonPressed()
    {
        if (!HasUnspentSkillPoints()) return;
        EmitSignal(SignalName.PlayerStatUpgraded, "AttackSpeed");
    }

    private void OnPlayerMovementSpeedButtonPressed()
    {
        if (!HasUnspentSkillPoints()) return;
        EmitSignal(SignalName.PlayerStatUpgraded, "MovementSpeed");
    }

    public Dictionary<Traits, ModifiableStat> GetplayerStats() => playerStats;
    public Dictionary<Traits, ModifiableStat> GetenemyStats() => enemyStats;
}
