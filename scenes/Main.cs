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

		if (@input.IsActionPressed("fullscreen"))
		{
			GetWindow().Mode = Window.ModeEnum.Fullscreen;
			GetWindow().Size = new Vector2I(1920, 1080);
		}
		
		if (@input.IsActionPressed("windowed"))
		{
			GetWindow().Mode = Window.ModeEnum.Windowed;
			GetWindow().Size = new Vector2I(960, 540);
		}
	}
}
