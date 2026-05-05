using System.Runtime.InteropServices;
using Components;
using Godot;
using Managers;
using Upgrades;

namespace Characters;

public partial class GhostWarrior : Enemy
{
    private bool playerInRange = false;
    private bool hasSpawned = false;

    private const float animationSpeedScale = 0.3f;
    private const float animationSpeedScaleSpawnDeath = 0.5f;
    private Area2D area2D;
    private HealthNode healthNode;
    private AnimationPlayer animationPlayer;

    private float attacksPerSecond;
	private float attackCooldown = 0.0f;
	private float attackInterval = 1.0f;

    public override void _Ready()
    {
        base._Ready();

        area2D = GetNode<Area2D>("Area2D");
        healthNode = GetNode<HealthNode>("HealthNode");
        healthNode.Died += OnDeath;
        healthNode.HealthChanged += OnHealthChanged;
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        animationPlayer.SpeedScale = animationSpeedScaleSpawnDeath;
        animationPlayer.Play("spawn");
    }

    private void OnHealthChanged(float newHealth, float maxHealth)
    {
        if (newHealth > 0)
        {
            var hurtColor = new Color("#de2200");

        }
    }


    private void OnAnimationFinished(StringName animName)
    {
        if (animName == "spawn")
        {
            hasSpawned = true;
            animationPlayer.SpeedScale = animationSpeedScale; // reset here
            animationPlayer.Play("run");
        }
        else if (animName == "attack1") 
        {
            animationPlayer.SpeedScale = animationSpeedScale; // reset before playing idle
            animationPlayer.Play("idle");
        }
        else if (animName == "attack2")
        {
            animationPlayer.Play("idle");
        }
        else if (animName == "death")
        {
            EmitOnDeathAnimationFinished();
            QueueFree();
        }
    }

    private void OnArea2DBodyEntered(Node2D node2D)
    {
        if (node2D is Player && !healthNode.isDying)
        {
            playerInRange = true;
            animationPlayer.Play("idle");
        }
    }

    
    private void OnDeath(CharacterBody2D characterBody2D)
    {
        var collision = GetNode<CollisionShape2D>("CollisionShape2D");
        collision.Disabled = true;
        area2D.Monitoring = false;
        playerInRange = false;
        animationPlayer.SpeedScale = animationSpeedScaleSpawnDeath;
        animationPlayer.Play("death");
    }   

	private void StartAttack()
	{
		if (attacksPerSecond < 5)
			animationPlayer.SpeedScale = attacksPerSecond / 2;
		else
			animationPlayer.SpeedScale = attacksPerSecond * 5;

		animationPlayer.Play("attack2");
	}
    
    private void DealDamage()
	{
		var bodies = area2D.GetOverlappingBodies();
		foreach (var body in bodies)
		{
			if (body is CharacterBody2D target && body.IsInGroup("player"))
                // isCrit = false, so far enemies cant crit
				DamageManager.Instance.ApplyDamage(this, target, false);
		}
	}

    private void FireProjectile()
    {
        var projectile = ResourceLoader.Load<PackedScene>("res://characters/enemies/ranged enemy/GhostProjectile.tscn").Instantiate<GhostProjectile>();
        projectile.Source = this;
        projectile.target = GetTree().GetFirstNodeInGroup("player") as Player;
        projectile.GlobalPosition = projectile.target.GlobalPosition; // starts exactly on the enemy
        GetParent().AddChild(projectile);
    }
    
	public override void _Process(double delta)
	{
		if (!playerInRange)
			return;
        
		attacksPerSecond = Statistics.Instance.enemyStats[Statistics.Traits.AttackSpeed].GetValue();
		attackInterval = 1f / attacksPerSecond;
		attackCooldown += (float)delta;

		while (attackCooldown >= attackInterval)
		{
			attackCooldown -= attackInterval;
			StartAttack();
			// DealDamage(); // this is handled in a function call in the editor
		}
	}

    
    public override void _PhysicsProcess(double delta)
    {
        if (!playerInRange && hasSpawned && !healthNode.isDying)
        {
            if (animationPlayer.CurrentAnimation != "run") animationPlayer.Play("run");
            var movementSpeed = Statistics.Instance.enemyStats[Statistics.Traits.MovementSpeed].GetValue();
            Velocity = new Vector2((float)movementSpeed, Velocity.Y);
            MoveAndSlide();
        }
    }
}
