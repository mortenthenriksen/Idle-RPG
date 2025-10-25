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
    private Label playerLifeLabel;
    private Label enemyLifeLabel;
    private Label enemyHealthLabel;
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

        DamageManager.Instance.DamageDealt += DisplayDamageNumber;
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

        // 🔹 Calculate direction away from attacker
        var direction = (targetPosition - sourcePosition).Normalized();
        if (direction == Vector2.Zero)
            direction = Vector2.Up; // fallback if they overlap

        // 🔹 Add a small random offset for variety
        Random random = new Random();
        float horizontalJitter = (float)(random.NextDouble() * 0.4 - 0.2f); // small random rotation
        direction = direction.Rotated(horizontalJitter);
        
        // 🔹 Define movement target (flies away + slightly upward)
        float distance = 50f;
        Vector2 targetOffset = direction * distance - new Vector2(0, 30);

        // Create tween
        var tween = GetTree().CreateTween();
        tween.SetParallel(true);

        // Move diagonally away from attacker and slightly up
        tween.TweenProperty(number, "position", number.Position + targetOffset, 0.3f)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Sine);

        // Fade out while moving
        tween.TweenProperty(number, "modulate:a", 0, 0.6f)
            .SetEase(Tween.EaseType.In)
            .SetTrans(Tween.TransitionType.Sine);

        // Slight "pop" scale effect before fading
        number.Scale = new Vector2(0.7f, 0.7f);
        tween.TweenProperty(number, "scale", new Vector2(1.2f, 1.2f), 0.1f)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Sine);
        tween.TweenProperty(number, "scale", new Vector2(1f, 1f), 0.1f)
            .SetEase(Tween.EaseType.In)
            .SetTrans(Tween.TransitionType.Sine)
            .SetDelay(0.1f);

        // Cleanup when finished
        tween.Finished += () => number.QueueFree();
    }

    public void UpdatePlayerHealth(float newPlayerHealth, float playerMaxHealth)
    {
        playerHealthLabel.Text = $"Life: {newPlayerHealth}";
        playerLifeLabel.Text = $"{newPlayerHealth} / {playerMaxHealth}";
    }

    public void UpdateEnemyHealth(float newEnemyHealth, float enemyMaxHealth)
    {
        enemyHealthLabel.Text = $"Enemy life: {newEnemyHealth}";
        enemyLifeLabel.Text = enemyMaxHealth.ToString();
    }

    public void UpdateWaveCounter(int changeWaveValue)
    {
        waveCounterLabel.Text = $"Wave: {changeWaveValue} / {WaveManager.Instance.maxWave}";
    }

    public void UpdateTotalKillsCounter(int totalKills)
    {
        totalKillsCounterLabel.Text = $"Total kills: {totalKills}";
    }
}
