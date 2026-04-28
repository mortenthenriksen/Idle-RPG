using Godot;


public partial class Item : Resource
{
    [Export] public string ItemName { get; set; }
    [Export] public Texture2D Icon { get; set; }
}
