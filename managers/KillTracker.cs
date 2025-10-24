using Autoload;
using Characters;
using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Managers
{
    public partial class KillTracker : Node
    {
        public static KillTracker Instance { get; private set; }

        private const string SavePath = "user://kill_data.json";

        // Store total kills per enemy type
        private Dictionary<string, int> killCounts = new();

        public override void _Ready()
        {
            Instance = this;
            LoadFromFile();
            // GD.Print($"[KillTracker] Loaded {killCounts.Count} enemy types from save.");
        }

        public void IncreaseKillTracker(CharacterBody2D characterBody2D)
        {
            string type = characterBody2D?.GetType().Name ?? "Unknown";

            if (!killCounts.ContainsKey(type))
                killCounts[type] = 0;

            killCounts[type]++;

            // GD.Print($"[KillTracker] {type} killed. Total: {killCounts[type]}");

            SaveToFile();
        }

        public int GetTotalKills()
        {
            int total = 0;
            foreach (var count in killCounts.Values)
                total += count;
            return total;
        }

        public int GetKillsForType(string enemyType)
        {
            return killCounts.ContainsKey(enemyType) ? killCounts[enemyType] : 0;
        }

        public void ClearAllData()
        {
            killCounts.Clear();
            SaveToFile();
        }

        private void SaveToFile()
        {
            try
            {
                string json = JsonSerializer.Serialize(killCounts, new JsonSerializerOptions { WriteIndented = true });
                using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
                file.StoreString(json);
                // GD.Print("[KillTracker] Saved kill counts to file.");
            }
            catch (Exception e)
            {
                GD.PrintErr($"[KillTracker] Save failed: {e.Message}");
            }
        }

        private void LoadFromFile()
        {
            if (!FileAccess.FileExists(SavePath))
                return;

            try
            {
                using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
                string json = file.GetAsText();
                killCounts = JsonSerializer.Deserialize<Dictionary<string, int>>(json) ?? new();
            }
            catch (Exception e)
            {
                GD.PrintErr($"[KillTracker] Load failed: {e.Message}");
            }
        }
    }
}
