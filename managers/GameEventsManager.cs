using System;
using System.Net.NetworkInformation;
using Characters;
using Components;
using Godot;
using Managers;

namespace Autoload;

public partial class GameEventsManager : Node
{
    private HealthNode playerHealth;
	private HealthNode enemyHealth;
	private MeeleeSkeleton meeleeEnemy;
	private Player player;
	private int increaseWaveValue = 1;

	[Export] 
	private PackedScene meeleeSkeletonScene;

	[Export] 
	private Vector2 enemySpawnPosition = new Vector2(504, 424);

    public static GameEventsManager Instance { get; private set; }

	public async override void _Ready()
	{
		Instance = this;
		player = GetNode<Player>("/root/Main/Player");

		playerHealth = GetNode<HealthNode>("/root/Main/Player/HealthNode");
		playerHealth.HealthChanged += OnPlayerHealthChanged;
		playerHealth.Died += OnPlayerDied;

		enemyHealth = GetNode<HealthNode>("/root/Main/MeeleeEnemy/HealthNode");
		enemyHealth.HealthChanged += OnEnemyHealthChanged;
		enemyHealth.Died += OnEnemyDied;

		await ToSignal(GetTree(), "process_frame"); // Wait one frame

		UIManager.Instance.UpdatePlayerHealth(playerHealth.CurrentHealth);
		UIManager.Instance.UpdateEnemyHealth(enemyHealth.CurrentHealth);
		UIManager.Instance.UpdateWaveCounter(WaveManager.Instance.currentWave);
		UIManager.Instance.UpdateTotalKillsCounter(KillTracker.Instance.GetTotalKills());
	}

    private void OnEnemyDied(CharacterBody2D characterBody2D)
	{
		WaveManager.Instance.IncreaseWaveCounter();
		KillTracker.Instance.IncreaseKillTracker((Enemy)characterBody2D);

		// change this to not just instantiate some Meeleskeleton, but rather the enemy for that place or a random one maybe
		var newEnemy = meeleeSkeletonScene.Instantiate<MeeleeSkeleton>();
		float offset = player.Position.X + 100;
		newEnemy.GlobalPosition = new Vector2(enemySpawnPosition.X + offset, newEnemy.GlobalPosition.Y + 423);

		AddChild(newEnemy);

		var newEnemyHealth = newEnemy.GetNode<HealthNode>("HealthNode");
		newEnemyHealth.HealthChanged += OnEnemyHealthChanged;
		newEnemyHealth.Died += OnEnemyDied;

		UIManager.Instance.UpdateEnemyHealth(newEnemyHealth.CurrentHealth);
		UIManager.Instance.UpdateWaveCounter(WaveManager.Instance.currentWave);
		UIManager.Instance.UpdateTotalKillsCounter(KillTracker.Instance.GetTotalKills());
    }


    private void OnPlayerDied(CharacterBody2D characterBody2D)
    {
		GD.Print("Player died!");
    }


    private void OnPlayerHealthChanged(float newHealth)
    {
		UIManager.Instance.UpdatePlayerHealth(newHealth);
    }

    private void OnEnemyHealthChanged(float newHealth)
    {
		UIManager.Instance.UpdateEnemyHealth(newHealth);
    }
}
