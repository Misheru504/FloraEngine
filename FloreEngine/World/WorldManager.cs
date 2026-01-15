using FloreEngine.Diagnostics;
using FloreEngine.Rendering;
using FloreEngine.Utils;
using System.Numerics;

namespace FloreEngine.World;

internal class WorldManager : IDisposable
{
    private static readonly Lazy<WorldManager> _instance = new Lazy<WorldManager>(() => new WorldManager());
    public static WorldManager Instance => _instance.Value;

    public readonly Dictionary<Vector3, Chunk> ChunkMap;

    public int MaxLOD = 0;
    public int RenderDistance = 4;

    public FastNoise Noise;

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
        // UnloadChunks();

        int oldLength = ChunkMap.Count;
        for(int lod = 0; lod <= MaxLOD; lod++)
        {
            int chunkSize = Chunk.Size << lod;

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
                }
            });

            Parallel.ForEach(ChunkMap.Values, chunk => {
                if (chunk.Mesh == null)
                {
                    chunk.CreateMesh();
                }
            });

            foreach (Chunk chunk in ChunkMap.Values)
            {
                chunk.CreateRendering();
            }
        }
    }

    private void UnloadChunks()
    {
        Vector3 playerPos = Controller.ChunkPos;

        foreach(Vector3 pos in ChunkMap.Keys)
        {
            if(pos.X == pos.Y && pos.Y == pos.Z) Console.WriteLine(MathUtils.ChebyshevDistance(pos, playerPos));
        }
    }

    public ushort GetVoxelAtWorldPos(int x, int y, int z, int chunkSize, int scale)
    {
        Vector3 worldTilePos = new Vector3(x, y, z);
        Vector3 chunk = MathUtils.WorldToChunkCoord(worldTilePos, chunkSize); 

        if (ChunkMap.TryGetValue(chunk, out Chunk? c))
        {
            if (c == null || c.WorldSize != chunkSize || c.Voxels == null) return 0;

            Vector3 localTilePos = MathUtils.WorldToTilePosition(worldTilePos / scale);
            return c.Voxels[Chunk.Index((int) localTilePos.X, (int) localTilePos.Y, (int) localTilePos.Z)];
        }

        return 0;
    }

    public void Dispose()
    {
        foreach (Chunk chunk in ChunkMap.Values)
            chunk.Dispose();

        ChunkMap.Clear();

        GC.SuppressFinalize(this);
    }
}
