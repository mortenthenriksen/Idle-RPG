using System;
using Godot;

namespace Managers;

public partial class UIManager : Node
{
    [Export]
    public Theme pixelKubastaFontTheme;

    public static UIManager Instance { get; private set; }
    
    private Label playerHealthLabel;
    private Label enemyHealthLabel;
    private Label waveCounterLabel;
    private Label totalKillsCounterLabel;

    public override void _Ready()
    {
        Instance = this;
        playerHealthLabel = GetNode<Label>("/root/Main/UserInterface/BottomPanel/PlayerHealthLabel");
        enemyHealthLabel = GetNode<Label>("/root/Main/UserInterface/BottomPanel/EnemyHealthLabel");
        waveCounterLabel = GetNode<Label>("/root/Main/UserInterface/TopPanel/WaveCounterLabel");
        totalKillsCounterLabel = GetNode<Label>("/root/Main/UserInterface/TopPanel/TotalKillsCounterLabel");


        DamageManager.Instance.DamageDealt += OnDamageDealt;
    }

    private void OnDamageDealt(CharacterBody2D source, CharacterBody2D target, float damageAmount)
    {
        DisplayDamageNumber(source.Position, target.Position, damageAmount);
    }

    public void UpdatePlayerHealth(float newPlayerHealth)
    {
        playerHealthLabel.Text = $"Life: {newPlayerHealth}";
    }

    public void UpdateEnemyHealth(float newEnemyHealth)
    {
        enemyHealthLabel.Text = $"Enemy life: {newEnemyHealth}";
    }

    public void UpdateWaveCounter(int changeWaveValue)
    {
        waveCounterLabel.Text = $"Wave: {changeWaveValue} / {WaveManager.Instance.maxWave}";
    }

    public void UpdateTotalKillsCounter(int totalKills)
    {
        totalKillsCounterLabel.Text = $"Total kills: {totalKills}";
    }

    private void DisplayDamageNumber(Vector2 sourcePosition, Vector2 targetPosition, float damageAmount)
    {
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
}
