using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using System.Windows.Markup;
using Godot;
using Upgrades;

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
    // trait, gained per level, current level, max level
    private Dictionary<string, (Statistics.Traits, float, float, float)> ancestryDict = new Dictionary<string, (Statistics.Traits,float, float,float)>();
    

    public override void _Ready()
    {   
        Instance = this;
        ConnectSignalsToTextureRects(this);
        scrollContainer = GetNode<ScrollContainer>("MainPanel/ScrollContainer");
        scrollContainer.ScrollVertical = 158;
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
            (Statistics.Traits, float, float, float) value = ancestryDict.GetValueOrDefault(nameOfAncestor);

            if (value.Item3 == value.Item4) return;

            value.Item3 += 1;
            ancestryDict[nameOfAncestor] = value;
            Statistics.Instance.playerStats[value.Item1].AddIncreased(value.Item2);

            Label label = (Label)textureRect.GetChild(0);
            label.Text = $"{value.Item3}/{value.Item4}";

            EmitSignal(SignalName.AncestryUpdated, nameOfAncestor);  
        }
    }

    private void CreateAncestryDict()
    {
        ancestryDict.Add("AD1339_1",(Statistics.Traits.Damage, 0.15f,0,10));
        ancestryDict.Add("AD1339_2",(Statistics.Traits.ExperienceGained,0.05f,0,10));
        ancestryDict.Add("AD1339_3",(Statistics.Traits.Life,0.05f,0,10));
        ancestryDict.Add("AD1339_4",(Statistics.Traits.MovementSpeed,0.05f,0,10));
    }

    public Dictionary<string, (Statistics.Traits, float, float, float)> GetAncestryDict() => ancestryDict;
}
