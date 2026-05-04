using System;
using System.Linq;
using Autoload;
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

	[Export]
    private float cameraSmoothing = 10f;

	private AnimationPlayer animationPlayer;
	private Sprite2D sprite2d;
	private HealthNode healthNode;
    private Area2D area2D;
	private Camera2D camera2D;
    private float attacksPerSecond;
	
	private bool enemyInRange = false;
	private bool isBlocking = false;
	private bool isAutoPlay = true;

	private float attackCooldown = 0.0f;
	private float attackInterval = 1.0f;

	public override void _Ready()
	{
		AddToGroup(Groups.Player);
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		sprite2d = GetNode<Sprite2D>("Sprite2D");
		healthNode = GetNode<HealthNode>("HealthNode");
		healthNode.InitializeHealth(Statistics.Instance.playerStats[Statistics.Traits.Health].GetValue());
		area2D = GetNode<Area2D>("Area2D");
		camera2D = GetNode<Camera2D>("Camera2D");
		animationPlayer.SpeedScale = animationPlayerSpeedScale;
        animationPlayer.AnimationFinished += OnAnimationFinished;
        GameEventsManager.Instance.PlayerMovementSpeedChanged += OnMovementSpeedIncrease;
	}

    public override void _PhysicsProcess(double delta)
	{
		if (Input.IsActionJustPressed("toggle_autoplay"))
			isAutoPlay = !isAutoPlay;

		if (isAutoPlay)
			HandleAutoPlay();
		else
			HandleManualControl();

		MoveAndSlide();
	}

	private void HandleAutoPlay()
	{
		if (enemyInRange || animationPlayer.CurrentAnimation == "attack1" || isBlocking)
		{
			Velocity = new Vector2(0, Velocity.Y);
			return;
		}

		var speed = Statistics.Instance.playerStats[Statistics.Traits.MovementSpeed].GetValue();
		Velocity = new Vector2(speed, Velocity.Y);

		if (animationPlayer.CurrentAnimation != "run")
			animationPlayer.Play("run");
	}

	private void HandleManualControl()
	{
		if (isBlocking || animationPlayer.CurrentAnimation == "attack1")
		{
			Velocity = new Vector2(0, Velocity.Y);
			return;
		}

		var speed = Statistics.Instance.playerStats[Statistics.Traits.MovementSpeed].GetValue();
		float moveX = 0f;

		if (Input.IsActionPressed("move_right"))
		{
			moveX = speed;
			sprite2d.FlipH = false;
			
		}
		else if (Input.IsActionPressed("move_left"))
		{
			sprite2d.FlipH = true;
			moveX = -speed;
		}


		Velocity = new Vector2(moveX, Velocity.Y);

		if (enemyInRange)
			return;

		if (moveX != 0)
		{
			if (animationPlayer.CurrentAnimation != "run")
				animationPlayer.Play("run");
		}
		else
		{
			if (animationPlayer.CurrentAnimation != "idle")
				animationPlayer.Play("idle");
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

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("block"))
		{
			animationPlayer.SpeedScale = animationPlayerSpeedScale;
			animationPlayer.Play("block");
			isBlocking = true;
		}
	}

	public void OnMovementSpeedIncrease(float percentageIncrease)
	{
		var baseSpeed    = Statistics.Instance.playerStats[Statistics.Traits.MovementSpeed].BaseValue;
		var currentSpeed = Statistics.Instance.playerStats[Statistics.Traits.MovementSpeed].GetValue();
		float ratio = (float)(currentSpeed / baseSpeed);

		animationPlayerSpeedScale = 0.35f * ratio;
		animationPlayer.SpeedScale = animationPlayerSpeedScale;
	}
	

	private void StartAttack()
	{
		if (attacksPerSecond < 5)
			animationPlayer.SpeedScale = attacksPerSecond / 2;
		else
			animationPlayer.SpeedScale = attacksPerSecond * 5;

		animationPlayer.Play("attack1");
	}

	private void OnAnimationFinished(StringName animName)
	{
		animationPlayer.SpeedScale = animationPlayerSpeedScale;
		animationPlayer.Play("idle");
		if (animName == "block")
		{
			isBlocking = false;
			attackCooldown = attackInterval;
		}
	}

	private void DealDamage()
	{
		var bodies = area2D.GetOverlappingBodies();
		foreach (var body in bodies)
		{
			if (body is CharacterBody2D target && body.IsInGroup("enemy"))
				DamageManager.Instance.ApplyDamage(this, target);
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
			enemyInRange = false;
	}

	public bool GetIsBlocking() => isBlocking;
	public bool GetIsAutoPlay() => isAutoPlay;
}