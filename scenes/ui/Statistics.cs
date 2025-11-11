using System;
using Godot;
using Managers;


public partial class Statistics : Control
{
    [Signal]
    public delegate void PlayerStatUpgradedEventHandler(string statName);

    [Signal]
    public delegate void EnemyStatUpgradedEventHandler(float newHealth, float maxHealth);

    public static Statistics Instance { get; private set; }

    private Button playerLifeButton;
    private Button playerAttackDamageButton;
    private Button playerAttackSpeedButton;
    private Button playerMovementSpeedButton;

    public override void _Ready()
    {
        var statsNode = GetNode("/root/Main/UserInterface/Statistics");
        playerLifeButton = statsNode.GetNode<Button>("%PlayerLifeButton");
        playerAttackDamageButton = statsNode.GetNode<Button>("%PlayerAttackDamageButton");
        playerAttackSpeedButton = statsNode.GetNode<Button>("%PlayerAttackSpeedButton");
        playerMovementSpeedButton = statsNode.GetNode<Button>("%PlayerMovementSpeedButton");

        playerLifeButton.Pressed += OnPlayerLifeButtonPressed;
        playerAttackDamageButton.Pressed += OnPlayerAttackDamageButtonPressed;
        playerAttackSpeedButton.Pressed += OnPlayerAttackSpeedButtonPressed;
        playerMovementSpeedButton.Pressed += OnPlayerMovementSpeedButtonPressed;
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
        EmitSignal(SignalName.PlayerStatUpgraded, "AttackDamage");
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
}
