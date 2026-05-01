using Godot;


public partial class Item : Resource
{
    [Export] public int Id { get; set; }
    [Export] public string ItemName { get; set; }
    [Export] public Texture2D Icon { get; set; }
    [Export] public string Slot { get; set; }  // "Weapon", "Armor", "Boots", etc.
    // stats items can give
    [Export] public float Damage { get; set; }
    [Export] public float Life { get; set; }
    [Export] public float MovementSpeed{ get; set; }
    [Export] public float Defense { get; set; }

    // ranges
    public float DamageMin  { get; set; }
    public float DamageMax  { get; set; }
    public float DefenseMin { get; set; }
    public float DefenseMax { get; set; }
    public float LifeMin  { get; set; }
    public float LifeMax  { get; set; }
    public float MovementSpeedMin   { get; set; }
    public float MovementSpeedMax   { get; set; }
}


