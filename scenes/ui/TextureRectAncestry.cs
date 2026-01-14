using Godot;
using System.Collections.Generic;

namespace UI;

public partial class TextureRectAncestry : TextureRect
{

    public override void _Ready()
    {
        // Make sure this control has tooltip text so the engine requests the custom tooltip.
        // TooltipText = "Custom tooltip text";
    }

    // public override Control _MakeCustomTooltip(string text)
    // {
    //     var label = new Label();
    //     label.Text = text;
    //     return label;
    // }


}
