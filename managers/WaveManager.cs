using Godot;

namespace Managers;

public partial class WaveManager : Node
{

    public static WaveManager Instance { get; private set; }

    public int currentWave { get; private set; }
    public int maxWave { get; private set; }
    public int waveDifficulty { get; private set; }

    public override void _Ready()
    {
        Instance = this;
        currentWave = 0;
        maxWave = 1;
        waveDifficulty = 0;
    }

    public void IncreaseWaveCounter()
    {
        currentWave += 1;
        currentWave %= maxWave;
        if (currentWave % maxWave == 0)
        {
            waveDifficulty += 1;
        }
    }

    public int GetWaveDifficulty()
    {
        return waveDifficulty;
    }
}
