using Godot;

namespace Inventory;

public partial class Inventory : Control
{
    private Item holdingItem = null;
    private Item[] InventoryItems = new Item[21];
    private Item[] EquipmentItems = new Item[9];

    public enum EquipSlot { Shield, Weapon, Helmet, Chest, Pants, Boots, Amulet, Gloves, Ring }

    private static readonly string[] EquipSlotNodeNames =
        { "ShieldSlot", "WeaponSlot", "HelmetSlot", "ChestSlot", "PantsSlot", "BootsSlot", "AmuletSlot", "GlovesSlot", "RingSlot" };

    private ItemTooltip _tooltip;
    private TextureRect holdingDisplay;

    // ─────────────────────────────────────────────
    // Lifecycle
    // ─────────────────────────────────────────────

    public override void _Ready()
    {
        ConnectSignalsToInventorySlots(GetNode<GridContainer>("%GridContainer"));
        ConnectSignalsToInventorySlots(GetNode<HBoxContainer>("%HBoxContainer"));

        holdingDisplay = GetNode<TextureRect>("%HoldingDisplay");
        holdingDisplay.Visible = false;

        var tooltipScene = GD.Load<PackedScene>("res://scenes/ui/ItemTooltip.tscn");
        _tooltip = tooltipScene.Instantiate<ItemTooltip>();
        AddChild(_tooltip);
        _tooltip.Hide();

        InventoryItems[0] = ItemDatabaseManager.Instance.CreateItem("wooden_gloves_type6_tier1");
        InventoryItems[3] = ItemDatabaseManager.Instance.CreateItem("wooden_gloves_type1_tier1");

        SyncInventoryUI();
        SyncEquipmentUI();
    }

    public override void _Process(double delta)
    {
        if (holdingItem != null)
            holdingDisplay.GlobalPosition = GetGlobalMousePosition() + new Vector2(3, 3);
    }

    // ─────────────────────────────────────────────
    // Slot signals
    // ─────────────────────────────────────────────

    private void ConnectSignalsToInventorySlots(Node parent)
    {
        foreach (var child in parent.GetChildren())
        {
            if (child is Control slot && slot.Name.ToString().Contains("Slot"))
            {
                slot.GuiInput += (inputEvent) => OnSlotGuiInput(inputEvent, slot);

                string slotName = slot.Name.ToString();
                slot.MouseEntered += () =>
                {
                    var item = GetItemForSlot(slotName);
                    if (item != null && holdingItem == null)
                        _tooltip.ShowTooltip(item, slot.GlobalPosition);
                };
                slot.MouseExited += () => _tooltip.Hide();
            }

            if (child.GetChildCount() > 0)
                ConnectSignalsToInventorySlots(child);
        }
    }

