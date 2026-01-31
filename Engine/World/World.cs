namespace FloraEngine.World;

public class World
{
    public string Name { get; private set; }
    public int Seed { get; private set; }
    public List<ChunkData> Chunks { get; private set; }
    public Random Random { get; private set; }

    public World(string name, int seed = 0)
    {
        Name = name;
        Seed = seed;
        Chunks = new List<ChunkData>();
        Random = new Random(seed);
    }

    public World(WorldData data)
    {
        Name = data.name;
        Seed = data.seed;
        Chunks = data.chunks.ToList();
        Random = new Random(Seed);
    }
}
