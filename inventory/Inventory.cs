using Godot;

namespace Inventory;

public partial class Inventory : Control
{
	private Item holdingItem = null;
	private Item[] InventoryItems = new Item[21];
	public enum EquipSlot { Shield, Weapon, Helmet, Armor, Pants, Boots, Amulet, Ring1, Ring2 }
	private Item[] EquipmentItems = new Item[9]; // Matches the 5 slots in your image
	
	private PackedScene itemScene; 
	private TextureRect holdingDisplay;

	public override void _Ready()
	{
		// Connect signals to both containers
		ConnectSignalsToInventorySlots(GetNode<GridContainer>("%GridContainer"));
		ConnectSignalsToInventorySlots(GetNode<HBoxContainer>("%HBoxContainer"));

		holdingDisplay = GetNode<TextureRect>("%HoldingDisplay");
		holdingDisplay.Visible = false;

		// Hardcode test items
		InventoryItems[0] = new Item { ItemName = "Gold Armor", Icon = GD.Load<Texture2D>("res://assets/items/equipment/equipable/gold_armour_chest.png") };
		
		// Equip something by default
		GD.Print("Testing", (int)EquipSlot.Boots);
		EquipmentItems[(int)EquipSlot.Boots] = new Item { ItemName = "Obsidian Boots", Icon = GD.Load<Texture2D>("res://assets/items/equipment/equipable//obsidian_boots.png") };

		SyncInventoryUI();
		SyncEquipmentUI();
	}

	public override void _Process(double delta)
	{
		// If we are holding something, make the display follow the mouse
		if (holdingItem != null)
		{
			holdingDisplay.GlobalPosition = GetGlobalMousePosition() + new Vector2(3, 3);
		}
	}

	private void PrintChildren(Node node)
    {
		foreach (var child in node.GetChildren())
		{
			if (node.Name != "GridContainer")
            {
				GD.Print(child.Name);
				if (child.Name != null)
				{
					PrintChildren(child);
				}
            }
        }
    }

	private void SyncInventoryUI()
	{
		var grid = GetNode<GridContainer>("%GridContainer");
		
		// Loop through your data array (size 21)
		for (int i = 0; i < InventoryItems.Length; i++)
		{
			var slotNode = grid.GetChild(i);
			UpdateSlotVisual(slotNode, InventoryItems[i]);
		}
	}

