using FloreEngine.Diagnostics;
using FloreEngine.Utils;
using System.Numerics;

namespace FloreEngine.World;

internal class WorldManager : IDisposable
{
    private static readonly Lazy<WorldManager> _instance = new Lazy<WorldManager>(() => new WorldManager());
    public static WorldManager Instance => _instance.Value;

    public readonly Dictionary<Vector3, Chunk> ChunkMap;

    public int MaxLOD = 4;
    public int RenderDistance = 4;

    public FastNoise Noise;
    public const int CHUNK_RESOLUTION = 16;

    public WorldManager()
    {
        Random r = new Random();
        Noise = FastNoise.FromEncodedNodeTree(FastNoise.TREE_METADATA);
        Noise.Seed = r.Next(int.MinValue, int.MaxValue);
        //Noise.Seed = -863112003;
        Logger.Print($"Seed: {Noise.Seed}");

        ChunkMap = new Dictionary<Vector3, Chunk>();
    }

    public unsafe void Update()
    {
        int oldLength = ChunkMap.Count;
        for(int lod = 0; lod <= MaxLOD; lod++)
        {
            int chunkSize = CHUNK_RESOLUTION << lod;

            for (int y = -RenderDistance; y <= RenderDistance + 1; y++)
                for (int z = -RenderDistance; z <= RenderDistance + 1; z++)
                    for (int x = -RenderDistance; x <= RenderDistance + 1; x++)
                    {
                        Vector3 playerPos = Vector3.Zero; // TODO: Player pos & unloading
                        Vector3 position = new Vector3(x, y, z) * chunkSize;

                        if (!ChunkMap.ContainsKey(position))
                        {
                            Chunk c = new Chunk(position, lod);
                            ChunkMap[position] = c;
                        }
                    }
        }

        if(ChunkMap.Count != oldLength)
        {
            Parallel.ForEach(ChunkMap.Values, chunk => {
                if (chunk.Mesh == null)
                {
                    chunk.FillVoxels();
                    chunk.CreateMesh();
                }
            });

            foreach (Chunk chunk in ChunkMap.Values)
            {
                chunk.CreateRendering();
            }
        }
    }

    public void Dispose()
    {
        foreach (Chunk chunk in ChunkMap.Values)
            chunk.Dispose();

        ChunkMap.Clear();

        GC.SuppressFinalize(this);
    }
}
