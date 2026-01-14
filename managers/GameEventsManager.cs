using System;
using System.Collections.Generic;
using Characters;
using Components;
using Godot;
using Managers;
using UI;

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
	private ulong enemiesKilledThisPrestige = 0;

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
		
		statistics = GetNode<Statistics>("/root/Main/UserInterface/Statistics");
		statistics.PlayerStatUpgraded += OnPlayerSkillPointUsed;

		DamageManager.Instance.AttackBlocked += OnAttackBlocked;

		await ToSignal(GetTree(), "process_frame"); // Wait one frame

		SpawnEnemy();
        UpdateUI();
    }

    private void OnAttackBlocked(CharacterBody2D source, CharacterBody2D target)
    {
        TemporaryBuffsForPlayer("AttackDamageMultiplicative", 2);
    }

	private void TemporaryBuffsForPlayer(string statName, float value)
	{
		switch (statName)
		{
			case "AttackDamageMultiplicative":
				DamageManager.Instance.MultiplicativeIncreasePlayerDamage(value);
				break;

			default:
				GD.Print("Unknown temp buff: " + statName);
				break;
		}	
		UpdateUI();
	}

	private void StatsGainedFromAncestry()
	{
		// casting type just to be nice
		Dictionary<string, (float, int, int)> ancestryDict = Ancestry.Instance.GetAncestryDict();

	}
	
	// could also add value param here, so that GameEventManager isnt responsible for this
    private void OnPlayerSkillPointUsed(string statName)
	{
		ExperienceManager.Instance.DecreaseUnspentSkillPoints();
		// TODO: make all of these scale somehow, do some testing to see how it fits with exp gain
		switch (statName)
			{
				case "Life":
					playerHealth.IncreaseMaxHealth(4f);
					break;

				case "AttackDamageAdditive":
					DamageManager.Instance.AdditiveIncreasePlayerDamage(1f);
					break;

				case "AttackSpeed":
					// percentage increase of attackspeed, is usefull other places also
					DamageManager.Instance.IncreasePlayerAttackSpeed(1f);
					break;

				case "MovementSpeed":
					player.IncreaseMovementSpeed(0.10f);
					break;

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
		UIManager.Instance.UpdateExpUI((ulong)ExperienceManager.Instance.currentExp, (ulong)ExperienceManager.Instance.GetExpRequiredForNextLevel());
		UIManager.Instance.UpdatePlayerAttackDamage(DamageManager.Instance.GetPlayerDamage());
		UIManager.Instance.UpdatePlayerAttackSpeed(DamageManager.Instance.GetPlayerAttackSpeed());
		UIManager.Instance.UpdateSkillPointsUI(ExperienceManager.Instance.GetUnspentSkillPoints());

		float playerMovementSpeed = player.GetPlayerMovementSpeed();
		float basePlayerMovementSpeed = player.GetPlayerBaseMovementSpeed();
		float movementSpeedPercentage = playerMovementSpeed / basePlayerMovementSpeed * 100; 
		UIManager.Instance.UpdatePlayerMovementSpeed(movementSpeedPercentage);
    }

	private void SpawnEnemy()
	{
		// change this to not just instantiate some Meeleskeleton, but rather the enemy for that place or a random one maybe
		// var newEnemy = ResourceLoader.Load<PackedScene>("res://characters/enemies/meelee enemy/SlimeBoss.tscn").Instantiate<SlimeBoss>();
		var newEnemy = ResourceLoader.Load<PackedScene>("res://characters/enemies/meelee enemy/MeeleeSkeleton.tscn").Instantiate<MeeleeSkeleton>();
		// also, find another way to do this than using the player pos
		float offset = player.Position.X - 500;
		newEnemy.GlobalPosition = new Vector2(enemySpawnPosition.X + offset, newEnemy.GlobalPosition.Y + 481);

		AddChild(newEnemy);

		enemyHealth = newEnemy.GetNode<HealthNode>("HealthNode");
		enemyHealth.HealthChanged += OnEnemyHealthChanged;
		enemyHealth.Died += OnEnemyDied;
	}

	private void OnEnemyDied(CharacterBody2D enemy)
	{	
		WaveManager.Instance.IncreaseWaveCounter();
		KillTracker.Instance.IncreaseKillTracker(enemy);
		GoldManager.Instance.GetGoldFromEnemy(enemy);
		ExperienceManager.Instance.AddExp(enemy);

		SpawnEnemy();

		UpdateUI();
	}


    private void OnPlayerDied(CharacterBody2D characterBody2D)
    {
		GD.Print("Player died!");
		playerHealth.ResetHealth();
		
		UIManager.Instance.UpdatePlayerHealth(playerHealth.currentHealth, playerHealth.maxHealth);
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
