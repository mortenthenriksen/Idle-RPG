using Godot;
using Components;
using Characters;

namespace Managers;

public partial class DamageManager : Node
{
    [Signal]
    public delegate void DamageDealtEventHandler(CharacterBody2D soruce, CharacterBody2D target, float DamageAmount);

    public static DamageManager Instance { get; private set; }

    private float playerDamage = 1;
    private Player player;
    private Timer playerAttackTimer;
    private Enemy enemy;

    public override void _Ready()
    {
        Instance = this;
        player = GetNode<Player>("/root/Main/Player");
        enemy = GetTree().GetFirstNodeInGroup("enemy") as Enemy;

        playerAttackTimer = player.GetNode<Timer>("AttackTimer");
    }

    public void ApplyDamage(CharacterBody2D source, CharacterBody2D target, float damage)
    {
        if (target == null || !IsInstanceValid(target))
            return;

        var healthNode = target.GetNodeOrNull<HealthNode>("HealthNode");
        if (healthNode == null || healthNode.IsDead)
            return;

        healthNode.ApplyDamage(damage);
        Instance.EmitSignal("DamageDealt", source, target, damage);
    }


    public void SetPlayerDamage(float increaseDamageValue)
    {
        playerDamage += increaseDamageValue;
    }

    public void SetPlayerAttackSpeed(float percentageIncrease)
    {
        playerAttackTimer.WaitTime /= 1f + percentageIncrease;
    }

    public float GetPlayerDamage()
    {
        return playerDamage;
    }

    public float GetPlayerAttackSpeed()
    {
        return (float)(1/playerAttackTimer.WaitTime);
    }
}

