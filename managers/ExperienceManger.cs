using Characters;
using Godot;

namespace Managers;

public partial class ExperienceManger : Node
{
    public static ExperienceManger Instance { get; private set; }

    // this should also not be ulong, but something much bigger
    public ulong currentExp { get; private set; }
    public ulong maxExp { get; private set; }

    public override void _Ready()
    {
        Instance = this;
        currentExp = 0;
        maxExp = 100;
    }

    public void GainExp(Enemy enemy)
    {
        currentExp += 10;
        currentExp %= maxExp;
    }

}
