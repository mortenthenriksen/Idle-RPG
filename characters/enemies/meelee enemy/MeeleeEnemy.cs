using Components;
using Godot;
using Managers;

namespace Characters;

public partial class MeeleeEnemy : CharacterBody2D
{
    [Signal]
    public delegate void EnemyDiedEventHandler();

    private float DamageAmount = 1;
    private bool playerInRange = false;

    private Area2D area2D;
    private AnimatedSprite2D animatedSprite2D;
    private HealthNode healthNode;
    private Timer attackTimer;

    public override void _Ready()
    {
        animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        attackTimer = GetNode<Timer>("AttackTimer");
        area2D = GetNode<Area2D>("Area2D");
        healthNode = GetNode<HealthNode>("HealthNode");
        healthNode.Connect("Died", new Callable(this, nameof(OnDied)));
    }

    private void OnAttackTimerTimeout()
    {
        if (!playerInRange) return;

        animatedSprite2D.Play("attack1");
    }
    
    private void OnFrameChanged()
    {
        if (animatedSprite2D.Animation == "attack1" && animatedSprite2D.Frame == 6)
        {
            var bodies = area2D.GetOverlappingBodies();
            foreach (var body in bodies)
            {
                if (body is Player player)
                {
                    DamageManager.ApplyDamage(player, DamageAmount);
                }
            }
        }
    }
    
    private void OnAnimatedSprite2DAnimationFinished()
    {
        if (animatedSprite2D.Animation == "attack1")
        {
            animatedSprite2D.Play("idle");
            attackTimer.Start();
        }
    }

    private void OnArea2DBodyEntered(Node2D node2D)
    {
        if (node2D is Player player)
        {
            playerInRange = true;
            animatedSprite2D.Play("idle");
            attackTimer.Start();
        }
    }
    
    private void OnDied()
    {
        animatedSprite2D.Play("death"); // your death animation name
        EmitSignal(SignalName.EnemyDied);

        SetPhysicsProcess(false);
        SetProcess(false);

        var collision = GetNode<CollisionShape2D>("CollisionShape2D");
        collision.Disabled = true;

        animatedSprite2D.AnimationFinished += () =>
        {
            QueueFree();
        };
    }
    
    public override void _PhysicsProcess(double delta)
    {
        if (!playerInRange)
        {
            Velocity = new Vector2((float)GlobalSettings.MeeleeEnemyMovementSpeed, Velocity.Y);
            MoveAndSlide();
        }
    }
}
