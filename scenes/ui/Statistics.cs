using System.Collections.Generic;
using Autoload;
using Characters;
using Godot;
using Managers;

namespace Upgrades;

public partial class Statistics : Control
{
    [Signal]
    public delegate void PlayerStatUpgradedEventHandler(Traits traits);

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
        
        InitializeStats();
        SetupButtonMapping();
    }

    private void InitializeStats()
    {
        // Player
        playerStats[Traits.Damage] = new ModifiableStat(2);
        playerStats[Traits.Life] = new ModifiableStat(20);
        playerStats[Traits.AttackSpeed] = new ModifiableStat(1.33f);
        playerStats[Traits.MovementSpeed] = new ModifiableStat(85f);
        playerStats[Traits.ExperienceGained] = new ModifiableStat(0f);

        // Enemy
        enemyStats[Traits.Damage] = new ModifiableStat(1);
        enemyStats[Traits.Life] = new ModifiableStat(10);
        enemyStats[Traits.AttackSpeed] = new ModifiableStat(1.33f);
        enemyStats[Traits.MovementSpeed] = new ModifiableStat(0.85f);
    }

    private void SetupButtonMapping()
    {
        // Map the Unique Name (%) of the button to the Trait it upgrades
        var buttonMap = new Dictionary<string, Traits>
        {
            { "%PlayerLifeButton", Traits.Life },
            { "%PlayerAttackDamageButton", Traits.Damage },
            { "%PlayerAttackSpeedButton", Traits.AttackSpeed },
            { "%PlayerMovementSpeedButton", Traits.MovementSpeed }
        };

        foreach (var (uniqueName, trait) in buttonMap)
        {
            var btn = GetNode<Button>(uniqueName);
            // Use a lambda to pass the specific trait to a single shared function
            btn.Pressed += () => HandleUpgradeRequest(trait);
        }
    }

    private void HandleUpgradeRequest(Traits trait)
    {
        if (ExperienceManager.Instance.GetUnspentSkillPoints() <= 0) return;

        switch(trait) 
        {
            case Traits.Damage:
                playerStats[trait].AddFlat(1);
                break;
            case Traits.AttackSpeed:
                playerStats[trait].AddIncreased(0.01f);
                break;
            case Traits.Life:
                playerStats[trait].AddFlat(5);
                break;
            case Traits.MovementSpeed:
                var value = 0.01f;
                playerStats[trait].AddIncreased(value);

                break;
        }
        EmitSignal(SignalName.PlayerStatUpgraded, Variant.From(trait));
    }

    public Dictionary<Traits, ModifiableStat> GetplayerStats() => playerStats;
    public Dictionary<Traits, ModifiableStat> GetenemyStats() => enemyStats;
}
