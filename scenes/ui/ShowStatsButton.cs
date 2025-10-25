using Godot;

namespace UI;

public partial class ShowStatsButton : Button
{

    private Statistics statistics;
    private bool isVisble = true;

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
