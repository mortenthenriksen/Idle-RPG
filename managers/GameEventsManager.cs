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
	private Enemy enemy;
	private Statistics statistics; 
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
		enemy = GetTree().GetFirstNodeInGroup("enemy") as Enemy;

		playerHealth = GetNode<HealthNode>("/root/Main/Player/HealthNode");
        playerHealth.HealthChanged += OnPlayerHealthChanged;
		playerHealth.Died += OnPlayerDied;
		
        enemyHealth = GetNode<HealthNode>("/root/Main/MeeleeEnemy/HealthNode");
        enemyHealth.HealthChanged += OnEnemyHealthChanged;
		enemyHealth.Died += OnEnemyDied;

		statistics = GetNode<Statistics>("/root/Main/UserInterface/Statistics");
		statistics.PlayerStatUpgraded += OnPlayerStatUpgraded;

        await ToSignal(GetTree(), "process_frame"); // Wait one frame

        UpdateUI();
        // UIManager.Instance.UpdateGoldLabel(GoldManager.Instance.goldValue);
    }

    private void OnPlayerStatUpgraded(string statName)
    {
		if (statName == "Life")
		{
			playerHealth.IncreaseMaxHealth((float)4);
		}

		UpdateUI();
    }


    private void UpdateUI()
    {
        UIManager.Instance.UpdatePlayerHealth(playerHealth.currentHealth, playerHealth.maxHealth);
        UIManager.Instance.UpdateEnemyHealth(enemyHealth.currentHealth, enemyHealth.maxHealth);
        UIManager.Instance.UpdateWaveCounter(WaveManager.Instance.currentWave);
        UIManager.Instance.UpdateTotalKillsCounter(KillTracker.Instance.GetTotalKills());
        UIManager.Instance.UpdateExpUI(ExperienceManger.Instance.currentExp, ExperienceManger.Instance.maxExp);
    }


    private void OnEnemyDied(Enemy enemy)
	{
		WaveManager.Instance.IncreaseWaveCounter();
		KillTracker.Instance.IncreaseKillTracker(enemy);
		GoldManager.Instance.GetGoldFromEnemy(enemy);
		ExperienceManger.Instance.GainExp(enemy);

		// change this to not just instantiate some Meeleskeleton, but rather the enemy for that place or a random one maybe
		var newEnemy = meeleeSkeletonScene.Instantiate<MeeleeSkeleton>();
		// also, find another way to do this than using the player pos
		float offset = player.Position.X + 100;
		newEnemy.GlobalPosition = new Vector2(enemySpawnPosition.X + offset, newEnemy.GlobalPosition.Y + 423);

		AddChild(newEnemy);

		var newEnemyHealth = newEnemy.GetNode<HealthNode>("HealthNode");
		newEnemyHealth.HealthChanged += OnEnemyHealthChanged;
		newEnemyHealth.Died += OnEnemyDied;

		UpdateUI();
    }


    private void OnPlayerDied(CharacterBody2D characterBody2D)
    {
		GD.Print("Player died!");
    }

    private void OnPlayerHealthChanged(float newHealth, float maxHealth)
    {
		UIManager.Instance.UpdatePlayerHealth(newHealth, maxHealth);
    }

    private void OnEnemyHealthChanged(float newHealth, float maxHealth)
    {
		UIManager.Instance.UpdateEnemyHealth(newHealth, maxHealth);
    }
}
