using Components;
using Godot;
using Managers;

namespace Characters;

public partial class MouseClicker : Node2D
{
    private Area2D area2D;
    private CollisionShape2D collisionShape2D;

    public override void _Ready()
    {
        area2D = GetNode<Area2D>("Area2D");
        collisionShape2D = GetNode<CollisionShape2D>("Area2D/CollisionShape2D");
    }


    public Enemy GetEnemy()
    {
        return GetTree().GetFirstNodeInGroup("enemy") as Enemy;        
    }

    public override void _Input(InputEvent @event)
    {
        Position = GetGlobalMousePosition();

        if (@event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
        {
            var enemy = GetEnemy();     
            if (enemy == null) return;
            var enemyArea2D = enemy.GetNode<Area2D>("Area2D");
            var overlapping = area2D.GetOverlappingAreas();
            var healthNode = enemy.GetNode<HealthNode>("HealthNode");

            if (overlapping.Contains(enemyArea2D) && !healthNode.isDying)
            {
                var player = GetTree().GetFirstNodeInGroup("player") as Player;
                if (player != null)
                    DamageManager.Instance.ApplyDamage(player, enemy, false);
            }
        }
    }
}
