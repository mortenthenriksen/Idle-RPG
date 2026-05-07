using Godot;
using Upgrades;
using Managers;
using Components;
using Autoload;
namespace Inventory;

public partial class Inventory : Control
{
    private Item holdingItem = null;
    // item arrays
    private Item[] InventoryItems = new Item[21];
    private Item[] EquipmentItems = new Item[9];

    public enum EquipSlot { Shield, Weapon, Helmet, Chest, Pants, Boots, Amulet, Gloves, Ring }

    private static readonly string[] EquipSlotNodeNames =
        { "ShieldSlot", "WeaponSlot", "HelmetSlot", "ChestSlot", "PantsSlot", "BootsSlot", "AmuletSlot", "GlovesSlot", "RingSlot" };

    private ItemTooltip toolTip;
    private TextureRect holdingDisplay;

    // ─────────────────────────────────────────────
    // Healthcycle
    // ─────────────────────────────────────────────

    public override void _Ready()
    {
        AddToGroup("inventory");
        ConnectSignalsToInventorySlots(GetNode<GridContainer>("%GridContainer"));
        ConnectSignalsToInventorySlots(GetNode<HBoxContainer>("%HBoxContainer"));

        holdingDisplay = GetNode<TextureRect>("%HoldingDisplay");
        holdingDisplay.Visible = false;

        var tooltipScene = GD.Load<PackedScene>("res://scenes/ui/ItemTooltip.tscn");
        toolTip = tooltipScene.Instantiate<ItemTooltip>();
        AddChild(toolTip);
        toolTip.Hide();

        FillInventoryWithTestItems();
        
		foreach (var item in EquipmentItems)
			ApplyItemStats(item);

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
                        toolTip.ShowTooltip(item, slot.GlobalPosition);
                };
                slot.MouseExited += () => toolTip.Hide();
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

        // CASE 1: PICKUP — removing from equipment
		if (holdingItem == null && targetArray[index] != null)
		{
			Item itemToPick = targetArray[index];

			if (IsEquipmentArray(targetArray))
				RemoveItemStats(itemToPick);

			holdingItem = itemToPick;
			targetArray[index] = null;
			AnimateItemToMouse(slot, itemToPick.Icon);
		}
		// CASE 2 & 3: PLACE / SWAP
        else if (holdingItem != null)
        {
            Item itemToDrop = holdingItem;
            bool targetIsEquip = IsEquipmentArray(targetArray);

            if (targetIsEquip)
            {
                string requiredSlot = GetRequiredSlotType(slot.Name.ToString());
                if (requiredSlot != null && itemToDrop.Slot != requiredSlot)
                    return; 
            }

            if (targetArray[index] == null)
            {
                if (targetIsEquip) ApplyItemStats(itemToDrop);
                targetArray[index] = itemToDrop;
                holdingItem = null;
            }
            else
            {
                Item displaced = targetArray[index];
                if (targetIsEquip)
                {
                    RemoveItemStats(displaced);
                    ApplyItemStats(itemToDrop);
                }
                targetArray[index] = itemToDrop;
                holdingItem = displaced;
            }

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

        // checking if a valid index was found, if not then index = -1, which then returns false
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

    private void ApplyItemStats(Item item)
    {
        // just check what the item has then update, this is stupid >:()
        if (item == null) return;
        var stats = Statistics.Instance.playerStats;

        if (item.Damage        != 0) stats[Statistics.Traits.Damage].AddFlat(item.Damage);
        if (item.MovementSpeed != 0) 
        {
            stats[Statistics.Traits.MovementSpeed].AddFlat(item.MovementSpeed);
            GameEventsManager.Instance.EmitOnPlayerMovementSpeedChanged(item.MovementSpeed);
        }
        if (item.Health          != 0)
        {
            stats[Statistics.Traits.Health].AddFlat(item.Health);
            GetPlayerHealthNode()?.GetMaxHealthFromStatsDict();
        }
        if (item.Defense != 0) stats[Statistics.Traits.Defence].AddFlat(item.Defense);
        if (item.AttackSpeed != 0) stats[Statistics.Traits.AttackSpeed].AddIncreased(item.AttackSpeed);

        UIManager.Instance.RefreshPlayerStats(GetPlayerHealthNode());
    }

    private void RemoveItemStats(Item item)
    {
        if (item == null) return;
        var stats = Statistics.Instance.playerStats;

        if (item.Damage        != 0) stats[Statistics.Traits.Damage].RemoveFlat(item.Damage);
        if (item.MovementSpeed != 0) 
        {
            stats[Statistics.Traits.MovementSpeed].RemoveFlat(item.MovementSpeed);
            GameEventsManager.Instance.EmitOnPlayerMovementSpeedChanged(item.MovementSpeed);
        }
        if (item.Health          != 0)
        {
            stats[Statistics.Traits.Health].RemoveFlat(item.Health);
            GetPlayerHealthNode()?.GetMaxHealthFromStatsDict();
        }
        if (item.Defense != 0) stats[Statistics.Traits.Defence].RemoveFlat(item.Defense);
        if (item.AttackSpeed != 0) stats[Statistics.Traits.AttackSpeed].RemoveIncreased(item.AttackSpeed);

        UIManager.Instance.RefreshPlayerStats(GetPlayerHealthNode());
    }

	private HealthNode GetPlayerHealthNode()
	{
		var player = GetTree().GetFirstNodeInGroup("player");
		return player?.GetNodeOrNull<HealthNode>("HealthNode");
	}

	private bool IsEquipmentArray(Item[] array) => array == EquipmentItems;

    private void FillInventoryWithTestItems()
    {
        var testItems = new[]
        {
            // Mix of slots and type levels for variety
            "wooden_gloves_type1_tier1",
            "wooden_gloves_type6_tier1",
            "wooden_helm_type7_tier1", 
            "wooden_pants_type8_tier1",
            "wooden_chest_type6_tier1",
            "wooden_boots_type2_tier1",
            "wooden_sword_large7_tier1"
            // add more item IDs from your CSV here
        };

        int slot = 0;
        foreach (var itemId in testItems)
        {
            if (slot >= InventoryItems.Length) break;

            // Skip already occupied slots
            while (slot < InventoryItems.Length && InventoryItems[slot] != null)
                slot++;

            if (slot >= InventoryItems.Length) break;

            var item = ItemDatabaseManager.Instance.CreateItem(itemId);
            if (item != null)
                InventoryItems[slot++] = item;
        }
    }

    public bool TryAddItem(Item item)
    {
        for (int i = 0; i < InventoryItems.Length; i++)
        {
            if (InventoryItems[i] != null) continue;
            InventoryItems[i] = item;
            SyncInventoryUI();
            return true;
        }
        // fix so the item isnt lost
        GD.Print("[Inventory] Full — drop was lost.");
        return false;
    }


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
            // to find the nodes, it uses FindChild(node, recursive, not owned only), so it works
            // even with nodes that are not directly children
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