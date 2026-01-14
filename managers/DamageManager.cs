using Godot;
using Components;
using Characters;
using Autoload;

namespace Managers;

public partial class DamageManager : Node
{
    [Signal]
    public delegate void DamageDealtEventHandler(CharacterBody2D source, CharacterBody2D target, float DamageAmount);

    [Signal]
    public delegate void AttackBlockedEventHandler(CharacterBody2D source, CharacterBody2D target);

    private float playerDamage;
    private float basePlayerDamage = 1;

    public static DamageManager Instance { get; private set; }
    private Enemy enemy;
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

        playerDamage = basePlayerDamage;

        playerAttackTimer = player.GetNode<Timer>("AttackTimer");
        playerAttackTimer.WaitTime = baseAttackWaitTime;
    }

    public void ApplyDamage(CharacterBody2D source, CharacterBody2D target, float damage)
    {
        var healthNode = target.GetNodeOrNull<HealthNode>("HealthNode");
        if (player.GetIsBlocking())
        {
            EmitSignal("AttackBlocked", source, target);
            return;
        }
        healthNode.ApplyDamage(damage);
        EmitSignal("DamageDealt", source, target, damage);
    }

    public void AdditiveIncreasePlayerDamage(float increaseDamageValue)
    {
        basePlayerDamage += increaseDamageValue;
        playerDamage = basePlayerDamage;
    }

    public void MultiplicativeIncreasePlayerDamage(float increaseDamageValue)
    {
        playerDamage = basePlayerDamage * increaseDamageValue;
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

