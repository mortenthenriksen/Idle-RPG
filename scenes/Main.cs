using Characters;
using Components;
using Godot;
using Managers;

namespace Main;

public partial class Main : Node
{
	private HealthNode playerHealth;
	private HealthNode enemyHealth;
	private MeeleeEnemy meeleeEnemy;
	private Player player;

	[Export] 
	private PackedScene meeleeEnemyScene;

	[Export] 
	private Vector2 enemySpawnPosition = new Vector2(504, 424);


	public override void _Ready()
	{
		playerHealth = GetNode<HealthNode>("Player/HealthNode");
		playerHealth.Connect("HealthChanged", new Callable(this, nameof(OnPlayerHealthChanged)));
		// playerHealth.Connect("Died", new Callable(this, nameof(OnPlayerDied)));

		enemyHealth = GetNode<HealthNode>("MeeleeEnemy/HealthNode");
		enemyHealth.Connect("HealthChanged", new Callable(this, nameof(OnEnemyHealthChanged)));

		meeleeEnemy = GetNode<MeeleeEnemy>("MeeleeEnemy");
		meeleeEnemy.Connect("EnemyDied", new Callable(this, nameof(OnEnemyDied)));

		player = GetNode<Player>("Player");

		UIManager.Instance.UpdatePlayerHealth(playerHealth.CurrentHealth);
		UIManager.Instance.UpdateEnemyHealth(enemyHealth.CurrentHealth);

	}

	private void OnPlayerHealthChanged(float newHealth)
	{
		UIManager.Instance.UpdatePlayerHealth(newHealth);
	}

	private void OnEnemyHealthChanged(float newHealth)
	{
		UIManager.Instance.UpdateEnemyHealth(newHealth);
	}

	private void OnEnemyDied()
	{
		// Spawn a new enemy
		var newEnemy = meeleeEnemyScene.Instantiate<MeeleeEnemy>();
		AddChild(newEnemy);

		// Set position
		float offset = player.Position.X + 100;
		GD.Print(player.GlobalPosition);
		newEnemy.GlobalPosition = new Vector2(enemySpawnPosition.X + offset, newEnemy.GlobalPosition.Y + 423);
		GD.Print(newEnemy.GlobalPosition);

		// Reconnect its signals
		var newEnemyHealth = newEnemy.GetNode<HealthNode>("HealthNode");
		newEnemyHealth.Connect(HealthNode.SignalName.HealthChanged, new Callable(this, nameof(OnEnemyHealthChanged)));
		newEnemyHealth.Connect(HealthNode.SignalName.Died, new Callable(this, nameof(OnEnemyDied)));

		// Update the UI for the new enemy
		UIManager.Instance.UpdateEnemyHealth(newEnemyHealth.CurrentHealth);

		GD.Print("New enemy spawned!");
	}
	
	// private void OnPlayerDied()
	// {
	// 	GD.Print("GAME OVER");
	// 	GetTree().Paused = true;
    // }

}
