using System.Collections.Generic;
using Godot;

namespace Upgrades;

public partial class Ancestry : Control
{
    [Signal]
    public delegate void AncestryUpdatedEventHandler(AncestryEntry ancestryEntry);

    [Export]
    private int honorOfAncestors;
    public static Ancestry Instance { get; private set; }
    private ScrollContainer scrollContainer;

    // trait, trait amount per level, current level, max level
    // make this into a structure instead
    // private Dictionary<string, (Statistics.Traits, float, float, float)> ancestryDict = new Dictionary<string, (Statistics.Traits,float, float,float)>();
    private Dictionary<string, AncestryEntry> ancestryDict = new Dictionary<string, AncestryEntry>();
    
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
                textureRect.GuiInput += (input) => OnTextureRectGuiInput(textureRect, input);  
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
        if (input is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
        {  
            string nameOfAncestorTexture = textureRect.Name;
            AncestryEntry ancestryEntry = ancestryDict.GetValueOrDefault(nameOfAncestorTexture);

            if (ancestryEntry.CurrentLevel == ancestryEntry.MaxLevel) return;

            ancestryEntry.CurrentLevel += 1;
            Statistics.Instance.playerStats[ancestryEntry.Trait].AddIncreased(ancestryEntry.AmountPerLevel);

            // update UI
            Label label = (Label)textureRect.GetChild(0);
            label.Text = $"{ancestryEntry.CurrentLevel}/{ancestryEntry.MaxLevel}";

            EmitSignal(SignalName.AncestryUpdated, ancestryEntry);  
        }
    }

    private void CreateAncestryDict()
    {
        // Trait, damage per level, (current level not part of constructor), max level
        ancestryDict.Add("AD1339_1", new AncestryEntry(Statistics.Traits.Damage, 0.15f,10));
        ancestryDict.Add("AD1339_2", new AncestryEntry(Statistics.Traits.ExperienceGained,0.05f,10));
        ancestryDict.Add("AD1339_3", new AncestryEntry(Statistics.Traits.Health,0.05f,10));
        ancestryDict.Add("AD1339_4", new AncestryEntry(Statistics.Traits.MovementSpeed,0.05f,10));
    }

    public Dictionary<string, AncestryEntry> GetAncestryDict() => ancestryDict;

    // remove all the nodes?
    public override void _ExitTree()
    {
        foreach (var entry in ancestryDict.Values)
            entry.Free();
        
        ancestryDict.Clear();
    }
}
