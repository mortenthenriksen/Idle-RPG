using Godot;
using Upgrades;

namespace UI;

public partial class ToggleStatsButton : Button
{

    private Statistics statistics;
    private bool isVisble = false;

    public override void _Ready()
    {
        statistics = GetNode<Statistics>("%Statistics");
        this.Pressed += ToggleStatisticsVisibility;
        statistics.Visible = isVisble;
    }
    
    private void ToggleStatisticsVisibility()
    {
        isVisble = !isVisble;
        statistics.Visible = isVisble;
    }

}
