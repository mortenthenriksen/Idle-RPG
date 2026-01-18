using System;
using System.Collections.Generic;
using Characters;
using Components;
using Godot;
using Managers;
using Upgrades;

namespace Autoload;

public partial class GameEventsManager : Node
{
	[Export]
	private Vector2 enemySpawnPosition = new Vector2(736, 481);
    
    public static GameEventsManager Instance { get; private set; }
	private Dictionary<Statistics.Traits, ModifiableStat> playerStats;

	private HealthNode playerHealth;
	private HealthNode enemyHealth;
	private MeeleeSkeleton meeleeEnemy;
	private Player player;
	private Enemy enemy;
	private Statistics statistics;
	private int increaseWaveValue = 1;
	private ulong enemiesKilledThisPrestige = 0;
	private bool blockedDamageBuffIsActive;


	public async override void _Ready()
    {
		Instance = this;
		await ToSignal(GetTree(), "process_frame"); // Wait one frame
		player = GetTree().GetFirstNodeInGroup("player") as Player;
		enemy = GetTree().GetFirstNodeInGroup("enemy") as Enemy;

		playerHealth = GetNode<HealthNode>("/root/Main/Player/HealthNode");
        playerHealth.HealthChanged += OnPlayerHealthChanged;
		playerHealth.Died += OnPlayerDied;
		
		statistics = GetNode<Statistics>("/root/Main/UserInterface/Statistics");
		statistics.PlayerStatUpgraded += OnPlayerSkillPointUsed;

		DamageManager.Instance.AttackBlocked += OnAttackBlocked;
		Ancestry.Instance.AncestryUpdated += StatsGainedFromAncestry;

		SpawnEnemy();
		
		playerStats = statistics.GetBasePlayerStats();

        UpdateUI();

    }

    private void OnAttackBlocked(CharacterBody2D source, CharacterBody2D target)
    {
        TemporaryBuffsForPlayer("AttackDamageMultiplicative", 1);
    }

	// make these go away after some time
	private void TemporaryBuffsForPlayer(string statName, float value)
	{
		switch (statName)
		{
			case "AttackDamageMultiplicative":
				if (!blockedDamageBuffIsActive)
				{
					blockedDamageBuffIsActive = true;

					playerStats[Statistics.Traits.Damage].AddPercent(value);

					Timer timer = CreateTempBuffTimer(5.0f);

					timer.Timeout += () => {
						playerStats[Statistics.Traits.Damage].RemovePercent(value);
						blockedDamageBuffIsActive = false;
						timer.QueueFree(); 
						UpdateUI();
					};
				}
				break;

			default:
				GD.Print("Unknown temp buff: " + statName);
				break;
		}	
		UpdateUI();
	}

    private Timer CreateTempBuffTimer(float buffDurationTime)
    {
		Timer timer = new Timer();
		AddChild(timer);
		timer.WaitTime = buffDurationTime;
		timer.OneShot = true;
		timer.Start();
		return timer;
    }

    private void StatsGainedFromAncestry(string nameOfAncestor)
	{
		// var accumulatedAncestorStats = Ancestry.Instance.GetAncestryDictValues();
		// foreach (var kvp in accumulatedAncestorStats)
		// 	{
		// 		if (statUpdaters.TryGetValue(kvp.Key, out var updateAction))
		// 		{
		// 			updateAction(kvp.Value); 
		// 			GD.Print($"Applied Ancestry: {kvp.Key} updated by {kvp.Value}");
		// 		}
		// 	}
		UpdateUI();
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
					DamageManager.Instance.IncreasePlayerAttackSpeed(0.1f);
					break;

				case "MovementSpeed":
					player.IncreaseMovementSpeed(0.1f);
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
		UIManager.Instance.UpdatePlayerAttackDamage(playerStats[Statistics.Traits.Damage].GetValue());
		UIManager.Instance.UpdatePlayerAttackSpeed(playerStats[Statistics.Traits.AttackSpeed].GetValue());
		UIManager.Instance.UpdateSkillPointsUI(ExperienceManager.Instance.GetUnspentSkillPoints());

		var basePlayerMovementSpeed = playerStats[Statistics.Traits.MovementSpeed].BaseValue;
		var playerMovementSpeed = playerStats[Statistics.Traits.MovementSpeed].GetValue();
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
