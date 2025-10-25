using Godot;

namespace Managers;

public partial class WaveManager : Node
{

    public static WaveManager Instance { get; private set; }

    public int currentWave { get; private set; }
    public int maxWave { get; private set; }

    public override void _Ready()
    {
        Instance = this;
        currentWave = 0;
        maxWave = 10;
    }

    public void IncreaseWaveCounter()
    {
        currentWave += 1;
        currentWave %= maxWave;
    }
}
