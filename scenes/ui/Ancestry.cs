using System;
using System.Collections.Generic;
using Godot;

namespace Upgrades;

public partial class Ancestry : Control
{
    [Signal]
    public delegate void AncestryUpdatedEventHandler(string nameOfAncestor);

    [Export]
    private int honorOfAncestors;
    public static Ancestry Instance { get; private set; }
    private ScrollContainer scrollContainer;

    // name of the ancestor, stat they give, current level, max level
    private Dictionary<string, (Enum, float, int, int)> ancestryDict = new Dictionary<string, (Enum,float, int, int)>();
    private Dictionary<Statistics.Traits, float>  accumulatedAncestorStats = new Dictionary<Statistics.Traits, float>(); 
    

    public override void _Ready()
    {   
        Instance = this;
        ConnectSignalsToTextureRects(this);
        scrollContainer = GetNode<ScrollContainer>("MainPanel/ScrollContainer");
        scrollContainer.SetDeferred("scoll_vertical", 158);
        CreateAncestryDict();
    }

    private void ConnectSignalsToTextureRects(Node node)
    {
        foreach (var child in node.GetChildren())
        {
            if (child is TextureRect textureRect)
            {
                textureRect.MouseEntered += () => OnMouseExitedTextureRect(textureRect);
                textureRect.MouseExited += () => OnMouseEnteredTextureRect(textureRect);
                textureRect.GuiInput += (input) => OnTextureRectGuiInput(textureRect, input);  // Add this
            }
            if (child.Name != null)
            {
                ConnectSignalsToTextureRects(child);
            }
        }
    }

    private void OnMouseExitedTextureRect(TextureRect textureRect)
    {
        // GD.Print("Exited");
    }

    private void OnMouseEnteredTextureRect(TextureRect textureRect)
    {
        // foreach (var child in textureRect.GetChildren())
        // {
        //     GD.Print(child.Name);
        // }
    }

    private void OnTextureRectGuiInput(TextureRect textureRect, InputEvent input)
    {
        if (input is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            string nameOfAncestor = textureRect.Name;
            UpdateAncestryDictValues(nameOfAncestor);
            UpdateAccumulatedStats();
            EmitSignal(SignalName.AncestryUpdated, nameOfAncestor);
        }
    }

    private void CreateAncestryDict()
    {
        ancestryDict.Add("AD1339_1",(Statistics.Traits.Damage,15,1,10));
        ancestryDict.Add("AD1339_2",(Statistics.Traits.ExperienceGained,20,1,10));
        ancestryDict.Add("AD1339_3",(Statistics.Traits.Life,100,1,10));
        ancestryDict.Add("AD1339_4",(Statistics.Traits.MovementSpeed,1,1,10));
        ancestryDict.Add("AD1339_5",(Statistics.Traits.Damage,10,1,10));
    }

    private void UpdateAncestryDictValues(string nameOfAncestor)
    {
        (Enum, float, int, int) values = ancestryDict[nameOfAncestor];
        // max level 
        if (values.Item3 == values.Item4) return;
        else
        {
            values.Item3 += 1;
            ancestryDict[nameOfAncestor] = values;
        }
        UpdateAccumulatedStats();
    }

    private Dictionary<Statistics.Traits, float> UpdateAccumulatedStats()
    {
        // Initialize a dictionary with 0 for all stats
        accumulatedAncestorStats = new Dictionary<Statistics.Traits, float>();
        foreach (Statistics.Traits stat in Enum.GetValues(typeof(Statistics.Traits)))
        {
            accumulatedAncestorStats[stat] = 0f;
        }

        foreach (var entry in ancestryDict.Values)
        {
            // Cast the Enum back to our specific Stats type
            Statistics.Traits statType = (Statistics.Traits)entry.Item1;
            float value = entry.Item2;
            int currentLevel = entry.Item3;

            accumulatedAncestorStats[statType] += value * currentLevel;
        }
        return accumulatedAncestorStats;
    }

    public Dictionary<Statistics.Traits, float> GetAncestryDictValues() => accumulatedAncestorStats;
}
