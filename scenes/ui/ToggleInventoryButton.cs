using Godot;

namespace Inventory;

public partial class ToggleInventoryButton : Button
{

    private Inventory inventory;
    private bool isVisble = false;

    public override void _Ready()
    {
        inventory = GetNode<Inventory>("%Inventory");
        this.Pressed += ToggleInventoryVisibility;
        inventory.Visible = isVisble;
    }
    
    private void ToggleInventoryVisibility()
    {
        isVisble = !isVisble;
        inventory.Visible = isVisble;
    }

}
