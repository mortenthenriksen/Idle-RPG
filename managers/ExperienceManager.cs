using System;
using Godot;

namespace Managers;

public partial class ExperienceManager : Node
{
    public static ExperienceManager Instance { get; private set; }

    [Export] public float baseExp = 10f;          // EXP to go from level 1 -> 2
    [Export] public float growthFactor = 1.18f;   

    public int currentLevel { get; private set; } = 1;
    public float currentExp { get; private set; } = 0f;
    private int unspentSkillPoints;

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

        // loop because you might gain multiple levels at once if you kill a boss etc.
        while (currentExp >= GetExpRequiredForNextLevel())
        {
            currentExp -= GetExpRequiredForNextLevel();
            LevelUp();
        }
    }

    private float GetExperinceFromEnemyDead(CharacterBody2D enemy)
    {
        // change this to increase exponentially also i think
        return 10;
    }


    private void LevelUp()
    {
        currentLevel++;

        unspentSkillPoints += 3;

        // // optional: auto-scale enemy waves here
        // WaveManager.Instance.OnPlayerLevelUp(CurrentLevel);

        // optional: update UI
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
