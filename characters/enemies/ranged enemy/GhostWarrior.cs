using Components;
using Godot;
using Managers;
using Upgrades;

namespace Characters;

public partial class GhostWarrior : Enemy
{
    private enum State { None, Melee, Ranged, Spawning, Dying}
    private State currentState = State.None;
    private bool withinAttackRange = false; 

    private Area2D meleeArea2D;
    private Area2D rangedArea2D;
    private HealthNode healthNode;
    private AnimationPlayer animationPlayer;
    private const float animationPlayerSpeedscale = 0.3f;
    private float attacksPerSecond;
	private float attackCooldown = 0.0f;
	private float attackInterval = 1.0f;

    public override void _Ready()
    {
        base._Ready();

        meleeArea2D  = GetNode<Area2D>("MeleeArea2D");
        rangedArea2D  = GetNode<Area2D>("RangedArea2D");
        healthNode = GetNode<HealthNode>("HealthNode");
        healthNode.Died += OnDeath;
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        currentState = State.Spawning;
        animationPlayer.Play("spawn");
    }

    private void SetAttackState()
    {
        var overlappingRanged = rangedArea2D.GetOverlappingBodies();
        var overlappingMelee = meleeArea2D.GetOverlappingBodies();
        if (overlappingRanged.Count > 0 && !(overlappingMelee.Count > 0))
        {
            withinAttackRange = true;
            currentState = State.Ranged;
        }
        else if (overlappingMelee.Count > 0)
        {
            withinAttackRange = true;
            currentState = State.Melee;
        }
        else
        {
            withinAttackRange = false;
            currentState = State.None;
        }

        // GD.Print(overlappingRanged, overlappingMelee);
        // GD.Print("withinAttackrange: ", withinAttackRange, " ", currentState);
    }

    private void OnAnimationFinished(StringName animName)
    {
        if (animName == "spawn")
        {
            currentState = State.None;
            animationPlayer.Play("idle");
            SetAttackState();
        }
        else if (animName == "attack1" || animName == "attack2")
        {
            currentState = State.None;
            animationPlayer.SpeedScale = animationPlayerSpeedscale;
            animationPlayer.Play("idle");
        }
        else if (animName == "death")
        {
            EmitOnDeathAnimationFinished();
            QueueFree();
        }
    }

    private void OnDeath(CharacterBody2D characterBody2D)
    {
        // clean up crew
        currentState = State.Dying;
        var collision = GetNode<CollisionShape2D>("CollisionShape2D");
        collision.Disabled = true;
        meleeArea2D.Monitoring = false;
        rangedArea2D.Monitoring = false;
        animationPlayer.Play("death");
    }   

    private void StartAttack()
    {
        if (attacksPerSecond < 5)
            animationPlayer.SpeedScale = attacksPerSecond / 2;
        else
            animationPlayer.SpeedScale = attacksPerSecond * 5;

        if (currentState == State.Melee)
        {
            animationPlayer.Play("attack1");
        }
        else if (currentState == State.Ranged)
        {
            animationPlayer.Play("attack2");
        }
    }
    
    private void DealDamage()
	{   
        // this is only called for a meelee attack else FireProjectile is called
		var bodies = meleeArea2D.GetOverlappingBodies(); 
		foreach (var body in bodies)
		{
			if (body is CharacterBody2D target && body.IsInGroup("player"))
                // isCrit = false, currently, enemies cant crit
				DamageManager.Instance.ApplyDamage(this, target, false);
		}
	}

    private void FireProjectile()
    {
        var projectile = ResourceLoader.Load<PackedScene>("res://characters/enemies/ranged enemy/GhostProjectile.tscn").Instantiate<GhostProjectile>();
        projectile.Source = this;
        projectile.target = GetTree().GetFirstNodeInGroup("player") as Player;
        projectile.GlobalPosition = projectile.target.GlobalPosition; 
        GetParent().AddChild(projectile);
    }
    
    public override void _Process(double delta)
    {
        if (currentState == State.Dying) return;

        attacksPerSecond = Statistics.Instance.enemyStats[Statistics.Traits.AttackSpeed].GetValue();
        attackInterval = 1f / attacksPerSecond;
        attackCooldown += (float)delta;

        if (attackCooldown >= attackInterval)
        {
            attackCooldown = 0f;
            SetAttackState();
            StartAttack();
        }
    }

    
    public override void _PhysicsProcess(double delta)
    {
        if (!withinAttackRange && currentState != State.Spawning && !healthNode.isDying)
        {
            if (animationPlayer.CurrentAnimation != "run") animationPlayer.Play("run");
            var movementSpeed = Statistics.Instance.enemyStats[Statistics.Traits.MovementSpeed].GetValue();
            Velocity = new Vector2((float)movementSpeed, Velocity.Y);
            MoveAndSlide();
        }
        else
        {
            Velocity = new Vector2(0, Velocity.Y);
            MoveAndSlide();
        }
    }

}
