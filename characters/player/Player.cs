using Components;
using Godot;
using Managers;

namespace Characters;

public partial class Player : CharacterBody2D
{
	private float DamageAmount = 1;
	private Camera2D camera2D;
	private AnimatedSprite2D animatedSprite2D;
	private HealthNode healthNode;
	private Area2D area2D;
	private Timer attackTimer;

	private bool enemyInRange = false;
	private static Vector2 offsetVector = new Vector2(224, -183);

	public override void _Ready()
	{
		camera2D = GetNode<Camera2D>("Camera2D");
		healthNode = GetNode<HealthNode>("HealthNode");
		animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

		area2D = GetNode<Area2D>("Area2D");
		attackTimer = GetNode<Timer>("AttackTimer");
	}

	private void OnAttackTimerTimeout()
    {
        if (!enemyInRange) return;

		animatedSprite2D.Play("attack1");
    }
    
    private void OnFrameChanged()
    {
        if (animatedSprite2D.Animation == "attack1" && animatedSprite2D.Frame == 3)
        {
            var bodies = area2D.GetOverlappingBodies();
            foreach (var body in bodies)
            {
                if (body is MeeleeEnemy enemy)
                {
					DamageManager.ApplyDamage(enemy, DamageAmount);
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
		// change this to group everywhere, might aswel do it now, to make nice code :)
		if (node2D is MeeleeEnemy player)
		{
			enemyInRange = true;
			animatedSprite2D.Play("idle");
			attackTimer.Start();
		}
	}
	
	private void OnArea2DBodyExited(Node2D node2D)
    {
        if (node2D is MeeleeEnemy player)
		{
			enemyInRange = false;
			animatedSprite2D.Play("run");
			attackTimer.Stop();
		}
    }

	public override void _PhysicsProcess(double delta)
	{
		if (!enemyInRange)
		{
			Velocity = new Vector2((float)GlobalSettings.PlayerMovementSpeed, Velocity.Y);
			MoveAndSlide();
			camera2D.GlobalPosition = (GlobalPosition + offsetVector).Round();
		}
	}

	// public override void _Input(InputEvent @input)
	// {
	// 	if (@input.IsActionPressed("spacebar"))
	// 	{
	// 		enemyInRange = !enemyInRange;
	// 		if (!enemyInRange)
	// 		{
	// 			animatedSprite2D.Play("idle");
	// 		}
	// 		else
	// 		{
	// 			animatedSprite2D.Play("run");
	// 		}
	// 	}
	// }
}
