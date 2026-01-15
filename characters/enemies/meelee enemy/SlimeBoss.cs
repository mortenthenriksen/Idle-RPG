using Components;
using Godot;
using Managers;

namespace Characters;

public partial class SlimeBoss : Enemy
{
    private float DamageAmount = 7;
    private bool playerInRange = false;
    private bool hasSpawned = false;

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
        healthNode.HealthChanged += OnHealthChanged;
    }

    private void OnAttackTimerTimeout()
    {
        if (!playerInRange) return;
        animatedSprite2D.Play("cleave");
    }

    private void OnHealthChanged(float newHealth, float maxHealth)
    {
        if (newHealth > 0)
        {
            var hurtColor = new Color("#de2200");
            animatedSprite2D.Modulate = hurtColor;
        }
    }
    // testing email, again, for the last time
    private void OnFrameChanged()
    {
        var regularColor = new Color("#FFFFFF");
        animatedSprite2D.Modulate = regularColor;

        if (animatedSprite2D.Animation == "cleave" && animatedSprite2D.Frame == 6)
        {
            var bodies = area2D.GetOverlappingBodies();
            foreach (var body in bodies)
            {
                if (body is Player player)
                {
                    // also make this damageamount come from the damagemanager
                    DamageManager.Instance.ApplyDamage(this, player);
                }
            }
        }
    }
    
    private void OnAnimatedSprite2DAnimationFinished()
    {
        if (animatedSprite2D.Animation == "cleave")
        {
            animatedSprite2D.Play("idle");
            attackTimer.Start();
        }
        if (animatedSprite2D.Animation == "death")
        {
            QueueFree();
        }
        if (animatedSprite2D.Animation == "spawn")
        {
            hasSpawned = true;
            animatedSprite2D.Play("run");
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
    
    private void OnDeath(CharacterBody2D characterBody2D)
    {
        attackTimer.Stop();
        
        var collision = GetNode<CollisionShape2D>("CollisionShape2D");
        collision.Disabled = true;

        SetPhysicsProcess(false);
        SetProcess(false);
        playerInRange = false;

        animatedSprite2D.Play("death");
    }   

    
    public override void _PhysicsProcess(double delta)
    {
        if (!playerInRange && hasSpawned)
        {
            Velocity = new Vector2((float)GlobalSettings.MeeleeEnemyMovementSpeed, Velocity.Y);
            MoveAndSlide();
        }
    }
}
