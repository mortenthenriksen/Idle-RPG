using Characters;
using Godot;
using System.Collections.Generic;

namespace Components
{
    public partial class HealthNode : Node
    {
        [Export]
        public float maxHealth = 100;
        public float currentHealth { get; private set; }
        public bool IsDead => currentHealth <= 0;

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
            currentHealth = maxHealth;
        }

        public override void _Process(double delta)
        {
            // Remove old damage events (older than 1 second)
            float currentTime = (float)Time.GetTicksMsec() / 1000f;
            damageHistory.RemoveAll(entry => currentTime - entry.time > 1f);

            // Update DPS periodically
            dpsTimer += (float)delta;
            if (dpsTimer >= dpsUpdateInterval)
            {
                dpsTimer = 0f;
                float dps = ComputeDPS();
                if (dps > 0f)
                {
                    var owner = GetParent<CharacterBody2D>();
                    EmitSignal(SignalName.DPSUpdated, owner, dps);
                    if (owner.Name != "Player")
                    {
                        GD.Print($"[DPS] {GetParent().Name} taking {dps:F1} DPS");
                    }
                }
            }
        }

        public void ApplyDamage(float amount)
        {
            currentHealth -= amount;

            // Record damage timestamp
            float currentTime = (float)Time.GetTicksMsec() / 1000f;
            damageHistory.Add((currentTime, amount));

            EmitSignal(SignalName.HealthChanged, currentHealth, maxHealth);

            if (currentHealth <= 0)
            {
                var owner = GetParent<CharacterBody2D>();
                EmitSignal(SignalName.Died, owner);
            }
        }

        private float ComputeDPS()
        {
            float totalDamage = 0f;
            foreach (var entry in damageHistory)
                totalDamage += entry.damage;

            // Damage per second (over last 1s)
            return totalDamage / 1f;
        }

        public void IncreaseMaxHealth(float value)
        {
            maxHealth += value;
        }

        public void ResetHealth()
        {
            currentHealth = maxHealth;
        }
    }
}