    private void OnSlotGuiInput(InputEvent @event, Node slot)
    {
        if (@event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
            GetItemAtSlot(slot);
    }

    // ─────────────────────────────────────────────
    // Item interaction
    // ─────────────────────────────────────────────

    private void GetItemAtSlot(Node slot)
    {
        string nodeName = slot.Name.ToString();
        if (!ResolveSlot(nodeName, out var targetArray, out int index)) return;

        // CASE 1: Pickup
        if (holdingItem == null && targetArray[index] != null)
        {
            holdingItem = targetArray[index];
            targetArray[index] = null;
            _tooltip.Hide();
            AnimateItemToMouse(slot, holdingItem.Icon);
        }
        // CASE 2 & 3: Place / Swap
        else if (holdingItem != null)
        {
            string requiredSlot = GetRequiredSlotType(nodeName);
            if (requiredSlot != null && holdingItem.Slot != requiredSlot)
            {
                GD.Print($"Cannot place {holdingItem.ItemName} in {nodeName} — requires {requiredSlot}");
                return;
            }

            Item itemToDrop = holdingItem;
            holdingItem = targetArray[index]; // null if empty, swapped item if occupied
            targetArray[index] = itemToDrop;

            AnimateItemToSlot(slot, itemToDrop.Icon);
            UpdateMouseCursorIcon();
        }
    }

    // Resolves a slot node name to its array + index. Returns false if invalid.
    private bool ResolveSlot(string nodeName, out Item[] array, out int index)
    {
        if (nodeName.StartsWith("Slot") && int.TryParse(nodeName.Replace("Slot", ""), out int slotNum))
        {
            array = InventoryItems;
            index = slotNum - 1;
            return true;
        }

        array = EquipmentItems;
        index = nodeName switch
        {
            "ShieldSlot"  => (int)EquipSlot.Shield,
            "WeaponSlot"  => (int)EquipSlot.Weapon,
            "HelmetSlot"  => (int)EquipSlot.Helmet,
            "ChestSlot"   => (int)EquipSlot.Chest,
            "PantsSlot"   => (int)EquipSlot.Pants,
            "BootsSlot"   => (int)EquipSlot.Boots,
            "AmuletSlot"  => (int)EquipSlot.Amulet,
            "GlovesSlot"  => (int)EquipSlot.Gloves,
            "RingSlot"    => (int)EquipSlot.Ring,
            _             => -1
        };

        return index != -1;
    }

    private Item GetItemForSlot(string nodeName)
    {
        ResolveSlot(nodeName, out var array, out int index);
        return index >= 0 ? array[index] : null;
    }

    private string GetRequiredSlotType(string nodeName)
    {
        return nodeName switch
        {
            "ShieldSlot"  => "Shield",
            "WeaponSlot"  => "Weapon",
            "HelmetSlot"  => "Helmet",
            "ChestSlot"   => "Chest",
            "PantsSlot"   => "Pants",
            "BootsSlot"   => "Boots",
            "AmuletSlot"  => "Amulet",
            "GlovesSlot"  => "Gloves",
            "RingSlot"    => "Ring",
            _             => null
        };
    }

    // ─────────────────────────────────────────────
    // UI sync
    // ─────────────────────────────────────────────

    private void SyncInventoryUI()
    {
        var grid = GetNode<GridContainer>("%GridContainer");
        for (int i = 0; i < InventoryItems.Length; i++)
            UpdateSlotVisual(grid.GetChild(i), InventoryItems[i]);
    }

    private void SyncEquipmentUI()
    {
        Node equipRoot = GetNode<HBoxContainer>("%HBoxContainer");
        for (int i = 0; i < EquipmentItems.Length; i++)
        {
            Node slotNode = equipRoot.FindChild(EquipSlotNodeNames[i], true, false);
            if (slotNode != null)
                UpdateSlotVisual(slotNode, EquipmentItems[i]);
        }
    }

    private void UpdateSlotVisual(Node slotNode, Item item)
    {
        var textureRect = slotNode.GetNode<TextureRect>("ItemIcon");
        if (item != null && item.Icon != null)
        {
            textureRect.Texture = item.Icon;
            textureRect.Visible = true;
        }
        else
        {
            textureRect.Texture = null;
            textureRect.Visible = false;
        }
    }

    private void UpdateMouseCursorIcon()
    {
        holdingDisplay.Texture  = holdingItem?.Icon;
        holdingDisplay.Visible  = holdingItem != null;
    }

    // ─────────────────────────────────────────────
    // Animations
    // ─────────────────────────────────────────────

    private void AnimateItemToSlot(Node slotNode, Texture2D texture)
    {
        var textureRect = slotNode.GetNode<TextureRect>("ItemIcon");

        TextureRect ghost = CreateGhost(texture, textureRect.Size);
        ghost.GlobalPosition = GetGlobalMousePosition();
        textureRect.Visible = false;

        var tween = GetTree().CreateTween();
        tween.Parallel().TweenProperty(ghost, "global_position", textureRect.GlobalPosition, 0.1f)
            .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);

        tween.Finished += () =>
        {
            textureRect.Texture = texture;
            textureRect.Visible = true;
            ghost.QueueFree();
        };
    }

    private void AnimateItemToMouse(Node slotNode, Texture2D texture)
    {
        var textureRect = slotNode.GetNodeOrNull<TextureRect>("ItemIcon");
        if (textureRect == null) return;

        TextureRect ghost = CreateGhost(texture, textureRect.Size);
        ghost.MouseFilter = MouseFilterEnum.Ignore;
        ghost.GlobalPosition = textureRect.GlobalPosition;

        textureRect.Visible = false;
        holdingDisplay.Visible = false;

        var tween = GetTree().CreateTween();
        tween.TweenProperty(ghost, "global_position", GetGlobalMousePosition() + new Vector2(3, 3), 0.05f)
            .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);

        tween.Finished += () =>
        {
            ghost.QueueFree();
            UpdateMouseCursorIcon();
            SyncInventoryUI();
            SyncEquipmentUI();
        };
    }

    private TextureRect CreateGhost(Texture2D texture, Vector2 size)
    {
        var ghost = new TextureRect
        {
            Texture     = texture,
            ExpandMode  = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            Size        = size
        };
        AddChild(ghost);
        return ghost;
    }
}