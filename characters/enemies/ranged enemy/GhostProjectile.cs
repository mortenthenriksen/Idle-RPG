using Godot;
using Managers;

namespace Characters;

public partial class GhostProjectile : Area2D
{
    private AnimatedSprite2D animatedSprite2D;
    private bool hasHit = false;

    public Enemy Source { get; set; }
    public Player target {get; set; }

    public override void _Ready()
    {
        animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        animatedSprite2D.Play("default");
        animatedSprite2D.AnimationFinished += OnAnimationFinished;
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (hasHit) return;
        if (body is Player player && body.IsInGroup("player"))
        {
            hasHit = true;
            DamageManager.Instance.ApplyDamage(Source, player, false);
        }
    }


    private void OnFrameChanged()
    {
        if (animatedSprite2D.Animation == "default" && animatedSprite2D.Frame == 6)
        {
            var bodies = this.GetOverlappingBodies();
            foreach (var body in bodies)
            {
                if (body is Player player)
                {
                    DamageManager.Instance.ApplyDamage(Source, player, false);
                }
            }
        }
    }

    private void OnAnimationFinished()
    {
        QueueFree();
    }
}