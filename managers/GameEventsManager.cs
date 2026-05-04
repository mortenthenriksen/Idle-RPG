using Characters;
using Components;
using Godot;
using Managers;
using Upgrades;
using static Upgrades.Statistics;

namespace Autoload;

public partial class GameEventsManager : Node
{
	
    [Signal]
    public delegate void PlayerMovementSpeedChangedEventHandler(float percentageIncrease);

    [Export]
    private Vector2 enemySpawnPosition = new Vector2(736, 481);

    public static GameEventsManager Instance { get; private set; }

    private HealthNode playerHealthNode;
    private HealthNode enemyHealthNode;
    private Player player;
    private Enemy enemy;
    private bool blockedDamageBuffIsActive;

    public async override void _Ready()
    {
        Instance = this;
        await ToSignal(GetTree(), "process_frame");

        player = GetTree().GetFirstNodeInGroup("player") as Player;

        playerHealthNode = player.GetNode<HealthNode>("HealthNode");
		playerHealthNode.HealthChanged += OnPlayerHealthChanged;
        playerHealthNode.Died += OnPlayerDied;

        Statistics.Instance.PlayerStatUpgraded += OnPlayerStatUpgraded;
        Statistics.Instance.EnemyStatUpgraded  += OnEnemyStatUpgraded;
        DamageManager.Instance.AttackBlocked   += OnAttackBlocked;
        Ancestry.Instance.AncestryUpdated      += OnAncestryUpdated;

        SpawnEnemy();
		UIManager.Instance.RefreshPlayerStats(playerHealthNode);

        UpdateGeneralUI();
    }

    // ── Health ───────────────────────────────────────────────────────────────

    private void OnPlayerHealthChanged(float newHealth, float maxHealth)
    {
        UIManager.Instance.UpdatePlayerHealth(newHealth, maxHealth);
    }

    private void OnPlayerDied(CharacterBody2D body)
    {
        playerHealthNode.ResetHealth();
    }

    private void OnEnemyHealthChanged(float newHealth, float maxHealth)
    {
        UIManager.Instance.UpdateEnemyHealth(newHealth, maxHealth);
    }

    private async void OnEnemyDied(CharacterBody2D enemy)
    {
        WaveManager.Instance.IncreaseWaveCounter();
        KillTracker.Instance.IncreaseKillTracker(enemy);
        ExperienceManager.Instance.AddExp(enemy);

        UpdateGeneralUI();
        
        await ToSignal(enemy, Enemy.SignalName.DeathAnimationFinished);
        SpawnEnemy();
    }

    private void UpdateGeneralUI()
    {
        UIManager.Instance.UpdateWaveCounter(WaveManager.Instance.currentWave);
        UIManager.Instance.UpdateTotalKillsCounter(KillTracker.Instance.GetTotalKills());
        UIManager.Instance.UpdateExpUI((ulong)ExperienceManager.Instance.currentExp, (ulong)ExperienceManager.Instance.GetExpRequiredForNextLevel());
    }

    // ── Stats ────────────────────────────────────────────────────────────────


    private void OnPlayerStatUpgraded(Traits trait, float value)
    {
        UIManager.Instance.RefreshPlayerStats(playerHealthNode);

        if (trait == Traits.MovementSpeed)
            EmitSignal(SignalName.PlayerMovementSpeedChanged, value);

        if (trait == Traits.Health)
            playerHealthNode.GetMaxHealthFromStatsDict();
    }

    private void OnEnemyStatUpgraded(Traits trait, float value)
	{
		UIManager.Instance.RefreshEnemyStats(enemyHealthNode);
		if (trait == Traits.Health)
            enemyHealthNode.GetMaxHealthFromStatsDict();
	}

    private void OnAncestryUpdated(string nameOfAncestor)
    {
        UIManager.Instance.RefreshPlayerStats(playerHealthNode);
    }

    // ── Buffs ────────────────────────────────────────────────────────────────

    private void OnAttackBlocked(CharacterBody2D source, CharacterBody2D target)
    {
        if (blockedDamageBuffIsActive) return;

        blockedDamageBuffIsActive = true;
        Statistics.Instance.playerStats[Traits.Damage].AddMore(1);
        UIManager.Instance.RefreshPlayerStats(playerHealthNode);

        var timer = new Timer { WaitTime = 5.0f, OneShot = true };
        AddChild(timer);
        timer.Timeout += () =>
        {
            Statistics.Instance.playerStats[Traits.Damage].RemoveMore(1);
            blockedDamageBuffIsActive = false;
            UIManager.Instance.RefreshPlayerStats(playerHealthNode);
            timer.QueueFree();
        };
        timer.Start();
    }

    // ── Spawning ─────────────────────────────────────────────────────────────

    private void SpawnEnemy()
    {
        enemy = ResourceLoader.Load<PackedScene>("res://characters/enemies/meelee enemy/MeeleeSkeleton.tscn").Instantiate<MeeleeSkeleton>();

        float offset = player.Position.X - 500;
        enemy.GlobalPosition = new Vector2(enemySpawnPosition.X + offset, enemySpawnPosition.Y);
        AddChild(enemy);

        enemyHealthNode = enemy.GetNode<HealthNode>("HealthNode");
        enemyHealthNode.HealthChanged += OnEnemyHealthChanged;
        enemyHealthNode.Died += OnEnemyDied;
		UIManager.Instance.RefreshEnemyStats(enemyHealthNode);
    }

    public void EmitOnPlayerMovementSpeedChanged(float percentage)
    {
        EmitSignal(SignalName.PlayerMovementSpeedChanged, percentage);
    }

}