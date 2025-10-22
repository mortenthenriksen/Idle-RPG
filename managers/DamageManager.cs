using Godot;
using Components;

namespace Managers;

public partial class DamageManager : Node
{
    public static DamageManager Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }

    public static void ApplyDamage(Node2D target, float amount)
    {
        if (target == null)
            return;

        var healthNode = target.GetNodeOrNull<HealthNode>("HealthNode");
        if (healthNode != null)
        {
            healthNode.ApplyDamage(amount);
        }
    }
}

