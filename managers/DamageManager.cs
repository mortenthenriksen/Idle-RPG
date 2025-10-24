using Godot;
using Components;

namespace Managers;

public partial class DamageManager : Node
{
    [Signal]
    public delegate void DamageDealtEventHandler(CharacterBody2D soruce, CharacterBody2D target, float DamageAmount);

    public static DamageManager Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }

    public static void ApplyDamage(CharacterBody2D source, CharacterBody2D target, float DamageAmount)
    {
        if (target == null)
            return;

        var healthNode = target.GetNodeOrNull<HealthNode>("HealthNode");
        if (healthNode != null)
        {
            healthNode.ApplyDamage(DamageAmount);
            Instance.EmitSignal("DamageDealt", source, target, DamageAmount);
        }
    }
}

