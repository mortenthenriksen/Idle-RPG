using Godot;


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

    public override void _Ready()
    {
        var statsNode = GetNode("/root/Main/UserInterface/Statistics");
        playerLifeButton = statsNode.GetNode<Button>("%PlayerLifeButton");
        playerAttackDamageButton = statsNode.GetNode<Button>("%PlayerAttackDamageButton");
        playerAttackSpeedButton = statsNode.GetNode<Button>("%PlayerAttackSpeedButton");

        playerLifeButton.Pressed += OnPlyerLifeButtonPressed;
        playerAttackDamageButton.Pressed += OnPlyerAttackDamageButtonPressed;
        playerAttackSpeedButton.Pressed += OnPlayerAttackSpeedButtonPressed;
        
    }

    private void OnPlyerLifeButtonPressed()
    {
        EmitSignal(SignalName.PlayerStatUpgraded, "Life");
    }

    private void OnPlyerAttackDamageButtonPressed()
    {
        EmitSignal(SignalName.PlayerStatUpgraded, "AttackDamage");
    }
    
    private void OnPlayerAttackSpeedButtonPressed()
    {
        EmitSignal(SignalName.PlayerStatUpgraded, "AttackSpeed");
    }

}
