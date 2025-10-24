using Autoload;
using Components;
using Godot;
using Managers;

namespace Characters;

public partial class MeeleeSkeleton : Enemy
{
    private float DamageAmount = 1;
    private bool playerInRange = false;

    private Area2D area2D;
    private AnimatedSprite2D animatedSprite2D;
    private HealthNode healthNode;
    private Timer attackTimer;

    public override void _Ready()
    {
        base._Ready();

        animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        attackTimer = GetNode<Timer>("AttackTimer");
        area2D = GetNode<Area2D>("Area2D");
        healthNode = GetNode<HealthNode>("HealthNode");
        healthNode.Died += OnDeath;
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
                    DamageManager.ApplyDamage(this, player, DamageAmount);
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
        if (animatedSprite2D.Animation == "death")
        {
            QueueFree();
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
    
    protected override void OnDeath(CharacterBody2D characterBody2D)
    {
        base.OnDeath(characterBody2D);

        animatedSprite2D.Play("death"); // your death animation name

        SetPhysicsProcess(false);
        SetProcess(false);

        var collision = GetNode<CollisionShape2D>("CollisionShape2D");
        collision.Disabled = true;
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
