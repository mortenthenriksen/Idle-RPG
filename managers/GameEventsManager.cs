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
	private Vector2 enemySpawnPosition = new Vector2(736, 481);
	

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
    }

    private void OnPlayerStatUpgraded(string statName)
	{	
		// TODO: make all of these scale somehow, do some testing to see how it fits with exp gain
		switch (statName)
			{
				case "Life":
					playerHealth.IncreaseMaxHealth(4f);
					break;

				case "AttackDamage":
					DamageManager.Instance.SetPlayerDamage(1f);
					break;

				case "AttackSpeed":
					// percentage increase of attackspeed, is usefull other places also
					DamageManager.Instance.SetPlayerAttackSpeed(1f);
					break;

				// case "MovementSpeed":
				// 	player.IncreaseMovementSpeed(10f);
				// 	break;

				// case "EnemyWeakness":
				// 	enemyHealth.DecreaseMaxHealth(3f);
				// 	break;

				default:
					GD.Print("Unknown stat upgrade: " + statName);
					break;
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
		UIManager.Instance.UpdatePlayerAttackDamage(DamageManager.Instance.GetPlayerDamage());
		UIManager.Instance.UpdatePlayerAttackSpeed(DamageManager.Instance.GetPlayerAttackSpeed());
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
		float offset = player.Position.X;
		newEnemy.GlobalPosition = new Vector2(enemySpawnPosition.X + offset, newEnemy.GlobalPosition.Y + 481);

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
