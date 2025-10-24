using Godot;

namespace Main;

public partial class Main : Node
{
	public override void _Ready()
	{

	}
	
	public override void _Input(InputEvent @input)
	{
		if (@input.IsActionPressed("escape"))
		{
			GetTree().Quit();
		}
	}
}
