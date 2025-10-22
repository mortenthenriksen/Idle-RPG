using Godot;

namespace Managers;

public partial class UIManager : Node
{
    public static UIManager Instance { get; private set; }

    private Label playerHealthLabel;
    private Label enemyHealthLabel;

    public override void _Ready()
    {
        Instance = this;
        playerHealthLabel = GetNode<Label>("/root/Main/UserInterface/Panel/PlayerHealthLabel");
        enemyHealthLabel = GetNode<Label>("/root/Main/UserInterface/Panel/EnemyHealthLabel");
    }

    public void UpdatePlayerHealth(float newPlayerHealth)
    {
        if (playerHealthLabel != null)
            playerHealthLabel.Text = $"Life: {newPlayerHealth}";
    }

    public void UpdateEnemyHealth(float newEnemyHealth)
    {
        if (enemyHealthLabel != null)
            enemyHealthLabel.Text = $"Life: {newEnemyHealth}";
    }


}
