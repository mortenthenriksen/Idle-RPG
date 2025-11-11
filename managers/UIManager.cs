using System;
using Characters;
using Components;
using Godot;

namespace Managers;

public partial class UIManager : Node
{
    [Export]
    public Theme pixelKubastaFontTheme;

    [Export]
    public PackedScene coinPackedScene;

    public static UIManager Instance { get; private set; }

    private Label playerHealthLabel;
    private Label enemyHealthLabel;

    // stats tab
    private Label playerLifeLabel;
    private Label enemyLifeLabel;
    private Label playerAttackDamageLabel;
    private Label playerAttackSpeedLabel;
    private Label playerMovementSpeedLabel;
    private Label skillPointsLabel;

    private Label waveCounterLabel;
    private Label totalKillsCounterLabel;
    private Label goldLabel;
    private TextureProgressBar expBar;
    private Label expLabel;

    public override void _Ready()
    {
        Instance = this;
        playerHealthLabel = GetNode<Label>("/root/Main/UserInterface/BottomPanel/PlayerHealthLabel");
        enemyHealthLabel = GetNode<Label>("/root/Main/UserInterface/BottomPanel/EnemyHealthLabel");
        waveCounterLabel = GetNode<Label>("/root/Main/UserInterface/TopPanel/WaveCounterLabel");
        totalKillsCounterLabel = GetNode<Label>("/root/Main/UserInterface/TopPanel/TotalKillsCounterLabel");
        goldLabel = GetNode<Label>("%GoldLabel");
        expBar = GetNode<TextureProgressBar>("%ExpBar");
        expLabel = GetNode<Label>("%ExpLabel");

        var statsNode = GetNode("/root/Main/UserInterface/Statistics");
        playerLifeLabel = statsNode.GetNode<Label>("%PlayerLifeLabel");
        enemyLifeLabel = statsNode.GetNode<Label>("%EnemyLifeLabel");
        playerAttackDamageLabel = statsNode.GetNode<Label>("%PlayerAttackDamageLabel");
        playerAttackSpeedLabel = statsNode.GetNode<Label>("%PlayerAttackSpeedLabel");
        playerMovementSpeedLabel = statsNode.GetNode<Label>("%PlayerMovementSpeedLabel");
        skillPointsLabel = statsNode.GetNode<Label>("%SkillPointsLabel");

        DamageManager.Instance.DamageDealt += DisplayDamageNumber;
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

    public void UpdateExpUI(ulong expValue, ulong maxExp)
    {
        expBar.Value = expValue;
        expBar.MaxValue = maxExp;

        expLabel.Text = $"{expValue} / {maxExp}";
    }

    public void UpdateGoldUI(Vector2 positionEnemy, ulong goldValue)
    {
        goldLabel.Text = $"Gold: {goldValue}";

        DisplayGoldCoin(positionEnemy);
    }

    private void DisplayGoldCoin(Vector2 positionEnemy)
    {
        // Instantiate coin
        var goldCoin = coinPackedScene.Instantiate<Node2D>();
        goldCoin.GlobalPosition = positionEnemy;
        AddChild(goldCoin);

        // Create a tween for animation
        var tween = GetTree().CreateTween();
        tween.SetEase(Tween.EaseType.Out);
        tween.SetTrans(Tween.TransitionType.Quad);

        // Animate upward jump
        Vector2 startPos = goldCoin.GlobalPosition;
        Vector2 peakPos = startPos + new Vector2(0, -40); // move up 40px

        tween.TweenProperty(goldCoin, "global_position", peakPos, 0.3f);

        // Bounce down slightly
        tween.TweenProperty(goldCoin, "global_position", startPos + new Vector2(0, -10), 0.25f);


        // Fade out before removing
        if (goldCoin is CanvasItem canvasItem)
        {
            tween.Parallel().TweenProperty(canvasItem, "modulate:a", 0f, 0.4f);
        }

        // QueueFree when done
        tween.TweenCallback(Callable.From(() => goldCoin.QueueFree()));
    }

    public void UpdatePlayerHealth(float newPlayerHealth, float playerMaxHealth)
    {
        playerHealthLabel.Text = $"Life: {newPlayerHealth}";
        playerLifeLabel.Text = $"{newPlayerHealth} / {playerMaxHealth}";
    }

    public void UpdatePlayerAttackDamage(float playerDamage)
    {
        playerAttackDamageLabel.Text = $"{playerDamage}";
    }

    public void UpdatePlayerAttackSpeed(float attackSpeed)
    {
        playerAttackSpeedLabel.Text = $"{attackSpeed:F2}";
    }

    public void UpdatePlayerMovementSpeed(float playerMovementSpeed)
    {
        playerMovementSpeedLabel.Text = $"{playerMovementSpeed:F0}%";
    }


    public void UpdateEnemyHealth(float newEnemyHealth, float enemyMaxHealth)
    {
        enemyHealthLabel.Text = $"Enemy life: {newEnemyHealth}";
        enemyLifeLabel.Text = $"{newEnemyHealth} / {enemyMaxHealth}";
    }

    public void UpdateWaveCounter(int changeWaveValue)
    {
        waveCounterLabel.Text = $"Wave: {changeWaveValue} / {WaveManager.Instance.maxWave}";
    }

    public void UpdateTotalKillsCounter(int totalKills)
    {
        totalKillsCounterLabel.Text = $"Total kills: {totalKills}";
    }


    private void DisplayDamageNumber(CharacterBody2D source, CharacterBody2D target, float damageAmount)
    {
        Vector2 sourcePosition = source.Position;
        Vector2 targetPosition = target.Position;

        var number = new Label
        {
            GlobalPosition = targetPosition + new Vector2(0, -80),
            Text = Mathf.RoundToInt(damageAmount).ToString(),
            ZIndex = 5
        };

        number.Theme = pixelKubastaFontTheme;
        number.AddThemeFontSizeOverride("font_size", 32);
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
