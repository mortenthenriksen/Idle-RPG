using Godot;

public partial class GlobalSettings : Node
{
    public static float PlayerMovementSpeed { get; set; } = 85.0f;
    
    public static float MeeleeEnemyMovementSpeed { get; set; } = -60.0f;
    
    public override void _Ready()
    {
        
    }

    public static void SetScrollSpeed(float speed)
    {
        PlayerMovementSpeed = speed;
    }
}