	private void UpdateSlotVisual(Node slotNode, Item item)
	{
		// Find the icon display inside the slot
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

	private void ConnectSignalsToInventorySlots(Node parent)
	{
		foreach (var child in parent.GetChildren())
		{
			if (child is Control slot && slot.Name.ToString().Contains("Slot"))
			{
				slot.GuiInput += (inputEvent) => OnSlotGuiInput(inputEvent, slot);
				GD.Print(slot.Name);
			}
			if (child.GetChildCount() > 0)
			{
				ConnectSignalsToInventorySlots(child);
			}
		}
	}

	private void OnSlotGuiInput(InputEvent @event, Node slot)
	{
		if (@event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
        {
            GetItemAtSlot(slot);
        }
    }

	private void GetItemAtSlot(Node slot)
{
    string nodeName = slot.Name.ToString();
    Item[] targetArray = null;
    int index = -1;

    // --- IDENTIFY THE SLOT ---
    if (nodeName.StartsWith("Slot") && int.TryParse(nodeName.Replace("Slot", ""), out int slotNum))
    {
        targetArray = InventoryItems;
        index = slotNum - 1;
    }
    else
    {
        targetArray = EquipmentItems;
        index = nodeName switch
        {
            "ShieldSlot" => (int)EquipSlot.Shield,
            "WeaponSlot" => (int)EquipSlot.Weapon,
            "HelmetSlot" => (int)EquipSlot.Helmet,
            "ArmorSlot"  => (int)EquipSlot.Armor,
            "PantsSlot"  => (int)EquipSlot.Pants,
            "BootsSlot"  => (int)EquipSlot.Boots,
            "AmuletSlot" => (int)EquipSlot.Amulet,
            "Ring1Slot"  => (int)EquipSlot.Ring1,
            "Ring2Slot"  => (int)EquipSlot.Ring2,
            _ => -1
        };
    }

    if (index == -1 || targetArray == null) return;

    // --- CASE 1: PICKUP ---
    if (holdingItem == null && targetArray[index] != null)
    {
        Item itemToPick = targetArray[index];
        holdingItem = itemToPick;
        targetArray[index] = null;

        // Animate from Slot to Mouse
        AnimateItemToMouse(slot, itemToPick.Icon);
    }
    // --- CASE 2 & 3: PLACE / SWAP ---
    else if (holdingItem != null)
    {
        Item itemToDrop = holdingItem;

        if (targetArray[index] == null) {
            targetArray[index] = itemToDrop;
            holdingItem = null;
        }
        else {
            Item temp = targetArray[index];
            targetArray[index] = itemToDrop;
            holdingItem = temp;
        }

        AnimateItemToSlot(slot, itemToDrop.Icon);
        UpdateMouseCursorIcon(); 
    }
}

	private void SyncEquipmentUI()
	{
		// We search the Equipment side of your UI
		Node equipRoot = GetNode<HBoxContainer>("%HBoxContainer");
		
		// Map the Enum back to the Node Names
		string[] slotNames = { "ShieldSlot", "WeaponSlot", "HelmetSlot", "ArmorSlot", "PantsSlot", "BootsSlot", "AmuletSlot", "Ring1Slot", "Ring2Slot" };

		for (int i = 0; i < EquipmentItems.Length; i++)
		{
			// Find the node by name inside the equipment container
			// 'true' allows recursive search since they are nested in VBoxes
			Node slotNode = equipRoot.FindChild(slotNames[i], true, false);
			
			if (slotNode != null)
			{
				UpdateSlotVisual(slotNode, EquipmentItems[i]);
			}
		}
	}

	private void AnimateItemToSlot(Node slotNode, Texture2D texture)
	{
		var textureRect = slotNode.GetNode<TextureRect>("ItemIcon");
		
		// 1. Create a "Ghost" icon for the animation
		TextureRect ghost = new TextureRect();
		ghost.Texture = texture;
		ghost.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
		ghost.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
		ghost.Size = textureRect.Size;
		
		// 2. Add it to the UI (ensure it's on top of everything)
		AddChild(ghost);
		ghost.GlobalPosition = GetGlobalMousePosition();

		// Hide the real icon while the ghost is moving
		textureRect.Visible = false;

		// 3. Create the Tween
		Tween tween = GetTree().CreateTween();
		
		// Move to slot and fade in/out slightly for effect
		tween.Parallel().TweenProperty(ghost, "global_position", textureRect.GlobalPosition, 0.1f)
			.SetTrans(Tween.TransitionType.Quad)
			.SetEase(Tween.EaseType.Out);
			
		// 4. When finished, show the real icon and delete the ghost
		tween.Finished += () => {
			textureRect.Texture = texture;
			textureRect.Visible = true;
			ghost.QueueFree();
		};
	}

	private void AnimateItemToMouse(Node slotNode, Texture2D texture)
	{
		var textureRect = slotNode.GetNodeOrNull<TextureRect>("ItemIcon");
		if (textureRect == null) return;

		// 1. Create Ghost
		TextureRect ghost = new TextureRect();
		ghost.Texture = texture;
		ghost.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
		ghost.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
		ghost.Size = textureRect.Size;
		ghost.MouseFilter = MouseFilterEnum.Ignore;
		
		// Add to the main Inventory node so it stays on top of UI
		AddChild(ghost);
		ghost.GlobalPosition = textureRect.GlobalPosition;

		// Hide real slot icon immediately
		textureRect.Visible = false;
		// Keep holding display hidden until ghost arrives to prevent "double icons"
		holdingDisplay.Visible = false;

		// 2. The Tween
		// Target position is Mouse + your (3, 3) offset
		Vector2 targetPos = GetGlobalMousePosition() + new Vector2(3, 3);
		
		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(ghost, "global_position", targetPos, 0.05f)
			.SetTrans(Tween.TransitionType.Quad)
			.SetEase(Tween.EaseType.Out);

		// 3. Handover
		tween.Finished += () => {
			ghost.QueueFree();
			UpdateMouseCursorIcon(); // Shows the actual holdingDisplay
			SyncInventoryUI();
			SyncEquipmentUI();
		};
}

	private void UpdateMouseCursorIcon()
	{
		if (holdingItem != null)
		{
			holdingDisplay.Texture = holdingItem.Icon;
			holdingDisplay.Visible = true;
		}
		else
		{
			holdingDisplay.Visible = false;
		}
	}
}

