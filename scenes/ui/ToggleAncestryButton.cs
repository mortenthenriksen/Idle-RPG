using Godot;

namespace UI;

public partial class ToggleAncestryButton : Button
{
    private Ancestry ancestry;
    private bool isVisble = false;

    public override void _Ready()
    {
        ancestry = GetNode<Ancestry>("%Ancestry");
        this.Pressed += ToggleStatisticsVisibility;
        ancestry.Visible = isVisble;
    }
    
    private void ToggleStatisticsVisibility()
    {
        isVisble = !isVisble;
        ancestry.Visible = isVisble;
    }
}
