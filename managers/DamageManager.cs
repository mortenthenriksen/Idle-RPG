using Godot;
using Components;
using Characters;

namespace Managers;

public partial class DamageManager : Node
{
    [Signal]
    public delegate void DamageDealtEventHandler(CharacterBody2D soruce, CharacterBody2D target, float DamageAmount);

    public static DamageManager Instance { get; private set; }

    private Enemy enemy;

    private float playerDamage = 1;
    private Player player;
    private Timer playerAttackTimer;

    private float baseAttackWaitTime = 0.75f;       
    private float totalAttackSpeedBonus = 0f;    

    private float lastCalculatedDPS = 0f;

    public override void _Ready()
    {
        Instance = this;
        player = GetNode<Player>("/root/Main/Player");
        enemy = GetTree().GetFirstNodeInGroup("enemy") as Enemy;

        playerAttackTimer = player.GetNode<Timer>("AttackTimer");
        playerAttackTimer.WaitTime = baseAttackWaitTime;
    }

    public void ApplyDamage(CharacterBody2D source, CharacterBody2D target, float damage)
    {
        if (target == null || !IsInstanceValid(target))
            return;

        var healthNode = target.GetNodeOrNull<HealthNode>("HealthNode");
        if (healthNode == null || healthNode.IsDead)
            return;

        healthNode.ApplyDamage(damage);
        EmitSignal("DamageDealt", source, target, damage);
    }

    public void IncreasePlayerDamage(float increaseDamageValue)
    {
        playerDamage += increaseDamageValue;
    }

    public void IncreasePlayerAttackSpeed(float percentageIncrease)
    {
        totalAttackSpeedBonus += percentageIncrease;

        float newWaitTime = baseAttackWaitTime / (1f + totalAttackSpeedBonus);

        playerAttackTimer.WaitTime = newWaitTime;
    }

    public float GetPlayerDamage() => playerDamage;
    public float GetPlayerAttackSpeed() => (float)(1 / playerAttackTimer.WaitTime);
}

