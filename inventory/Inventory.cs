using Godot;

namespace Inventory;

public partial class Inventory : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// PrintChildren(this);
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
        
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
}
