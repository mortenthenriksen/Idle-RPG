using System.Linq;
using Components;
using Godot;
using Helpers;
using Managers;
using Upgrades;

namespace Characters; 

public partial class Player : CharacterBody2D
{
	[Export]
	private float animationPlayerSpeedScale = 0.35f;

	private AnimationPlayer animationPlayer;
	private HealthNode healthNode;
    private Area2D area2D;
    private float attacksPerSecond;
	
	private bool enemyInRange = false;
	private bool isBlocking = false;

	// manual attack timer variables
	private float attackCooldown = 0.0f;   // time since last attack
	private float attackInterval = 1.0f;   // seconds between attacks (updated dynamically)

	public override void _Ready()
	{
		AddToGroup(Groups.Player);
		healthNode = GetNode<HealthNode>("HealthNode");
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		area2D = GetNode<Area2D>("Area2D");
		animationPlayer.SpeedScale = animationPlayerSpeedScale;
		animationPlayer.AnimationFinished += OnAnimationFinished;
	}

    public override void _PhysicsProcess(double delta)
    {
        if (!enemyInRange && !(animationPlayer.CurrentAnimation == "attack1") && !isBlocking)
        {
			var playerMovementSpeed = Statistics.Instance.playerStats[Statistics.Traits.MovementSpeed].GetValue();
            Velocity = new Vector2(playerMovementSpeed, Velocity.Y);
            MoveAndSlide();

            if (animationPlayer.CurrentAnimation != "run")
                animationPlayer.Play("run");

            return;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (Input.IsActionPressed("block"))
		{
			animationPlayer.SpeedScale = animationPlayerSpeedScale;
			animationPlayer.Play("block");
			isBlocking = true;
		}
    }


    public override void _Process(double delta)
    {
        if (!enemyInRange)
            return;

	
        attacksPerSecond = Statistics.Instance.playerStats[Statistics.Traits.AttackSpeed].GetValue();
        attackInterval = 1f / attacksPerSecond;

        attackCooldown += (float)delta;

        while (attackCooldown >= attackInterval && !isBlocking)
        {
            attackCooldown -= attackInterval;
            StartAttack();
            DealDamage();
        }
    }

	public void IncreaseMovementSpeed(float percentageIncrease)
	{
		
		var playerMovementSpeed = Statistics.Instance.playerStats[Statistics.Traits.MovementSpeed];
		playerMovementSpeed.AddIncreased(percentageIncrease);
		
		// this is so make the animation fit the movement speed
		animationPlayerSpeedScale = 0.35f * (1 + playerMovementSpeed.GetIncreased().Sum());
		animationPlayer.SpeedScale = animationPlayerSpeedScale;
	}

	private void StartAttack()
	{
		if (attacksPerSecond < 5)
		{
			animationPlayer.SpeedScale = attacksPerSecond / 2;
		}
		else
        {
            animationPlayer.SpeedScale = attacksPerSecond*5;
        }
		animationPlayer.Play("attack1");
	}

	private void OnAnimationFinished(StringName animName)
	{
        animationPlayer.SpeedScale = animationPlayerSpeedScale;
		animationPlayer.Play("idle");
		if (animName == "block")
		{
			isBlocking = false;
			// resets attack cooldown after block
			attackCooldown = 0f;
		}
	}

	private void DealDamage()
	{
		var bodies = area2D.GetOverlappingBodies();
		foreach (var body in bodies)
		{
			if (body is CharacterBody2D target && body.IsInGroup("enemy"))
			{
				DamageManager.Instance.ApplyDamage(this, target);
			}
		}
	}

	private void OnArea2DBodyEntered(Node2D node2D)
	{
		if (node2D is Enemy)
		{
            enemyInRange = true;
            animationPlayer.Play("idle");
		}
	}

	private void OnArea2DBodyExited(Node2D node2D)
	{
		if (node2D is Enemy)
		{
			enemyInRange = false;
		}
	}

	public bool GetIsBlocking() => isBlocking;
}
