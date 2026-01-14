using System;
using Godot;

namespace Managers;

public partial class ExperienceManager : Node
{
    public static ExperienceManager Instance { get; private set; }

    [Export] 
    public float baseExp = 33f;          // EXP to go from level 1 -> 2
    [Export] 
    public float growthFactor = 1.18f;   
    [Export]
    public int unspentSkillPoints;

    public int currentLevel { get; private set; } = 1;
    public double currentExp { get; private set; } = 0f;

    public override void _Ready()
    {
        Instance = this;
    }

    public float GetExpRequiredForNextLevel()
    {
        // total required to REACH next level from level 1
        // you can also make it "exp for this level only", depends how you're tracking it
        return baseExp * Mathf.Pow(growthFactor, currentLevel - 1);
    }

    public void AddExp(CharacterBody2D enemy)
    {   
        currentExp += GetExperinceFromEnemyDead(enemy);
        var requiredExp = Math.Floor(GetExpRequiredForNextLevel());
        // loop because you might gain multiple levels at once if you kill a boss etc.
        while (currentExp >= requiredExp)
        {
            currentExp -= requiredExp;
            LevelUp();
        }
    }

    private double GetExperinceFromEnemyDead(CharacterBody2D enemy)
    {
        var baseExpFromEnemy = 4;
        var linearIncrease = 1.03 * WaveManager.Instance.waveDifficulty;
        var exp = baseExpFromEnemy + linearIncrease;
        return exp;
    }


    private void LevelUp()
    {
        currentLevel++;

        unspentSkillPoints += 3;

        // TODO: make the exp not rely on ulong, but rather use one that can handle even larger
        UIManager.Instance.UpdateExpUI((ulong)currentExp, (ulong)GetExpRequiredForNextLevel());
    }

    public int GetUnspentSkillPoints()
    {
        return unspentSkillPoints;
    }

    public void DecreaseUnspentSkillPoints()
    {
        unspentSkillPoints += -1;
    }
}
