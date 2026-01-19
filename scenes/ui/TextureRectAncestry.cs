using System.Collections.Generic;
using Godot;
using Upgrades;

namespace UI;

public partial class TextureRectAncestry : TextureRect
{

    public override void _Ready()
    {
        // Make sure this control has tooltip text so the engine requests the custom tooltip.
        TooltipText = "Custom tooltip text";
    }

    [Export] public PackedScene CustomTooltipScene; 

    public override Control _MakeCustomTooltip(string text)
    {
        var label = new Label();
        var ancestryDict = Ancestry.Instance.GetAncestryDict();
        if (ancestryDict.TryGetValue(this.Name, out var value))
        {
            label.Text = $"Increases {value.Item1} with {(int)(value.Item2 * 100)}%\nTotal: {(int)(value.Item2 * value.Item3 * 100)}%";
        }

        return label;
    }
}