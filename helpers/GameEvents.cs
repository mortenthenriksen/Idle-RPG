
using Godot;

namespace Autoload;

public partial class GameEvents : Node
{

    public static GameEvents Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }

}
