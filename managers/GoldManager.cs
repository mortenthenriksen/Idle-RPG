using System;
using Characters;
using Godot;

namespace Managers;

public partial class GoldManager : Node
{
    public static GoldManager Instance { get; private set; }

    // this number is not nearly big enough for now it is :)
    public ulong goldValue { get; private set; }
    

    public override void _Ready()
    {
        Instance = this;
        goldValue = 0;
    }

    public void GetGoldFromEnemy(CharacterBody2D characterBody2D)
    {
        goldValue += 10;
    }

}
