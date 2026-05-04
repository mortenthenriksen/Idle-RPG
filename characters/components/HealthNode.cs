using Godot;
using System;
using System.Collections.Generic;
using Upgrades;

namespace Components;

public partial class HealthNode : Node
{
    public double maxHealth;
    public double currentHealth;
    public bool IsDead => currentHealth <= 0;
    public bool isDying = false;

    [Signal]
    public delegate void HealthChangedEventHandler(float newHealth, float maxHealth);

    [Signal]
    public delegate void DiedEventHandler(CharacterBody2D body);

    [Signal]
    public delegate void DPSUpdatedEventHandler(CharacterBody2D body, float dps);

    private List<(float time, float damage)> damageHistory = new();
    private float dpsUpdateInterval = 1f;
    private float dpsTimer = 0f;

    public override void _Ready()
    {
        var parent = GetParent();

        if (Statistics.Instance != null)
        {
            if (parent.IsInGroup("player"))
                maxHealth = Statistics.Instance.playerStats[Statistics.Traits.Health].GetValue();
            else if (parent.IsInGroup("enemy"))
                maxHealth = Statistics.Instance.enemyStats[Statistics.Traits.Health].GetValue();
        }
        currentHealth = maxHealth;
    }

    public void InitializeHealth(double value)
    {
        maxHealth     = value;
        currentHealth = maxHealth;
    }

    public override void _Process(double delta)
    {
        float currentTime = (float)Time.GetTicksMsec() / 1000f;
        damageHistory.RemoveAll(entry => currentTime - entry.time > 1f);

        dpsTimer += (float)delta;
        if (dpsTimer >= dpsUpdateInterval)
        {
            dpsTimer = 0f;
            float dps = ComputeDPS();
            if (dps > 0f)
            {
                var owner = GetParent<CharacterBody2D>();
                EmitSignal(SignalName.DPSUpdated, owner, dps);
            }
        }
    }

    public void ApplyDamage(float amount)
    {
        if (isDying) return;

        currentHealth = Math.Max(0, currentHealth - amount);

        float currentTime = (float)Time.GetTicksMsec() / 1000f;
        damageHistory.Add((currentTime, amount));

        EmitSignal(SignalName.HealthChanged, currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            isDying = true;
            var owner = GetParent<CharacterBody2D>();
            EmitSignal(SignalName.Died, owner);
        }
    }

    public void GetMaxHealthFromStatsDict()
    {
        var parent = GetParent();
        double newMax;

        if (parent.IsInGroup("player"))
            newMax = Statistics.Instance.playerStats[Statistics.Traits.Health].GetValue();
        else if (parent.IsInGroup("enemy"))
        {
            newMax = Statistics.Instance.enemyStats[Statistics.Traits.Health].GetValue();
        }
        else
            return;

        double delta = newMax - maxHealth;
        maxHealth = newMax;
        currentHealth = Math.Clamp(currentHealth + delta, 0, maxHealth);
        EmitSignal(SignalName.HealthChanged, currentHealth, maxHealth);
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isDying = false;
        EmitSignal(SignalName.HealthChanged, currentHealth, maxHealth);
    }

    private float ComputeDPS()
    {
        float totalDamage = 0f;
        foreach (var entry in damageHistory)
            totalDamage += entry.damage;
        return totalDamage / 1f;
    }
}