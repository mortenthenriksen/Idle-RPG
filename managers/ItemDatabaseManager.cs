using Godot;
using Inventory;
using System.Collections.Generic;

public partial class ItemDatabaseManager : Node
{
    public static ItemDatabaseManager Instance { get; private set; }

    // Stores templates, not real items
    private Dictionary<string, Item> _templates = new();
    private RandomNumberGenerator _rng = new();

    public override void _Ready()
    {
        Instance = this;
        LoadFromCsv("res://data/items.csv");
    }

    private void LoadFromCsv(string path)
    {
        var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        if (file == null) { GD.PrintErr($"ItemDatabaseManager: Could not open {path}"); return; }

        file.GetCsvLine(); // skip header

        while (!file.EofReached())
        {
            var cols = file.GetCsvLine();
            if (cols.Length < 12 || string.IsNullOrWhiteSpace(cols[0])) continue;

            var template = new Item
            {
                ItemName   = cols[1],
                Slot       = cols[2],             
                Icon       = GD.Load<Texture2D>(cols[3]),  
                DamageMin  = float.Parse(cols[4]),
                DamageMax  = float.Parse(cols[5]),
                HealthMin    = float.Parse(cols[6]),
                HealthMax    = float.Parse(cols[7]),
                DefenseMin = float.Parse(cols[8]),
                DefenseMax = float.Parse(cols[9]),
                MovementSpeedMin = float.Parse(cols[10]),
                MovementSpeedMax = float.Parse(cols[11]),
            };

            _templates[cols[1]] = template;
        }

        file.Close();
        GD.Print($"ItemDatabaseManager: Loaded {_templates.Count} templates.");
    }

    // Call this whenever you want a new item instance with freshly rolled stats
    public Item CreateItem(string name)
    {
        if (!_templates.TryGetValue(name, out var t))
        {
            GD.PrintErr($"ItemDatabaseManager: No template found for '{name}'");
            return null;
        }

        return new Item
        {
            ItemName   = t.ItemName,
            Icon       = t.Icon,
            Slot       = t.Slot,
            DamageMin  = t.DamageMin,
            DamageMax  = t.DamageMax,
            DefenseMin = t.DefenseMin,
            DefenseMax = t.DefenseMax,
            HealthMin  = t.HealthMin,
            HealthMax  = t.HealthMax,
            MovementSpeedMin   = t.MovementSpeedMin,
            MovementSpeedMax   = t.MovementSpeedMax,
            // Roll the actual stats
            Damage  = RollStat(t.DamageMin,  t.DamageMax),
            Defense = RollStat(t.DefenseMin, t.DefenseMax),
            Health  = RollStat(t.HealthMin,  t.HealthMax),
            MovementSpeed   = RollStat(t.MovementSpeedMin,   t.MovementSpeedMax),
        };
    }

    private float RollStat(float min, float max)
    {
        if (min == 0 && max == 0) return 0;
        return Mathf.Round(_rng.RandfRange(min, max));
    }

    // Keep GetByName for lookups that don't need a roll (e.g. checking slot type)
    public Item GetByName(string name) =>
        _templates.TryGetValue(name, out var t) ? t : null;

    public IEnumerable<string> GetAllItemNames() => _templates.Keys;
}