using System.Collections.Generic;
using Godot;

namespace UI;

public partial class Ancestry : Control
{
    [Export]
    private int honorOfAncestors;
    private ScrollContainer scrollContainer;

    public static Ancestry Instance { get; private set; }

    // name of the ancestor, stat they give, current level, max level
    private Dictionary<string, (float, int, int)> ancestryDict = new Dictionary<string, (float, int, int)>();

    public override void _Ready()
    {   
        Instance = this;
        ConnectSignalsToTextureRects(this);
        scrollContainer = GetNode<ScrollContainer>("MainPanel/ScrollContainer");
        scrollContainer.SetDeferred("scoll_vertical", 158);
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
            GD.Print($"Clicked on: {textureRect.Name}");
        }
    }

    private void CreateAncestryDict()
    {
        ancestryDict.Add("RedGuy",(1,0,10));
    }

    public Dictionary<string, (float, int, int)> GetAncestryDict() => ancestryDict;
}
