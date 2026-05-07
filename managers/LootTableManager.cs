using Godot;
using Inventory;
using System;
using System.Collections.Generic;

namespace Managers;

/// <summary>
/// Weighted loot table. Items with higher tier and type numbers are rarer.
/// Weight formula: 1.0 / (tier * type), so tier1_type1 = most common.
/// </summary>
public partial class LootTableManager : Node
{
    public static LootTableManager Instance { get; private set; }

    // Chance that any drop happens at all (0-1)
    [Export] public float DropChance = 0.5f;

    private record LootEntry(string ItemName, float Weight);
    private List<LootEntry> _pool = new();
    private RandomNumberGenerator _rng = new();
    private float _totalWeight;

    public override void _Ready()
    {
        Instance = this;
        BuildPool();
    }

    private void BuildPool()
    {
        _pool.Clear();
        _totalWeight = 0f;

        foreach (var name in ItemDatabaseManager.Instance.GetAllItemNames())
        {
            float weight = ComputeWeight(name);
            _pool.Add(new LootEntry(name, weight));
            _totalWeight += weight;
        }

        GD.Print($"[LootTableManager] Built pool with {_pool.Count} entries, total weight {_totalWeight:F1}");
    }

    /// <summary>
    /// Parses tier and type from names like "wooden_boots_type2_tier1".
    /// Weight = 1 / (tier * type). Unknown names get a low default weight.
    /// </summary>
    private float ComputeWeight(string itemName)
    {
        int tier = ParseSegment(itemName, "tier");
        int type = ParseSegment(itemName, "type");

        if (tier <= 0) tier = 1;
        if (type <= 0) type = 1;

        return 1f / (tier * type + 0.5f);
    }

    private int ParseSegment(string name, string prefix)
    {
        int idx = name.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
        if (idx < 0) return -1;

        // grab the digits immediately after the prefix
        int start = idx + prefix.Length;
        int end = start;
        while (end < name.Length && char.IsDigit(name[end]))
            end++;

        if (end == start) return -1;
        return int.Parse(name[start..end]);
    }

    /// <summary>
    /// Returns a randomly chosen item (or null if the drop roll fails).
    /// </summary>
    public Item? RollDrop()
    {
        if (_rng.Randf() > DropChance) return null;
        if (_pool.Count == 0) return null;

        float roll = _rng.RandfRange(0f, _totalWeight);
        float cumulative = 0f;

        foreach (var entry in _pool)
        {
            cumulative += entry.Weight;
            if (roll <= cumulative)
                return ItemDatabaseManager.Instance.CreateItem(entry.ItemName);
        }

        // Fallback: last entry
        return ItemDatabaseManager.Instance.CreateItem(_pool[^1].ItemName);
    }
}