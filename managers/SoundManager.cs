using System;
using Godot;

namespace Managers;

public partial class SoundManager : Node
{
    public static SoundManager Instance { get; private set; }

    private AudioStreamPlayer2D damageBlockedSound;

    public override void _Ready()
    {
        Instance = this;

        damageBlockedSound = GetNode<AudioStreamPlayer2D>("DamageBlockedSound");
        DamageManager.Instance.AttackBlocked += OnAttackBlocked;
    }

    private void OnAttackBlocked(CharacterBody2D source, CharacterBody2D target)
    {
        damageBlockedSound.Play((float)0.19);
        damageBlockedSound.VolumeDb = -15;
    }

}
