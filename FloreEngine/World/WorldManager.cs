using FloreEngine.Diagnostics;
using FloreEngine.Rendering;
using FloreEngine.Utils;
using Silk.NET.Maths;
using System.Collections.Concurrent;
using System.Numerics;

namespace FloreEngine.World;

internal class WorldManager : IDisposable
{
    private static readonly Lazy<WorldManager> _instance = new Lazy<WorldManager>(() => new WorldManager());
    public static WorldManager Instance => _instance.Value;

    public readonly Dictionary<Vector3, Chunk> RenderedChunks;
    public readonly ConcurrentDictionary<(Vector3, int), Chunk> LoadedChunks;

    public static bool centerWorldGen = true;

    internal Vector3 centerPos = Camera.Instance.Position;

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

        RenderedChunks = new Dictionary<Vector3, Chunk>();
        LoadedChunks = new ConcurrentDictionary<(Vector3, int), Chunk>();
    }

    public unsafe void Update()
    {
        List<Chunk> toFill = new List<Chunk>();
        List<Chunk> toRender = new List<Chunk>();

        if (centerWorldGen)
            centerPos = Controller.ChunkPos;

        int oldLength = RenderedChunks.Count;
        for(int lod = 0; lod <= MaxLOD; lod++)
        {
            int chunkSize = Chunk.Size << lod;

            for (int y = -RenderDistance; y < RenderDistance; y++)
                for (int z = -RenderDistance; z < RenderDistance; z++)
                    for (int x = -RenderDistance; x < RenderDistance; x++)
                    {
                        Vector3 playerPos = centerPos; // TODO: Player pos & unloading
                        Vector3 position = playerPos + (new Vector3(x, y, z) * chunkSize);

                        if (RenderedChunks.TryGetValue(position, out Chunk? chunk)) continue;

                        if (LoadedChunks.TryGetValue((position, lod), out chunk))
                        {
                            chunk.hasFeatures = true;
                        }
                        else
                        {
                            chunk = new Chunk(position, lod, true);
                            toFill.Add(chunk);
                        }

                        toRender.Add(chunk);

                        RenderedChunks[position] = chunk;
                    }
        }

        FillChunks(toFill);
        CreateChunkRendering(toRender);
        UnloadFarChunks();
    }

    private static void CreateChunkRendering(List<Chunk> chunks)
    {
        Parallel.ForEach(chunks, chunk => {
            if (chunk.Mesh == null)
            {
                chunk.MeshChunk();
            }
        });

        foreach (Chunk chunk in chunks)
        {
            chunk.CreateRendering();
        }

        chunks.Clear();
    }

    private static void FillChunks(List<Chunk> chunks)
    {
        Parallel.ForEach(chunks, chunk => {
            if (chunk.Voxels == null)
            {
                chunk.CreateBaseTerrain();
            }
            if (chunk.hasFeatures)
            {
                chunk.CreateFeatures();
            }
        });

        chunks.Clear();
    }

    private void UnloadFarChunks()
    {
        foreach (var v in RenderedChunks)
        {
            if (MathUtils.OutOfDistance(v.Key, centerPos, RenderDistance * v.Value.WorldSize)) 
                RenderedChunks.Remove(v.Key);
        }

        foreach (var v in LoadedChunks)
        {
            if (MathUtils.OutOfDistance(v.Key.Item1, centerPos, (RenderDistance+1) * v.Value.WorldSize))
                LoadedChunks.Remove(v.Key, out _);
        }
    }

    public ushort GetVoxelAtWorldPos(int x, int y, int z, int level)
    {
        int scale = 1 << level;
        int chunkSize = Chunk.Size * (1 << level);

        Vector3 worldTilePos = new Vector3(x, y, z);
        Vector3 localTilePos = MathUtils.WorldToTilePosition(worldTilePos / scale);
        Vector3 chunkPos = MathUtils.WorldToChunkCoord(worldTilePos, chunkSize);

        if (LoadedChunks.TryGetValue((chunkPos, level), out Chunk? c))
        {
            if (c == null || c.WorldSize != chunkSize || c.Voxels == null) return 0;
        }
        else
        {
            c = new Chunk(chunkPos, level, false);
            c.CreateBaseTerrain();
            LoadedChunks[(chunkPos, level)] = c;
        }

        return c != null ? c.Voxels[Chunk.Index((int)localTilePos.X, (int)localTilePos.Y, (int)localTilePos.Z)] : (ushort) 0;

        // return 0;
    }

    public void Dispose()
    {
        foreach (Chunk chunk in RenderedChunks.Values)
            chunk.Dispose();

        RenderedChunks.Clear();
        LoadedChunks.Clear();

        GC.SuppressFinalize(this);
    }
}
