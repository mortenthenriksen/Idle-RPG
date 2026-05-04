using Godot;

namespace Inventory;

public partial class ItemTooltip : PanelContainer
{
    private Label _itemName;
    private Label _slot;
    private Label _damage;
    private Label _defense;
    private Label _Health;
    private Label _movementSpeed;

    public override void _Ready()
    {
        _itemName = GetNode<Label>("%ItemNameLabel");
        _slot     = GetNode<Label>("%SlotLabel");
        _damage   = GetNode<Label>("%DamageLabel");
        _defense  = GetNode<Label>("%DefenseLabel");
        _Health   = GetNode<Label>("%HealthLabel");
        _movementSpeed    = GetNode<Label>("%MovementSpeedLabel");

    }

    public void ShowTooltip(Item item, Vector2 position)
    {
        _itemName.Text = item.ItemName;
        _slot.Text     = item.Slot;

        SetStatLabel(_damage,  "Damage",  item.Damage,  item.DamageMin,  item.DamageMax);
        SetStatLabel(_defense, "Defense", item.Defense, item.DefenseMin, item.DefenseMax);
        SetStatLabel(_Health,  "Health",  item.Health,  item.HealthMin,  item.HealthMax);
        SetStatLabel(_movementSpeed,   "Speed",   item.MovementSpeed,   item.MovementSpeedMin,   item.MovementSpeedMax);

        GlobalPosition = position + new Vector2(8, 8);
        Visible = true;
    }

    // Shows e.g. "Defense: 5  (2-6)"
    private void SetStatLabel(Label label, string statName, float value, float min, float max)
    {
        bool hasStat = max > 0;
        label.Visible = hasStat;
        if (hasStat)
            label.Text = $"{statName}: {value}  ({min}-{max})";
    }

    public void HideTooltip() => Visible = false;
}