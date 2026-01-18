using System.Collections.Generic;
using Godot;
using Upgrades;

namespace UI;

public partial class TextureRectAncestry : TextureRect
{

    public override void _Ready()
    {
        // Make sure this control has tooltip text so the engine requests the custom tooltip.
        // TooltipText = "Custom tooltip text";
    }

    public override Control _MakeCustomTooltip(string text)
    {
        var label = new Label();
        Dictionary<string, (Statistics.Traits, float, float, float)> ancestryDict = Ancestry.Instance.GetAncestryDict();
        string textureRectName = this.Name;
        (Statistics.Traits, float, float, float) value = ancestryDict[textureRectName];
        label.Text = $"Increases {value.Item1} with {value.Item2 * 100}% \nTotal: {value.Item2 * value.Item3 * 100}%";
        return label;
    }


}
