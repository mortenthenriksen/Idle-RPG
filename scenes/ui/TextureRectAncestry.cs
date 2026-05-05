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

    public override Control _MakeCustomTooltip(string text)
    {
        var label = new Label();
        var ancestryDict = Ancestry.Instance.GetAncestryDict();
        if (ancestryDict.TryGetValue(Name, out var value))
        {
            label.Text = $"Increases {value.Trait} with {(int)(value.AmountPerLevel * 100)}%" + 
            $"\nTotal: {(int)(value.AmountPerLevel * value.CurrentLevel * 100)}%";
        }

        return label;
    }
}