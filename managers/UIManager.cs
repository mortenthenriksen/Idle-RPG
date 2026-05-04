using System;
using Characters;
using Components;
using Godot;
using Upgrades;

namespace Managers;

public partial class UIManager : Node
{
    [Export]
    public Theme pixelKubastaFontTheme;

    public static UIManager Instance { get; private set; }

    // stats tab
    private Label skillPointsLabel;

    private Label playerHealthLabel;
    private Label playerAttackDamageLabel;
    private Label playerAttackSpeedLabel;
    private Label playerMovementSpeedLabel;
    private Label playerCritChanceLabel;
    private Label playerCritDamageLabel;

    private Label enemyHealthLabel;
    private Label enemyAttackDamageLabel;
    private Label enemyAttackSpeedLabel;
    private Label enemyMovementSpeedLabel;

    private Label waveCounterLabel;
    private Label totalKillsCounterLabel;
    private TextureProgressBar expBar;
    private Label expLabel;

    public override void _Ready()
    {
        Instance = this;
        waveCounterLabel = GetNode<Label>("/root/Main/UserInterface/TopPanel/WaveCounterLabel");
        totalKillsCounterLabel = GetNode<Label>("/root/Main/UserInterface/TopPanel/TotalKillsCounterLabel");
        expBar = GetNode<TextureProgressBar>("%ExpBar");
        expLabel = GetNode<Label>("%ExpLabel");

        var statsNode = GetNode("/root/Main/UserInterface/Statistics");
        skillPointsLabel = statsNode.GetNode<Label>("%SkillPointsLabel");

        playerHealthLabel = statsNode.GetNode<Label>("%PlayerHealthLabel");
        playerAttackDamageLabel = statsNode.GetNode<Label>("%PlayerAttackDamageLabel");
        playerAttackSpeedLabel = statsNode.GetNode<Label>("%PlayerAttackSpeedLabel");
        playerMovementSpeedLabel = statsNode.GetNode<Label>("%PlayerMovementSpeedLabel");
        playerCritChanceLabel = statsNode.GetNode<Label>("%PlayerCrititcalChanceLabel");
        playerCritDamageLabel = statsNode.GetNode<Label>("%PlayerCriticalDamageLabel");
        
        enemyHealthLabel = statsNode.GetNode<Label>("%EnemyHealthLabel");
        enemyAttackDamageLabel = statsNode.GetNode<Label>("%EnemyAttackDamageLabel");
        enemyAttackSpeedLabel = statsNode.GetNode<Label>("%EnemyAttackSpeedLabel");
        enemyMovementSpeedLabel = statsNode.GetNode<Label>("%EnemyMovementSpeedLabel");

        DamageManager.Instance.DamageDealt += DisplayDamageNumber;
    }

    public void RefreshPlayerStats(HealthNode healthNode)
    {
        var stats = Statistics.Instance.playerStats;
        UpdatePlayerHealth(healthNode.currentHealth, healthNode.maxHealth);
        UpdatePlayerAttackDamage((float)stats[Statistics.Traits.Damage].GetValue());
        UpdatePlayerAttackSpeed((float)stats[Statistics.Traits.AttackSpeed].GetValue());
        UpdatePlayerCritChance((float)stats[Statistics.Traits.CritChance].GetValue());
        UpdatePlayerCritDamage((float)stats[Statistics.Traits.CritDamage].GetValue());

        UpdateSkillPointsUI(ExperienceManager.Instance.GetUnspentSkillPoints());

        var baseSpeed    = stats[Statistics.Traits.MovementSpeed].BaseValue;
        var currentSpeed = (float)stats[Statistics.Traits.MovementSpeed].GetValue();
        UpdatePlayerMovementSpeed(currentSpeed / baseSpeed * 100f);
    }

    public void RefreshEnemyStats(HealthNode healthNode)
    {
        var stats = Statistics.Instance.enemyStats;
        UpdateEnemyHealth(healthNode.currentHealth, healthNode.maxHealth);
        UpdateEnemyAttackDamage((float)stats[Statistics.Traits.Damage].GetValue());
        UpdateEnemyAttackSpeed((float)stats[Statistics.Traits.AttackSpeed].GetValue());

        UpdateSkillPointsUI(ExperienceManager.Instance.GetUnspentSkillPoints());

        var baseSpeed    = stats[Statistics.Traits.MovementSpeed].BaseValue;
        var currentSpeed = (float)stats[Statistics.Traits.MovementSpeed].GetValue();
        UpdateEnemyMovementSpeed(currentSpeed / baseSpeed * 100f);
    }

    // PLAYER
    
    public void UpdatePlayerHealth(double newPlayerHealth, double playerMaxHealth)
    {
        playerHealthLabel.Text = $"{(int)newPlayerHealth:F0} / {(int)playerMaxHealth:F0}";
    }

    public void UpdatePlayerAttackDamage(float playerDamage)
    {
        playerAttackDamageLabel.Text = $"{playerDamage:F0}";
    }

    public void UpdatePlayerAttackSpeed(float attackSpeed)
    {
        playerAttackSpeedLabel.Text = $"{attackSpeed:F2}";
    }

    public void UpdatePlayerMovementSpeed(float playerMovementSpeed)
    {
        playerMovementSpeedLabel.Text = $"{playerMovementSpeed:F0}%";
    }

    public void UpdatePlayerCritChance(float val)
    {
        playerCritChanceLabel.Text = $"{val*100:F0}%";
    }

