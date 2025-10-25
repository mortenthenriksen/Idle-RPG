using Godot;


public partial class Statistics : Control
{
    [Signal]
    public delegate void PlayerStatUpgradedEventHandler(string statName);

    [Signal]
    public delegate void EnemyStatUpgradedEventHandler(float newHealth, float maxHealth);

    public static Statistics Instance { get; private set; }

    private Button playerLifeButton;

    public override void _Ready()
    {
        var statsNode = GetNode("/root/Main/UserInterface/Statistics");
        playerLifeButton = statsNode.GetNode<Button>("%PlayerLifeButton");
        playerLifeButton.Pressed += OnPlyerLifeButtonPressed;
    }

    private void OnPlyerLifeButtonPressed()
    {
        EmitSignal(SignalName.PlayerStatUpgraded, "Life");
    }

}
