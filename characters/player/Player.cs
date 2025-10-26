using Components;
using Godot;
using Managers;

namespace Characters;

public partial class Player : CharacterBody2D
{
	// private Camera2D camera2D;
	private AnimatedSprite2D animatedSprite2D;
	private HealthNode healthNode;
	private Area2D area2D;
	private Timer attackTimer;

	private bool enemyInRange = false;

	public override void _Ready()
	{
		AddToGroup("player");
		healthNode = GetNode<HealthNode>("HealthNode");
		animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

		area2D = GetNode<Area2D>("Area2D");
		attackTimer = GetNode<Timer>("AttackTimer");
	}

	private void OnAttackTimerTimeout()
	{
		if (!enemyInRange) return;

		if (animatedSprite2D.Animation == "attack1" && animatedSprite2D.IsPlaying())
			return; // Don't start another attack while one is in progress

		animatedSprite2D.SpeedScale = DamageManager.Instance.GetPlayerAttackSpeed();
		animatedSprite2D.Play("attack1");
	}
    
    private void OnFrameChanged()
    {
        if (animatedSprite2D.Animation == "attack1" && animatedSprite2D.Frame == 4)
        {
            var bodies = area2D.GetOverlappingBodies();
            foreach (var body in bodies)
            {
                if (body is CharacterBody2D target && body.IsInGroup("enemy"))
                {
					DamageManager.Instance.ApplyDamage(this, target, DamageManager.Instance.GetPlayerDamage());
                }
            }
        }
    }
    
    private void OnAnimatedSprite2DAnimationFinished()
	{
        if (animatedSprite2D.Animation == "attack1")
		{
			animatedSprite2D.SpeedScale = 1.0f;
            animatedSprite2D.Play("idle");
            attackTimer.Start();
        }
    }

	private void OnArea2DBodyEntered(Node2D node2D)
	{
		if (node2D is Enemy enemy)
		{
			enemyInRange = true;
			animatedSprite2D.Play("idle");
			attackTimer.Start();
		}
	}
	
	private void OnArea2DBodyExited(Node2D node2D)
    {
        if (node2D is Enemy enemy)
		{
			enemyInRange = false;
			// change this to movement speed later, so that it doesnt look like the player is slidng
			animatedSprite2D.SpeedScale = 1.0f;
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
		}
	}
}
