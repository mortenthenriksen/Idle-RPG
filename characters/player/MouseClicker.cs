using Godot;

namespace Characters;

public partial class MouseClicker : Node2D
{
    private void OnSlotGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
        {
            
        }
    }
}