    public void UpdatePlayerCritDamage(float val)
    {
        playerCritDamageLabel.Text = $"{val*100:F0}%";
    }

    // ENEMY

    public void UpdateEnemyHealth(double newEnemyHealth, double enemyMaxHealth)
    {
        enemyHealthLabel.Text = $"{(int)newEnemyHealth:F0} / {(int)enemyMaxHealth:F0}";
    }

    public void UpdateEnemyAttackDamage(float enemyDamage)
    {
        enemyAttackDamageLabel.Text = $"{enemyDamage:F0}";
    }   
    public void UpdateEnemyAttackSpeed(float attackSpeed)
    {
        enemyAttackSpeedLabel.Text = $"{attackSpeed:F2}";
    }
    public void UpdateEnemyMovementSpeed(float enemyMovementSpeed)
    {
        enemyMovementSpeedLabel.Text = $"{enemyMovementSpeed:F0}%";
    }

    public void UpdateSkillPointsUI(int amount)
    {
        if (amount == 0)
        {
            skillPointsLabel.Visible = false;
        } else
        {
            skillPointsLabel.Visible = true;
            skillPointsLabel.Text = $"Points: {amount}";
        }
    }

    // GENERAL UI 
    public void UpdateWaveCounter(int changeWaveValue)
    {
        waveCounterLabel.Text = $"Wave: {changeWaveValue} / {WaveManager.Instance.maxWave}";
    }
    
    public void UpdateTotalKillsCounter(int totalKills)
    {
        totalKillsCounterLabel.Text = $"Total kills: {totalKills}";
    }

    public void UpdateExpUI(ulong expValue, ulong maxExp)
    {
        expBar.Value = expValue;
        expBar.MaxValue = maxExp;

        expLabel.Text = $"{expValue} / {maxExp}";
    }

    private void DisplayDamageNumber(CharacterBody2D source, CharacterBody2D target, float damageAmount, bool isCrit)
    {
        Vector2 sourcePosition = source.Position;
        Vector2 targetPosition = target.Position;

        var number = new Label
        {

            GlobalPosition = targetPosition + new Vector2(0, -80),
            Text = Mathf.RoundToInt(damageAmount).ToString(),
            ZIndex = 5,
            Theme = pixelKubastaFontTheme

        };

        
        if (isCrit)
        {
            number.AddThemeColorOverride("font_color", new Color("#ff0000")); // gold
            number.AddThemeFontSizeOverride("font_size", 48); // bigger
        }
        else number.AddThemeFontSizeOverride("font_size", 32);
        
        AddChild(number);

        // set up RNG
        Random rng = new Random();

        // --- BASE DIRECTION (away from attacker)
        Vector2 baseDir = (targetPosition - sourcePosition).Normalized();
        if (baseDir == Vector2.Zero)
            baseDir = Vector2.Up;

        // --- ANGLE JITTER
        // randomly rotate +/- ~20 degrees so they don't stack
        float angleJitterDeg = (float)(rng.NextDouble() * 40.0 - 20.0); // [-20, +20]
        float angleJitterRad = Mathf.DegToRad(angleJitterDeg);

        Vector2 dirJittered = baseDir.Rotated(angleJitterRad).Normalized();

        // --- DISTANCE VARIATION
        // how far it flies out from the hit point (in pixels)
        // e.g. between 35 and 70
        float dist = Mathf.Lerp(35f, 70f, (float)rng.NextDouble());

        // --- EXTRA UPWARD POP (varies so some float higher)
        float extraUp = Mathf.Lerp(10f, 40f, (float)rng.NextDouble());

        // final movement offset
        Vector2 targetOffset = dirJittered * dist - new Vector2(0, extraUp);

        // --- TIMING VARIATION
        // so not all numbers animate/fade at the exact same rate
        float moveTime = Mathf.Lerp(0.22f, 0.35f, (float)rng.NextDouble());
        float fadeTime = Mathf.Lerp(0.45f, 0.6f, (float)rng.NextDouble());

        // --- SCALE "POP" VARIATION
        float startScale = Mathf.Lerp(0.7f, 0.9f, (float)rng.NextDouble());
        float overshootScale = startScale + Mathf.Lerp(0.3f, 0.5f, (float)rng.NextDouble()); // e.g. 1.1 - 1.4 total
        float popTime = 0.08f;
        float settleTime = 0.08f;

        // initialize scale
        number.Scale = new Vector2(startScale, startScale);

        // Create tween
        var tween = GetTree().CreateTween();
        tween.SetParallel(true);

        // MOVE (diagonal drift away from attacker + float up)
        tween.TweenProperty(number, "position", number.Position + targetOffset, moveTime)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Sine);

        // FADE
        tween.TweenProperty(number, "modulate:a", 0, fadeTime)
            .SetEase(Tween.EaseType.In)
            .SetTrans(Tween.TransitionType.Sine);

        // POP: scale up fast then settle
        // we chain these sequentially (not parallel)
        var popTween = GetTree().CreateTween();
        popTween.TweenProperty(number, "scale", new Vector2(overshootScale, overshootScale), popTime)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Sine);

        popTween.TweenProperty(number, "scale", new Vector2(1f, 1f), settleTime)
            .SetEase(Tween.EaseType.In)
            .SetTrans(Tween.TransitionType.Sine);

        // CLEANUP
        // when the *fade/move* tween is done, kill the label
        tween.Finished += () => number.QueueFree();
    }



}
